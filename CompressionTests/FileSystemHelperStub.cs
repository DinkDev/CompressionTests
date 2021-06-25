namespace CompressionTests
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    internal class FileSystemHelperStub
    {
        private List<string> _zipFiles = new List<string>();

        public void CleanUp(string sourceDir)
        {
        }

        public IEnumerable<string> GetFilesRecursive(string sourceDir, string zip)
        {
            return _zipFiles;
        }

        public string GetFileName(string fullPath)
        {
            return Path.GetFileName(fullPath);
        }

        public string GetFileNameWithoutExtension(string fileName)
        {
            return Path.GetFileNameWithoutExtension(fileName);
        }

        public async Task<byte[]> ReadAllBytesAsync(string dataFileName)
        {
            return await Task.FromResult(Encoding.ASCII.GetBytes($"Contents from {dataFileName}"));
        }

        public string CombinePath(params string[] pathParts)
        {
            return Path.Combine(pathParts);
        }

        public bool FileExists(string save)
        {
            return true;
        }

        public void DeleteFile(string save)
        {
        }

        public void CopyFile(string source, string save)
        {
        }

        public void SafeMoveFile(string source, string dest)
        {
        }
    }
}