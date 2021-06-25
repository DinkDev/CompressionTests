namespace CompressionTests
{
    using System.Collections.Generic;


    public class DocumentPage
    {
        public int Id { get; set; }
        public bool IsDeleted { get; set; }
        public string RecordStatus { get; set; }
        public int DocumentId { get; set; }
        public int PageNumber { get; set; }

        public List<DocumentData> DocumentData { get; } = new List<DocumentData>();


        /// <summary>
        /// var myDeserializedClass = JsonConvert.DeserializeObject<DocumentPageCollection>(myJsonResponse);
        /// </summary>
        public class DocumentPageCollection
        {
            public List<DocumentPage> DocumentPage { get; set; }

            public static string JsonData => @"{
    ""DocumentPage"": [
        {
            ""Id"": 64,
            ""IsDeleted"": false,
            ""RecordStatus"": ""Default"",
            ""DocumentId"": 21,
            ""PageNumber"": 1
        },
        {
            ""Id"": 65,
            ""IsDeleted"": false,
            ""RecordStatus"": ""Default"",
            ""DocumentId"": 21,
            ""PageNumber"": 2
        },
        {
            ""Id"": 66,
            ""IsDeleted"": false,
            ""RecordStatus"": ""Default"",
            ""DocumentId"": 21,
            ""PageNumber"": 3
        },
        {
            ""Id"": 67,
            ""IsDeleted"": false,
            ""RecordStatus"": ""Default"",
            ""DocumentId"": 21,
            ""PageNumber"": 4
        }
    ]
}";
        }

    }

}