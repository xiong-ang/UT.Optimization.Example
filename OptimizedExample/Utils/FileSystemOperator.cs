using System.IO;

namespace OptimizedExample.Utils
{
    public class FileSystemOperator: IFileSystemOperation
    {
        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public void FileCopy(string sourceFileName, string destFileName)
        {
            File.Copy(sourceFileName, destFileName);
        }

        public bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        public DirectoryInfo CreateDirectory(string path)
        {
            return Directory.CreateDirectory(path);
        }
    }
}