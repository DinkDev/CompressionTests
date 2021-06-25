namespace CompressionTests
{
    using System;

    /// <summary>
    /// var myDeserializedClass = JsonConvert.DeserializeObject<DocumentPackageQueue>(myJsonResponse); 
    /// </summary>
    public class DocumentPackageQueue
    {
        public int DocumentPackageId { get; set; }
        public string QueueType { get; set; }
        public DateTime CreateDate { get; set; }

        public static string JsonData => @"{
            ""DocumentPackageId"": 42,
            ""QueueType"": ""Out"",
            ""CreateDate"": ""2021-06-22T08:33:40.8433333""
        }";
    }
}