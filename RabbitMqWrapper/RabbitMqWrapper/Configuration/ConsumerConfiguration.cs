using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace RabbitMQWrapper.Configuration
{
    [DataContract]
    [Serializable]
    public class ConsumerConfiguration : IConsumerConfiguration
    {
        [DataMember(IsRequired = true)]
        [JsonProperty("name")]
        public string Name { get; set; }

        [DataMember(IsRequired = true)]
        [JsonProperty("queueName")]
        public string QueueName { get; set; }

        [DataMember(IsRequired = true)]
        [JsonProperty("exchangeName")]
        public string ExchangeName { get; set; }

        [DataMember(IsRequired = true)]
        [JsonProperty("routingKey")]
        public string RoutingKey { get; set; }

        [DataMember(IsRequired = false)]
        [JsonProperty("messageWaitTimeoutMilliseconds", DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(1000)]
        public int MessageWaitTimeoutMilliseconds { get; set; }

        public bool IsValid
        {
            get
            {
                if (string.IsNullOrEmpty(Name))
                    return false;

                // either QueueName or ExchangeName should be populated
                if (string.IsNullOrEmpty(QueueName) && string.IsNullOrEmpty(ExchangeName))
                    return false;

                // if ExchangeName is populated then RoutingKey should also be populated 
                if (string.IsNullOrEmpty(ExchangeName) && string.IsNullOrEmpty(RoutingKey))
                    return false;

                return true;
            }
        }
    }
}
