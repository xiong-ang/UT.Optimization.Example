using System.Net.Http;

namespace OptimizedExample.Utils
{
    public interface IHttpClientWrapper
    {
        bool SendRequest(HttpRequestMessage message, out string answer);
    }
}