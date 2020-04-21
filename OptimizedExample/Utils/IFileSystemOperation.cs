using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OptimizedExample.Utils
{
    public interface IFileSystemOperation
    {
        bool FileExists(string path);
        void FileCopy(string sourceFileName, string destFileName);

        bool DirectoryExists(string path);
        DirectoryInfo CreateDirectory(string path);
    }
}
