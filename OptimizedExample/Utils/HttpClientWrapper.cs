using System.Net.Http;

namespace OptimizedExample.Utils
{
    public class HttpClientWrapper : IHttpClientWrapper
    {
        public bool SendRequest(HttpRequestMessage message, out string answer)
        {
            answer = string.Empty;
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = client.SendAsync(message).Result;
                if (null == response) return false;

                answer = response.Content.ReadAsStringAsync().Result;
                return true;
            }
        }
    }
}