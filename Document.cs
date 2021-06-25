namespace CompressionTests
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// var myDeserializedClass = JsonConvert.DeserializeObject<Document>(myJsonResponse);
    /// </summary>
    public class Document
    {
        public int Id { get; set; }
        public bool IsDeleted { get; set; }
        public string RecordStatus { get; set; }
        public DateTime ProcessingStartedDate { get; set; }
        public DateTime ProcessingCompletedDate { get; set; }
        public string FileName { get; set; }
        public int DocumentPackageId { get; set; }
        public string DocumentCode { get; set; }
        public string DocumentKeyId { get; set; }
        public string MailRoomDate { get; set; }
        public string TotalPageQuantity { get; set; }
        public string DocumentTypeTransformProcessDocumentType { get; set; }

        public static string JsonData => @"{
            ""Id"": 21,
            ""IsDeleted"": false,
            ""RecordStatus"": ""Completed"",
            ""ProcessingStartedDate"": ""2021-06-22T08:32:11.0154184"",
            ""ProcessingCompletedDate"": ""2021-06-22T08:33:40.6049953"",
            ""FileName"": ""13060495.2013-12-12.HP49ZKJ2PXXIFW3.IDS"",
            ""DocumentPackageId"": 42,
            ""DocumentCode"": ""IDS"",
            ""DocumentKeyId"": ""HP49ZKJ2PXXIFW3"",
            ""MailRoomDate"": ""12\/12\/2013"",
            ""TotalPageQuantity"": ""4"",
            ""DocumentTypeTransformProcessDocumentType"": ""IDS""
        }";

        public List<DocumentData> DocumentData { get; } = new List<DocumentData>();
        public List<DocumentPage> DocumentPages { get; } = new List<DocumentPage>();

        public void SetDocumentStatusErrored()
        {
            RecordStatus = @"Errored";
        }
    }
}