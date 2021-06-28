namespace CompressionTests.Stubs
{
    using System;
    using System.Text;
    using Microsoft.Extensions.Logging;

    internal class Stream : IDisposable
    {
        public string FileName { get; protected set; }
        public int Position { get; set; }
        public int Length { get; set; } = 4000;
        public ZipOutputStream ZipStream { get; set; }
        public TarOutputStream TarStream { get; set; }

        public static ILogger Logger { get; set; }

        public int Read(byte[] output, int start, int end)
        {
            Position += end - start;

            var data = $"Read: {FileName}, {start}, {end}";
            var bytes = Encoding.ASCII.GetBytes(data);
            output.CopyTo(bytes, 0);
            return bytes.Length;
        }

        public byte[] ToArray()
        {
            var data = $"Read: {FileName}, {Position}, {Length}";
            var bytes = Encoding.ASCII.GetBytes(data);
            return bytes;
        }

        public void Close()
        {
            Logger.LogInformation($"{GetType().Name}: {nameof(Close)} - start data dump.");

            Logger.LogInformation($"{nameof(TarStream)}:");
            TarStream?.State.ForEach(t => Logger.LogInformation(t));

            Logger.LogInformation($"{nameof(ZipStream)}:");
            ZipStream?.State.ForEach(z => Logger.LogInformation(z));

            Logger.LogInformation($"{GetType().Name}: {nameof(Close)} - end data dump.");
        }

        public void Dispose()
        {
            Close();
        }
    }

    internal enum FileMode
    {
        Create,
        Read
    }

    internal class FileStream : Stream
    {
        public FileStream(string targetFileName, FileMode create)
        {
            FileName = targetFileName;

            Length = 1001;
        }
    }

    internal class MemoryStream : Stream
    {
    }

    internal class File
    {
        public static Stream OpenRead(string fileName)
        {
            var stream = new FileStream(fileName, FileMode.Read);
            return stream;
        }
    }
}