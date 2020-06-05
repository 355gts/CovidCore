using CommonUtils.Serializer;
using Covid.Common.HttpClientHelper.Factories;
using Covid.Common.HttpClientHelper.Model;
using log4net;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Covid.Common.HttpClientHelper
{
    public class HttpClientHelper : IHttpClientHelper
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(HttpClientHelper));
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IJsonSerializer _serializer;
        private readonly HttpClient _httpClient;

        public HttpClientHelper(
            IHttpClientFactory httpClientFactory,
            IJsonSerializer serializer,
            string serviceName)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));

            if (string.IsNullOrEmpty(serviceName))
                throw new ArgumentNullException(nameof(serviceName));

            // retrieve the http client
            _httpClient = _httpClientFactory.GetHttpClient(serviceName);
            if (_httpClient == null)
                throw new ArgumentNullException(nameof(_httpClient));
        }

        public async Task<bool> DeleteAsync(string resourceUri, string apiVersion)
        {
            var message = new HttpRequestMessage(HttpMethod.Delete, resourceUri);
            // add api version header
            message.Headers.Add("X-Version", apiVersion);

            var result = await SendRequest<string>(message);

            return result.Success;
        }

        public async Task<AsyncResult<T>> GetAsync<T>(string resourceUri, string apiVersion) where T : class
        {
            var message = new HttpRequestMessage(HttpMethod.Get, resourceUri);
            // add api version header
            message.Headers.Add("X-Version", apiVersion);

            return await SendRequest<T>(message);
        }

        public async Task<AsyncResult<int>> PostAsync<T>(string resourceUri, T request, string apiVersion) where T : class
        {
            var message = ConfigureRequest(HttpMethod.Post, resourceUri, request, apiVersion);

            return await SendRequest<int>(message);
        }

        public async Task<bool> PutAsync<T>(string resourceUri, T request, string apiVersion) where T : class
        {
            var message = ConfigureRequest(HttpMethod.Put, resourceUri, request, apiVersion);

            var result = await SendRequest<string>(message);

            return result.Success;
        }

        public async Task<bool> PatchAsync<T>(string resourceUri, T request, string apiVersion) where T : class
        {
            var message = ConfigureRequest(HttpMethod.Patch, resourceUri, request, apiVersion);

            var result = await SendRequest<string>(message);

            return result.Success;
        }

        private HttpRequestMessage ConfigureRequest<T>(HttpMethod httpMethod, string resourceUri, T request, string apiVersion)
        {
            var message = new HttpRequestMessage(httpMethod, resourceUri);
            // add api version header
            message.Headers.Add("X-Version", apiVersion);

            if (request != null)
            {
                var content = _serializer.SerializeObject(request);
                message.Content = new StringContent(content, Encoding.UTF8, "application/json");
            }

            return message;
        }

        private async Task<AsyncResult<T>> SendRequest<T>(HttpRequestMessage message)
        {
            try
            {


                var response = await _httpClient.SendAsync(message).ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                    return new AsyncResult<T>(true, _serializer.Deserialize<T>(await response.Content.ReadAsStringAsync()));

                // hand 404's, 500's, etc
                return new AsyncResult<T>(false);
            }
            catch (Exception ex)
            {
                _logger.Warn($"An unexpected exception occurred making http request, error details - '{ex.Message}'", ex);
                return new AsyncResult<T>(false);
            }
        }
    }
}
