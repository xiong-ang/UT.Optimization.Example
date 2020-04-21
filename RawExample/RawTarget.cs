using System;
using System.IO;
using System.Net.Http;

namespace RawExample
{
    public class RawTarget
    {
        public bool InitializeConfigFile(string installedConfigFilePath, string userCofigFilePath)
        {
            if (!File.Exists(installedConfigFilePath))
            {
                // Log Error
                return false;
            }

            if (!File.Exists(userCofigFilePath))
            {
                try
                {
                    string userConfigFileDirectory = Path.GetDirectoryName(userCofigFilePath);
                    if (string.IsNullOrWhiteSpace(userConfigFileDirectory)) return false;
                    if (!Directory.Exists(userConfigFileDirectory))
                        Directory.CreateDirectory(userConfigFileDirectory);

                    File.Copy(installedConfigFilePath, userCofigFilePath);

                    return true;
                }
                catch (Exception)
                {
                    // Log Error
                }
            }

            return false;
        }

        public bool SendRequest(HttpRequestMessage message, out string answer)
        {
            answer = string.Empty;
            if (null == message) return false;

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = client.SendAsync(message).Result;
                    if (null == response) return false;

                    answer = response.Content.ReadAsStringAsync().Result;
                    return true;
                }
            }
            catch (Exception)
            {
                // Log Error
                return false;
            }
        }
    }
}