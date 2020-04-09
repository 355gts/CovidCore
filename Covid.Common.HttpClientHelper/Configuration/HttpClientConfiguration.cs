using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace Covid.Common.HttpClientHelper.Configuration
{
    [DataContract]
    [Serializable]
    public class HttpClientConfiguration : IHttpClientConfiguration
    {
        [DataMember(IsRequired = true)]
        [JsonProperty("name")]
        public string Name { get; set; }

        [DataMember(IsRequired = true)]
        [JsonProperty("rootUri")]
        public string RootUri { get; set; }
    }
}
