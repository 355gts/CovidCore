using System.Net.Http;

namespace Covid.Common.HttpClientHelper.Factories
{
    public interface IHttpClientFactory
    {
        HttpClient GetHttpClient(string serviceName);
    }
}
