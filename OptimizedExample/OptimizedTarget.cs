using System;
using System.IO;
using System.Net.Http;
using OptimizedExample.Utils;

namespace OptimizedExample
{
    public class OptimizedTarget
    {
        private readonly IHttpClientWrapper _httpClientWrapper;
        private readonly IFileSystemOperation _fileSystemOperator;

        // Default
        public OptimizedTarget()
        {
            _httpClientWrapper = new HttpClientWrapper();
            _fileSystemOperator = new FileSystemOperator();
        }

        // Used by unit test
        public OptimizedTarget(IHttpClientWrapper httpClientWrapper, IFileSystemOperation fileSystemOperator)
        {
            this._httpClientWrapper = httpClientWrapper;
            this._fileSystemOperator = fileSystemOperator;
        }

        public bool InitializeConfigFile(string installedConfigFilePath, string userCofigFilePath)
        {
            if (!_fileSystemOperator.FileExists(installedConfigFilePath))
            {
                // Log Error
                return false;
            }

            if (!_fileSystemOperator.FileExists(userCofigFilePath))
            {
                try
                {
                    string userConfigFileDirectory = Path.GetDirectoryName(userCofigFilePath);
                    if (!_fileSystemOperator.DirectoryExists(userConfigFileDirectory))
                        _fileSystemOperator.CreateDirectory(userConfigFileDirectory);

                    _fileSystemOperator.FileCopy(installedConfigFilePath, userCofigFilePath);

                    return true;
                }
                catch (Exception)
                {
                    // Log Error
                    return false;
                }
            }

            return true;
        }

        public bool SendRequest(HttpRequestMessage message, out string answer)
        {
            answer = string.Empty;
            if (null == message) return false;

            try
            {
                return _httpClientWrapper.SendRequest(message, out answer);
            }
            catch (Exception)
            {
                // Log Error
                return false;
            }
        }
    }
}