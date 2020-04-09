using Covid.Common.HttpClientHelper.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Covid.Common.HttpClientHelper.Factories
{
    public class HttpClientFactory : IHttpClientFactory
    {
        private readonly IEnumerable<IHttpClientConfiguration> _httpClientConfigurations;
        private readonly ConcurrentDictionary<string, HttpClient> _httpClients;
        private readonly object _lock = new object();

        public HttpClientFactory(IEnumerable<IHttpClientConfiguration> httpClientConfigurations)
        {
            _httpClientConfigurations = httpClientConfigurations ?? throw new ArgumentNullException(nameof(httpClientConfigurations));
            _httpClients = new ConcurrentDictionary<string, HttpClient>();
        }

        public HttpClient GetHttpClient(string serviceName)
        {
            if (!_httpClients.ContainsKey(serviceName))
            {
                var serviceConfiguration = _httpClientConfigurations.FirstOrDefault(n => n.Name == serviceName);
                if (serviceConfiguration == null)
                {
                    throw new Exception($"Failed to find service with name '{serviceName}' within end point configurations.");
                }

                var httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri(serviceConfiguration.RootUri);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                lock (_lock)
                {
                    if (!_httpClients.TryAdd(serviceName, httpClient))
                    {
                        throw new Exception($"Failed to add '{serviceName}' to http client dictionary.");
                    }
                }
            }

            return _httpClients[serviceName];
        }
    }
}
