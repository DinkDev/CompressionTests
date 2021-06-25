namespace CompressionTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    internal class UowStub : IDisposable
    {
        private readonly List<DocumentPackage> _errorQueue = new List<DocumentPackage>();
        private readonly List<DocumentPackage> _outQueue = new List<DocumentPackage>();
        private bool _firstPass = true;

        public UowStub()
        {
            Packages = new PackageRepoStub();
            Documents = new DocumentRepoStub();
            BundlesReturned = new BundleReturnedRepoStub();
        }

        public PackageRepoStub Packages { get; }
        public DocumentRepoStub Documents { get; }
        public BundleReturnedRepoStub BundlesReturned { get; }

        public event Action Started;

        public async Task<DocumentPackage> PopPackageFromQueueAsync(QueueType @out)
        {
            DocumentPackage package = null;

            if (_firstPass)
            {
                Started?.Invoke();

                package = JsonConvert.DeserializeObject<DocumentPackage>(DocumentPackage.JsonData);
                package.Documents.AddRange(UowStub.DocumentRepoStub.Documents);

                _firstPass = false;
            }

            return await Task.FromResult(package);
        }

        public void Dispose()
        {
        }

        public class PackageRepoStub
        {
            public async void UpdateAsync(DocumentPackage package)
            {
            }
        }

        public class DocumentRepoStub
        {
            static DocumentRepoStub()
            {
                Documents = new[] {JsonConvert.DeserializeObject<Document>(Document.JsonData)}
                    .ToList();
                var docData = JsonConvert.DeserializeObject<DocumentData.DocumentDataCollection>(DocumentData.DocumentDataCollection.JsonData).DocumentData;
                foreach (var doc in Documents)
                {
                    doc.DocumentData.AddRange(docData.Where(d => d.DocumentId == doc.Id));
                }

                var docPages = JsonConvert.DeserializeObject<DocumentPage.DocumentPageCollection>(DocumentPage.DocumentPageCollection.JsonData).DocumentPage;
                foreach (var doc in Documents)
                {
                    doc.DocumentPages.AddRange(docPages.Where(d => d.DocumentId == doc.Id));

                    foreach (var page in doc.DocumentPages)
                    {
                        page.DocumentData.AddRange(docData.Where(d => d.DocumentPageId == page.Id));
                    }
                }
            }

            public static List<Document> Documents { get; }

            public async Task<List<Document>> GetAllWithChildrenByPackageIdAsync(int packageId)
            {
                return await Task.FromResult(Documents);
            }
        }

        public class BundleReturnedRepoStub
        {
            public static DocumentBundleReturned Bundle => new DocumentBundleReturned();

            public async Task<Tuple<bool, DocumentBundleReturned>> UpdateAsync(DocumentBundleReturned currentBundle)
            {
                return await Task.FromResult(Tuple.Create(true, currentBundle));
            }

            public async Task<List<DocumentBundleReturned>> FindAsync(Func<DocumentBundleReturned, object> func)
            {
                return await Task.FromResult(new[] {Bundle}.ToList());
            }

            public async Task<Tuple<bool, DocumentBundleReturned>> CreateNewDocumentBundleReturnedAsync(
                DocumentBundleReturned bundle, string workingDirectory)
            {
                return await Task.FromResult(Tuple.Create(true, Bundle));
            }
        }

        public async Task<bool> AddPackageToQueueAsync(DocumentPackage package, QueueType queue)
        {
            switch (queue)
            {
                case QueueType.Out:
                    _outQueue.Add(package);
                    break;

                case QueueType.Error:
                    _errorQueue.Add(package);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(queue), queue, null);
            }

            return await Task.FromResult(true);
        }
    }
}