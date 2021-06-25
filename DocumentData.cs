namespace CompressionTests
{
    using System;
    using System.Collections.Generic;

    public class DocumentData
    {
        public int Id { get; set; }
        public bool IsDeleted { get; set; }
        public int DocumentPageId { get; set; }
        public string DataType { get; set; }
        public string DataFileName { get; set; }
        public string RecordStatus { get; set; }
        public DateTime CreateDate { get; set; }
        public int? DocumentId { get; set; }
        public string KeyName { get; set; }
        public int? PageCount { get; set; }

        /// <summary>
        /// var myDeserializedClass = JsonConvert.DeserializeObject<DocumentDataCollection>(myJsonResponse); 
        /// </summary>
        public class DocumentDataCollection
        {
            public List<DocumentData> DocumentData { get; set; }

    //        public static string JsonData => @"{
    //""DocumentData"": [
            public static string JsonData => @"{
    ""DocumentData"": [        {
            ""Id"": 74,
            ""IsDeleted"": false,
            ""DocumentPageId"": 64,
            ""DataType"": ""ImageData"",
            ""DataFileName"": ""c:\\CdcShare\\DocFiles\\13060495\\00000001.tif"",
            ""RecordStatus"": ""Default"",
            ""CreateDate"": ""2021-04-08T10:02:43.1500000""
        },
        {
            ""Id"": 75,
            ""IsDeleted"": false,
            ""DocumentPageId"": 65,
            ""DataType"": ""ImageData"",
            ""DataFileName"": ""c:\\CdcShare\\DocFiles\\13060495\\00000002.tif"",
            ""RecordStatus"": ""Default"",
            ""CreateDate"": ""2021-04-08T10:02:43.1500000""
        },
        {
            ""Id"": 76,
            ""IsDeleted"": false,
            ""DocumentPageId"": 66,
            ""DataType"": ""ImageData"",
            ""DataFileName"": ""c:\\CdcShare\\DocFiles\\13060495\\00000003.tif"",
            ""RecordStatus"": ""Default"",
            ""CreateDate"": ""2021-04-08T10:02:43.1500000""
        },
        {
            ""Id"": 77,
            ""IsDeleted"": false,
            ""DocumentPageId"": 67,
            ""DataType"": ""ImageData"",
            ""DataFileName"": ""c:\\CdcShare\\DocFiles\\13060495\\00000004.tif"",
            ""RecordStatus"": ""Default"",
            ""CreateDate"": ""2021-04-08T10:02:43.1500000""
        },
        {
            ""Id"": 2237,
            ""IsDeleted"": false,
            ""DocumentId"": 21,
            ""DataType"": ""BoundaryBoxData"",
            ""DataFileName"": ""c:\\CdcShare\\DocFiles\\13060495\\13060495.2013-12-12.HP49ZKJ2PXXIFW3.IDS_BBOXES.XML"",
            ""RecordStatus"": ""Default"",
            ""CreateDate"": ""2021-06-22T08:33:40.7400000""
        },
        {
            ""Id"": 2238,
            ""IsDeleted"": false,
            ""DocumentId"": 21,
            ""DataType"": ""MultiPageImageData"",
            ""KeyName"": ""PreparedTiff"",
            ""DataFileName"": ""c:\\CdcShare\\DocFiles\\13060495\\13060495.2013-12-12.HP49ZKJ2PXXIFW3.IDS_IMAGES.tif"",
            ""RecordStatus"": ""Default"",
            ""PageCount"": 4,
            ""CreateDate"": ""2021-06-22T08:33:40.7400000""
        },
        {
            ""Id"": 2239,
            ""IsDeleted"": false,
            ""DocumentId"": 21,
            ""DataType"": ""OcrData"",
            ""DataFileName"": ""c:\\CdcShare\\DocFiles\\13060495\\13060495.2013-12-12.HP49ZKJ2PXXIFW3.IDS_IMAGES.XML"",
            ""RecordStatus"": ""Default"",
            ""CreateDate"": ""2021-06-22T08:33:40.7400000""
        },
        {
            ""Id"": 2240,
            ""IsDeleted"": false,
            ""DocumentId"": 21,
            ""DataType"": ""ParagraphData"",
            ""DataFileName"": ""c:\\CdcShare\\DocFiles\\13060495\\13060495.2013-12-12.HP49ZKJ2PXXIFW3.IDS_IMAGES.TXT"",
            ""RecordStatus"": ""Default"",
            ""CreateDate"": ""2021-06-22T08:33:40.7400000""
        },
        {
            ""Id"": 2241,
            ""IsDeleted"": false,
            ""DocumentId"": 21,
            ""DataType"": ""TransformData"",
            ""DataFileName"": ""13060495.2013-12-12.HP49ZKJ2PXXIFW3_Transformed.xml"",
            ""RecordStatus"": ""Default"",
            ""CreateDate"": ""2021-06-22T08:33:40.7400000""
        }
    ]
}";

        }
    }
}