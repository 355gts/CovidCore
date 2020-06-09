using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace RabbitMQWrapper.Configuration
{
    [DataContract]
    [Serializable]
    public class QueueConfig : IQueueConfig
    {
        [DataMember(IsRequired = false)]
        [JsonProperty("name")]
        public string Name { get; set; }

        [DataMember(IsRequired = false)]
        [JsonProperty("queue")]
        public string Queue { get; set; }

        [DataMember(IsRequired = false)]
        [JsonProperty("routingKey")]
        public string RoutingKey { get; set; }

        [DataMember(IsRequired = false)]
        [JsonProperty("exchange")]
        public string Exchange { get; set; }
    }
}
