namespace CompressionTests
{
    using System;
    using System.Collections.Generic;

    internal class DocumentBundleReturned
    {
        public int Id => 33;
        public List<DocumentPackage> DocumentPackages { get; } = new List<DocumentPackage>();
        public string BundleName => @"10101";
        public string RecordStatus { get; set; } = ContainerRecordStatus.Processing;
        public string ReturnedFileName { get; set; }
        public int ReturnedFileChecksum { get; set; }
        public int ReturnedFileSize { get; set; }

        public void SetRecordStatusProcessing()
        {
            RecordStatus = ContainerRecordStatus.Processing;
        }

        public void SetLastError(string errorMessage, DateTime errorTime)
        {
        }

        public void SetRecordStatusCompleted()
        {
            RecordStatus = ContainerRecordStatus.Completed;
        }

        public void SetRecordStatusSent()
        {
            RecordStatus = ContainerRecordStatus.Sent;
        }
    }

    internal class ContainerRecordStatus
    {
        public static string Completed => "Completed";
        public static string Processing => "Processing";
        public static string Sent => "Sent";
    }
}