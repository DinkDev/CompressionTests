namespace CompressionTests.Stubs
{
    using System;

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
}