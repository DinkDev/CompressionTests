namespace CompressionTests
{
    using Microsoft.Extensions.Logging;
    using Stubs;
    using System;
    using System.Collections.Generic;
    //using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    internal class CompleteServerStub
    {
        // just for the nameof reference
        public class CompleteServer { } 

        public CancellationToken CancelToken { get; }

        public CompleteServerStub(CancellationToken cancelToken, Action started)
        {
            CancelToken = cancelToken;
            _uowStub = new UowStub();

            _uowStub.Started += started;
        }

        public SettingsStub Settings { get; } = new SettingsStub();

        public ILogger Logger { get; set; }
        public Func<DateTime> CurrentDateTime { get; set; } = () => DateTime.Now;

        public DateTime LastProcessTime { get; set; }

        private readonly UowStub _uowStub;
        public UowStub UnitOfWorkFactory()
        {
            return _uowStub;
        }

        private DocumentBundleReturned DocumentBundleReturnedFactory()
        {
            return UowStub.BundleReturnedRepoStub.Bundle;
        }

        public CrcStub ChecksumHelper { get; } = new CrcStub();


        public async Task ExecuteAsync()
        {
            LastProcessTime = CurrentDateTime();

            // get the next vastec bundle
            Logger.LogTrace($"{nameof(CompleteServer)}.{nameof(ExecuteAsync)} - Starting at {CurrentDateTime()}");
            var vastecBundle = await GetNextBundleAsync();
            while (!CancelToken.IsCancellationRequested)
            {
                try
                {
                    // continue to create out packages as long as there are more on the out queue
                    var moreDocuments = true;
                    while (moreDocuments)
                    {
                        (vastecBundle, moreDocuments) = await ProcessOutQueuePackagesAsync(vastecBundle, moreDocuments);
                    }

                    // TODO: the following never actually did anything
                    //ProcessResendBundles();

                    //ProcessResendPackages();
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, $"{nameof(CompleteServer)}.{nameof(ExecuteAsync)} - Error at {CurrentDateTime()}");
                }

                Thread.Sleep(Settings.PollInterval);
            }
        }

        public async Task<DocumentBundleReturned> GetNextBundleAsync()
        {
            //var bundle = Group.LoadFirst("vastec bundle", GroupStatusType.PROCESSING) ?? StartNewBundle();

            using (var uow = UnitOfWorkFactory())
            {
                var bundle =
                    (await uow.BundlesReturned.FindAsync(b => b.RecordStatus == ContainerRecordStatus.Processing))
                    .FirstOrDefault();

                if (bundle == null)
                {
                    bundle = DocumentBundleReturnedFactory();
                    bool rv;
                    (rv, bundle) =
                        await uow.BundlesReturned.CreateNewDocumentBundleReturnedAsync(bundle,
                            Settings.WorkingDirectory);

                    if (rv)
                    {
                        bundle.SetRecordStatusProcessing();
                        bundle.SetLastError(string.Empty, CurrentDateTime());

                        (rv, bundle) = await uow.BundlesReturned.UpdateAsync(bundle);
                    }

                    if (!rv)
                    {
                        Logger.LogError($"{nameof(CompleteServer)}.{GetNextBundleAsync()}: Error creating/updating new bundle {bundle.BundleName}");
                    }
                }

                return bundle;
            }
        }

        // was ProcessOutPackages
        private async Task<Tuple<DocumentBundleReturned, bool>> ProcessOutQueuePackagesAsync(DocumentBundleReturned currentBundle, bool moreDocuments)
        {
            moreDocuments = false;

            DocumentPackage package = null;
            var documents = new List<Document>();
            try
            {
                // get docs off [Out] queue
                using (var uow = UnitOfWorkFactory())
                {
                    package = await uow.PopPackageFromQueueAsync(QueueType.Out);

                    if (package != null)
                    {
                        // TODO: may need to update somewhere - be sure to get the result
                        //await uow.Packages.UpdateAsync(package);

                        documents = await uow.Documents
                            .GetAllWithChildrenByPackageIdAsync(package.Id);

                        var documentNames = string.Join(", ", documents.Select(d => d.DocumentKeyId));

                        // TODO: will need to query foe bundleReceived
                        Logger.LogInformation(
                            $"Processing starting - bundle: {package.DocumentBundleReceived.BundleName}, package: {package.PackageName}, application number: {package.ApplicationNumber}, Documents: {documentNames}");
                    }
                }

                try
                {
                    if (documents.Any())
                    {
                        moreDocuments = true;
                        LastProcessTime = CurrentDateTime().AddSeconds(Settings.BundleTimeLimit);

                        Logger.LogDebug($"{nameof(CompleteServer)}.{nameof(ProcessOutQueuePackagesAsync)}: Starting bundle ID: {currentBundle.Id}, Timeout: {LastProcessTime}");

                        // validate we have what we need
                        if (!ValidateDocs(documents, out var errorFileName))
                        {
                            // mark for error and send to error queue
                            package.SetRecordStatusErrored();
                            var errorMessage = $"Documents did not pass validation.  Missing required data for {errorFileName}.";
                            Logger.LogError(errorMessage);

                            package.SetLastError(errorMessage, CurrentDateTime());
                            package.SetRecordStatusErrored();
                            documents.ForEach(d => d.SetDocumentStatusErrored());

                            // add package to queue
                            using (var uow = UnitOfWorkFactory())
                            {
                                if (!await uow.AddPackageToQueueAsync(package, QueueType.Error))
                                {
                                    var message = $"Unable to push package {package.PackageName} to {QueueType.Error} queue.";
                                    Logger.LogError(message);
                                }
                            }

                            return Tuple.Create(currentBundle, moreDocuments);
                        }

                        Logger.LogInformation($"Validated package {package.PackageName}");

                        // Xml validation
                        if (Settings.DoSchemaValidation)
                        {
                            if (!ValidateXml(package))
                            {
                                return Tuple.Create(currentBundle, moreDocuments);
                            }
                        }

                        var packageBytes = await ZipPackageAsync(package);
                        if (packageBytes.Length == 0)
                        {
                            var errorMessage = $"Unable to complete create package: {package.PackageName}, no data Zipped";
                            Logger.LogError(errorMessage);

                            // mark for error and send to error queue
                            package.SetRecordStatusErrored();
                            package.SetLastError(errorMessage, CurrentDateTime());

                            documents.ForEach(d => d.SetDocumentStatusErrored());

                            // add package to queue
                            using (var uow = UnitOfWorkFactory())
                            {
                                if (!await uow.AddPackageToQueueAsync(package, QueueType.Error))
                                {
                                    var message = $"Unable to push package {package.PackageName} to {QueueType.Error} queue.";
                                    Logger.LogError(message);
                                }
                            }
                        }
                        else
                        {
                            // set output file values for package
                            package.ReturnedFileName = FileSystemHelper.GetFileName(GetPackageOutFile(package));

                            // check if already have same file in bundle (even if they send 2
                            // of same name we need to make sure only 1 per bundle)
                            var duplicatePackage = currentBundle.DocumentPackages.SingleOrDefault(p => p.PackageName == package.PackageName);
                            if (duplicatePackage != null)
                            {
                                Logger.LogWarning(
                                    $"pkg OutFileName '{package.PackageName}' (id={package.Id}) already child of curBundle.Id={currentBundle.Id} (duplicate id={duplicatePackage.Id})");

                                using (var uow = UnitOfWorkFactory())
                                {
                                    if (!await uow.AddPackageToQueueAsync(package, QueueType.Out))
                                    {
                                        var message = $"Unable to push package {package.PackageName} (id={package.Id}) to {QueueType.Out} queue.";
                                        Logger.LogError(message);
                                    }
                                }
                            }

                            package.ReturnedFileSize = Convert.ToInt32(packageBytes.Length);
                            package.ReturnedFileChecksum = ChecksumHelper.Compute(packageBytes);

                            // save to work dir
                            await WritePackageWorkFileAsync(package, packageBytes);

                            Logger.LogInformation(
                                $"{nameof(CompleteServer)}.{nameof(ProcessOutQueuePackagesAsync)} : Package {package.PackageName} sent.");

                            package.SetLastError(string.Empty, CurrentDateTime());
                            package.SetRecordStatusSent();

                            // is bundle full
                            if (BundleLimit(currentBundle, package.ReturnedFileSize))
                            {
                                currentBundle = await SendBundleAsync(currentBundle);
                            }

                            currentBundle.DocumentPackages.Add(package);

                            using (var uow = UnitOfWorkFactory())
                            {
                                bool result;
                                (result, currentBundle) = await uow.BundlesReturned.UpdateAsync(currentBundle);
                                if (!result)
                                {
                                    var message =
                                        $"Unable to update bundle {currentBundle.BundleName} (ID={currentBundle.Id}) with package {package.PackageName} (id={package.Id}) to sent.";
                                    Logger.LogError(message);
                                }
                            }
                        }
                    }
                    else
                    {
                        // no packages on [Out] queue

                        // if we have partial bundle ready and waiting past limit then send it
                        if (currentBundle.DocumentPackages.Count > 0 &&
                            CurrentDateTime() > LastProcessTime.AddSeconds(Settings.BundleTimeLimit))
                        {
                            currentBundle = await SendBundleAsync(currentBundle);
                            Logger.LogInformation($"{nameof(CompleteServer)}.{nameof(ProcessOutQueuePackagesAsync)}: Next Bundle timeout: {LastProcessTime.AddSeconds(Settings.BundleTimeLimit)}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, $"{nameof(CompleteServer)}.{nameof(ProcessOutQueuePackagesAsync)}: Error processing documents.");
                    Thread.Sleep(100);

                    package.SetLastError($"Error processing documents: {ex}", CurrentDateTime());
                    package.SetRecordStatusErrored();

                    documents.ForEach(d => d.SetDocumentStatusErrored());

                    // add package to queue
                    using (var uow = UnitOfWorkFactory())
                    {
                        if (!await uow.AddPackageToQueueAsync(package, QueueType.Error))
                        {
                            Logger.LogError($"{nameof(CompleteServer)}.{nameof(ProcessOutQueuePackagesAsync)}: Unable to push package {package.PackageName} to {QueueType.Error} queue.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"{nameof(CompleteServer)}.{nameof(ProcessOutQueuePackagesAsync)}: Error processing documents.");
                Thread.Sleep(1000);
            }

            return Tuple.Create(currentBundle, moreDocuments);
        }

        public async Task<DocumentBundleReturned> SendBundleAsync(DocumentBundleReturned currentBundle)
        {
            Logger.LogTrace($"{nameof(CompleteServer)}.{nameof(SendBundleAsync)}: {currentBundle.ReturnedFileName}");

            // send out current bundle
            currentBundle.SetRecordStatusCompleted();

            await WriteBundleOutFileAsync(currentBundle);
            await SendBundleOutAsync(currentBundle);

            currentBundle.SetRecordStatusSent();

            // start new bundle
            var newBundle = await GetNextBundleAsync();

            Logger.LogInformation($"{nameof(CompleteServer)}.{nameof(SendBundleAsync)}: new bundle started {newBundle.BundleName}");
            return newBundle;
        }

        private bool BundleLimit(DocumentBundleReturned currentBundle, int packageReturnedFileSize)
        {
            return true;
        }

        private async Task WritePackageWorkFileAsync(DocumentPackage package, byte[] packageBytes)
        {
            await Task.FromResult(0);
        }

        //public async Task<bool> WriteBundleOutFileAsync(DocumentBundleReturned bundle)
        //{
        //    return await Task.FromResult(true);
        //}

        public async Task<bool> WriteBundleOutFileAsync(DocumentBundleReturned bundle)
        {
            var rv = false;
            SetBundleTempFile(bundle);

            TarPackages(Settings.WorkingDirectory, bundle.ReturnedFileName);
            if (FileSystemHelper.FileExists(bundle.ReturnedFileName))
            {
                var bytes = await FileSystemHelper.ReadAllBytesAsync(bundle.ReturnedFileName);
                var checksum = ChecksumHelper.Compute(bytes);

                bundle.ReturnedFileChecksum = checksum;
                bundle.ReturnedFileSize = bytes.Length;
                bundle.ReturnedFileName = bundle.ReturnedFileName;

                rv = true;
            }

            return rv;
        }

        private bool ValidateXml(DocumentPackage package)
        {
            return true;
        }

        private bool ValidateDocs(List<Document> documents, out string errorFileName)
        {
            errorFileName = "Unknown";
            return true;
        }

        public async Task<bool> SendBundleOutAsync(DocumentBundleReturned bundle)
        {
            try
            {
                SetBundleTempFile(bundle);

                var source = bundle.ReturnedFileName;
                Logger.LogInformation($"{nameof(CompleteServer)}.{nameof(SendBundleOutAsync)}: bundle file name: {source}");

                if (Settings.SaveOutputFiles)
                {
                    var save = source + ".SENT";
                    if (FileSystemHelper.FileExists(save))
                    {
                        FileSystemHelper.DeleteFile(save);
                    }

                    FileSystemHelper.CopyFile(source, save);
                    Logger.LogInformation(
                        $"{nameof(CompleteServer)}.{nameof(SendBundleOutAsync)}: copied bundle to save: {save}");
                }

                SetBundleFile(bundle);

                using (var uow = UnitOfWorkFactory())
                {
                    bool result;
                    (result, bundle) = await uow.BundlesReturned.UpdateAsync(bundle);
                    if (!result)
                    {
                        Logger.LogError($"{nameof(CompleteServer)}.{nameof(SendBundleOutAsync)}: unable to update bundle {bundle.BundleName} (id={bundle.Id}) to sent.");
                    }
                }

                var dest = bundle.ReturnedFileName;

                if (FileSystemHelper.FileExists(dest))
                {
                    FileSystemHelper.DeleteFile(dest);
                }

                FileSystemHelper.SafeMoveFile(source, dest);

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"{nameof(CompleteServer)}.{nameof(SendBundleOutAsync)}");
                return false;
            }
        }

        public FileSystemHelperStub FileSystemHelper { get; } = new FileSystemHelperStub();

        /// <remarks>
        /// Caller should dispose of MemoryStream returned
        /// </remarks>
        public async Task<byte[]> ZipPackageAsync(DocumentPackage package)
        {
            var outStream = new MemoryStream();
            using (var zipStream = new ZipOutputStream(outStream))
            {
                zipStream.SetLevel(3);
                foreach (var document in package.Documents)
                {
                    try
                    {
                        var outputXmlFile =
                            document.DocumentData.Single(d => d.DataType == DocumentDataType.TransformData);
                        var documentBaseFileName = FileSystemHelper.GetFileNameWithoutExtension(document.FileName);

                        var xmlData = await FileSystemHelper.ReadAllBytesAsync(outputXmlFile.DataFileName);
                        var zipEntry =
                            new ZipEntry(
                                ReplaceUnderscoreWith2E(FileSystemHelper.CombinePath(documentBaseFileName,
                                    $"{documentBaseFileName}.XML")))
                            {
                                DateTime = CurrentDateTime(),
                                Size = xmlData.Length
                            };
                        zipStream.PutNextEntry(zipEntry);
                        zipStream.Write(xmlData, 0, xmlData.Length);
                        zipStream.CloseEntry();

                        foreach (var page in document.DocumentPages.OrderBy(p => p.PageNumber))
                        {
                            foreach (var svgFile in
                                page.DocumentData.Where(d => d.DataType == DocumentDataType.SvgData))
                            {
                                var svgFileName = FileSystemHelper.CombinePath(documentBaseFileName,
                                    FileSystemHelper.GetFileName(svgFile.DataFileName));
                                var svgData = await FileSystemHelper.ReadAllBytesAsync(svgFileName);

                                var fileName = ReplaceUnderscoreWith2E(svgFileName);

                                var svgEntry = new ZipEntry(fileName)
                                {
                                    DateTime = CurrentDateTime(),
                                    Size = svgData.Length
                                };

                                zipStream.PutNextEntry(svgEntry);
                                zipStream.Write(svgData, 0, svgData.Length);
                                zipStream.CloseEntry();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, $"{nameof(CompleteServer)}.{nameof(ZipPackageAsync)}");
                    }
                }

                zipStream.IsStreamOwner = false;
                zipStream.Close();
            }

            outStream.Position = 0;
            var zipData = outStream.ToArray();
            outStream.Close();

            return zipData;
        }

        public void TarPackages(string sourceDir, string targetFileName)
        {
            using (var outStream = new FileStream(targetFileName, FileMode.Create))
            {
                using (var tarStream = new TarOutputStream(outStream))
                {
                    var zipFiles = FileSystemHelper.GetFilesRecursive(sourceDir, "*.ZIP");
                    foreach (var zip in zipFiles)
                    {
                        using (var inputStream = File.OpenRead(zip))
                        {
                            var tarName = FileSystemHelper.GetFileName(zip);
                            var fileSize = inputStream.Length;
                            var tarEntry = TarEntry.CreateTarEntry(tarName);
                            tarEntry.Size = fileSize;
                            tarStream.PutNextEntry(tarEntry);

                            var localBuffer = new byte[32 * 1024];
                            while (true)
                            {
                                var numRead = inputStream.Read(localBuffer, 0, localBuffer.Length);
                                if (numRead <= 0)
                                {
                                    break;
                                }

                                tarStream.Write(localBuffer, 0, numRead);
                            }

                            tarStream.CloseEntry();
                        }
                    }

                    tarStream.Close();
                }

                outStream.Close();
            }

            Logger.LogTrace($"Created Tar: {targetFileName} at {CurrentDateTime()}");
            FileSystemHelper.CleanUp(sourceDir);
        }



        public string ReplaceUnderscoreWith2E(string instr)
        {
            var ret = instr.Replace("_", "+2e");
            return ret;
        }

        public string GetPackageOutFile(DocumentPackage package)
        {
            return FileSystemHelper.CombinePath(Settings.OutputDirectory,
                $"{package.PackageName}.{ReplaceUnderscoreWith2E(package.DocumentBundleReceived.BundleName)}{Settings.PackageFileExtension}");
        }


        public void SetBundleTempFile(DocumentBundleReturned bundle)
        {
            if (string.IsNullOrEmpty(bundle.ReturnedFileName))
            {
                bundle.ReturnedFileName = FileSystemHelper.CombinePath(Settings.TemporaryDirectory,
                    bundle.Id + Settings.BundleFileExtension);
            }
        }


        public void SetBundleFile(DocumentBundleReturned bundle)
        {
            if (string.IsNullOrEmpty(bundle.ReturnedFileName))
            {
                bundle.ReturnedFileName = FileSystemHelper.CombinePath(Settings.OutputDirectory,
                    bundle.Id + Settings.BundleFileExtension);
            }
        }
    }



    internal class ZipEntry
    {
        public string EntryName { get; }
        public DateTime DateTime { get; set; }
        public int Size { get; set; }

        public ZipEntry(string entryName)
        {
            EntryName = entryName;
        }

        public string Dump()
        {
            return $"{nameof(ZipEntry)}: {EntryName}, {DateTime}, {Size}";
        }
    }

    internal class ZipOutputStream : IDisposable
    {
        public List<string> State { get; } = new List<string>();

        public bool IsStreamOwner
        {
            set => State.Add($"IsStreamOwner: {value}");
        }

        public ZipOutputStream(Stream outStream)
        {
            outStream.ZipStream = this;
        }

        public void Dispose()
        {
            Close();
        }

        public void SetLevel(int compressionLevel)
        {
            State.Add($"SetLevel: {compressionLevel}");

        }

        public void PutNextEntry(ZipEntry zipEntry)
        {
            State.Add($"PutNextEntry: {zipEntry.Dump()}");
        }

        public void Write(string data, int start, int end)
        {
            State.Add($"Write: {data}, {start}, {end}");
        }

        public void Write(byte[] data, int start, int end)
        {
            var dataString = Encoding.ASCII.GetString(data);
            State.Add($"Write: {dataString}, {start}, {end}");
        }

        public void CloseEntry()
        {
            State.Add("CloseEntry");
        }

        public void Close()
        {
            State.Add("Close");

            // Check state here!!!!
        }
    }

    internal class TarEntry
    {
        public string EntryName { get; }
        public DateTime DateTime { get; set; }
        public int Size { get; set; }

        public TarEntry(string entryName)
        {
            EntryName = entryName;
        }

        public string Dump()
        {
            return $"{nameof(ZipEntry)}: {EntryName}, {DateTime}, {Size}";
        }

        public static TarEntry CreateTarEntry(string tarName)
        {
            return new TarEntry(tarName);
        }
    }

    internal class TarOutputStream : IDisposable
    {
        public List<string> State { get; } = new List<string>();

        public bool IsStreamOwner
        {
            set => State.Add($"IsStreamOwner: {value}");
        }

        public TarOutputStream(Stream outStream)
        {
            outStream.TarStream = this;
        }

        public void SetLevel(int compressionLevel)
        {
            State.Add($"SetLevel: {compressionLevel}");
        }

        public void PutNextEntry(TarEntry tarEntry)
        {
            State.Add($"PutNextEntry: {tarEntry.Dump()}");
        }

        public void Write(string data, int start, int end)
        {
            State.Add($"Write: {data}, {start}, {end}");
        }

        public void Write(byte[] data, int start, int end)
        {
            //byte[] bytes = Encoding.ASCII.GetBytes(someString);
            var dataString = Encoding.ASCII.GetString(data);
            State.Add($"Write: {dataString}, {start}, {end}");
        }

        public void CloseEntry()
        {
            State.Add("CloseEntry");
        }

        public void Dispose()
        {
            Close();
        }

        public void Close()
        {
            State.Add("Close");
        }
    }
}
