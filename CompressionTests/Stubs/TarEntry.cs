namespace CompressionTests.Stubs
{
    using System;

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
}