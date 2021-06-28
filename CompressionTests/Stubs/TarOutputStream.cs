namespace CompressionTests.Stubs
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    internal class TarOutputStream : IDisposable
    {
        public List<string> State { get; } = new List<string>();

        public bool IsStreamOwner
        {
            set => State.Add($"IsStreamOwner: {value}");
        }

        public TarOutputStream(Stream outStream)
        {
            outStream.TarStream = this;
        }

        public void SetLevel(int compressionLevel)
        {
            State.Add($"SetLevel: {compressionLevel}");
        }

        public void PutNextEntry(TarEntry tarEntry)
        {
            State.Add($"PutNextEntry: {tarEntry.Dump()}");
        }

        public void Write(string data, int start, int end)
        {
            State.Add($"Write: {data}, {start}, {end}");
        }

        public void Write(byte[] data, int start, int end)
        {
            //byte[] bytes = Encoding.ASCII.GetBytes(someString);
            var dataString = Encoding.ASCII.GetString(data);
            State.Add($"Write: {dataString}, {start}, {end}");
        }

        public void CloseEntry()
        {
            State.Add("CloseEntry");
        }

        public void Dispose()
        {
            Close();
        }

        public void Close()
        {
            State.Add("Close");
        }
    }
}