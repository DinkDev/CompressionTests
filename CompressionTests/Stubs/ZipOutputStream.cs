namespace CompressionTests.Stubs
{
    using System;
    using System.Collections.Generic;
    using System.Text;

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
}