using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Covid.Rabbit.Configuration
{
    [DataContract]
    [Serializable]
    public class QueueConfiguration : IQueueConfiguration
    {
        private IDictionary<string, QueueConfig> _consumerDictionary;
        private IDictionary<string, QueueConfig> _publisherDictionary;

        [DataMember(IsRequired = true)]
        [JsonProperty("hostname")]
        public string Hostname { get; set; }

        [DataMember(IsRequired = true)]
        [JsonProperty("username")]
        public string Username { get; set; }

        [DataMember(IsRequired = true)]
        [JsonProperty("password")]
        public string Password { get; set; }

        [DataMember(IsRequired = false)]
        [JsonProperty("uri")]
        public string Uri { get; set; }

        [DataMember(IsRequired = false)]
        [JsonProperty("port")]
        public int? Port { get; set; }

        [DataMember(IsRequired = false)]
        [JsonProperty("certificatePath")]
        public string CertificatePath { get; set; }

        [DataMember(IsRequired = true)]
        [JsonProperty("automaticRecoveryEnabled")]
        public bool AutomaticRecoveryEnabled { get; set; }

        [DataMember(IsRequired = true)]
        [JsonProperty("maxPrefetchSize")]
        public ushort MaxPrefetchSize { get; set; }

        [DataMember(IsRequired = true)]
        [JsonProperty("networkRecoveryIntervalSeconds")]
        public int NetworkRecoveryIntervalSeconds { get; set; }

        [DataMember(IsRequired = true)]
        [JsonProperty("continuationTimeoutSeconds")]
        public int ContinuationTimeoutSeconds { get; set; }

        [DataMember(IsRequired = true)]
        [JsonProperty("handshakeContinuationTimeoutSeconds")]
        public int HandshakeContinuationTimeoutSeconds { get; set; }

        [DataMember(IsRequired = true)]
        [JsonProperty("requestedConnectionTimeoutSeconds")]
        public int RequestedConnectionTimeoutSeconds { get; set; }

        [DataMember(IsRequired = true)]
        [JsonProperty("requestedHeartbeatSeconds")]
        public ushort RequestedHeartbeatSeconds { get; set; }

        [DataMember(IsRequired = false)]
        [JsonProperty("consumers")]
        public IEnumerable<QueueConfig> Consumers { get; set; }

        [DataMember(IsRequired = false)]
        [JsonProperty("publishers")]
        public IEnumerable<QueueConfig> Publishers { get; set; }

        public QueueConfig this[string key]
        {
            get
            {
                if (Consumers.Any(n => n.Name == key))
                    return Consumers.Where(n => n.Name == key).First();

                if (Publishers.Any(n => n.Name == key))
                    return Publishers.Where(n => n.Name == key).First();

                return null;
            }
        }
    }
}
