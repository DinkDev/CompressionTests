namespace CompressionTests
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// var myDeserializedClass = JsonConvert.DeserializeObject<DocumentPackage>(myJsonResponse); 
    /// </summary>
    public class DocumentPackage
    {



        public int Id { get; set; }
        public bool IsDeleted { get; set; }
        public string RecordStatus { get; set; }
        public string ApplicationNumber { get; set; }
        public DateTime LastErroredDate { get; set; }
        public string LastError { get; set; }
        public int DocumentBundleReceivedId { get; set; }
        public DateTime AcceptedDate { get; set; }
        public DateTime ReceivedDate { get; set; }
        public int ReceivedFileChecksum { get; set; }
        public string ReceivedFileName { get; set; }
        public DateTime ReturnedDate { get; set; }
        public int ReturnedFileChecksum { get; set; }
        public string ReturnedFileName { get; set; }
        public DateTime TransferredDate { get; set; }
        public string MessageCode { get; set; }

        public static string JsonData => @"         {
            ""Id"": 42,
            ""IsDeleted"": false,
            ""RecordStatus"": ""Completed"",
            ""ApplicationNumber"": ""13060495"",
            ""LastErroredDate"": ""2021-06-22T08:33:40.8179739"",
            ""LastError"": ""Test : Package 13060495 competed - sent to Out queue"",
            ""DocumentBundleReceivedId"": 33,
            ""AcceptedDate"": ""2021-04-08T08:38:46.9149728"",
            ""ReceivedDate"": ""2021-04-08T08:38:44.4823004"",
            ""ReceivedFileChecksum"": 489767400,
            ""ReceivedFileName"": ""13060495.zip"",

            ""ReturnedDate"": ""2021-06-24T09:00:00.0000001"",
            ""ReturnedFileChecksum"": 0,
            ""ReturnedFileName"": ""13060495.zip"",

            ""TransferredDate"": ""2020-11-18T09:05:43.9489893"",
            ""MessageCode"": ""OK""
        }";

        public BundleStub DocumentBundleReceived { get; } = new BundleStub();
        public string PackageName => "13060495";
        public int ReturnedFileSize { get; set; }
        public List<Document> Documents { get; } = new List<Document>();

        public void SetLastError(string lastError, DateTime lastErrorDate)
        {
            LastError = lastError;
            LastErroredDate = LastErroredDate;
        }

        public void SetRecordStatusErrored()
        {
            RecordStatus = @"Errored";
        }

        public void SetRecordStatusSent()
        {
            RecordStatus = @"Sent";
        }
    }

    public class BundleStub
    {
        public string BundleName => "1CLM1IDS";
    }
}