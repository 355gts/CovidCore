using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;

namespace RabbitMQWrapper.Configuration
{
    [DataContract]
    [Serializable]
    public class QueueConfiguration : IQueueConfiguration
    {
        [DataMember(IsRequired = true)]
        [JsonProperty("uri")]
        public Uri Uri { get; set; }

        [DataMember(IsRequired = true)]
        [JsonProperty("clientCertificateSubjectName")]
        public string ClientCertificateSubjectName { get; set; }

        [DataMember(IsRequired = false)]
        [JsonProperty("temporaryQueueNamePrefix", DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue("SPC_")]
        public string TemporaryQueueNamePrefix { get; set; }

        [DataMember(IsRequired = false)]
        [JsonProperty("messagePrefetchCount", DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue((ushort)1)]
        public ushort MessagePrefetchCount { get; set; }

        [DataMember(IsRequired = false)]
        [JsonProperty("rabbitMQHeartbeatSeconds", DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue((ushort)120)]
        public ushort RabbitMQHeartbeatSeconds { get; set; }

        [DataMember(IsRequired = false)]
        [JsonProperty("publishMessageConfirmationTimeoutSeconds", DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(5)]
        public int PublishMessageConfirmationTimeoutSeconds { get; set; }

        [DataMember(IsRequired = false)]
        [JsonProperty("millisecondsBetweenConnectionRetries", DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(1000)]
        public int MillisecondsBetweenConnectionRetries { get; set; }

        [DataMember(IsRequired = false)]
        [JsonProperty("automaticRecoveryEnabled", DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(true)]
        public bool AutomaticRecoveryEnabled { get; set; }

        [DataMember(IsRequired = false)]
        [JsonProperty("networkRecoveryIntervalSeconds", DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(5)]
        public int NetworkRecoveryIntervalSeconds { get; set; }

        [DataMember(IsRequired = false)]
        [JsonProperty("channelConfirmTimeoutIntervalSeconds", DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(1)]
        public int ChannelConfirmTimeoutIntervalSeconds { get; set; }

        [DataMember(IsRequired = false)]
        [JsonProperty("protocolTimeoutIntervalSeconds", DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(20)]
        public int ProtocolTimeoutIntervalSeconds { get; set; }

        [DataMember(IsRequired = false)]
        [JsonProperty("continuationTimeoutSeconds", DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(10)]
        public int ContinuationTimeoutSeconds { get; set; }

        [DataMember(IsRequired = false)]
        [JsonProperty("handshakeContinuationTimeoutSeconds", DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(10)]
        public int HandshakeContinuationTimeoutSeconds { get; set; }

        [DataMember(IsRequired = false)]
        [JsonProperty("requestedConnectionTimeoutSeconds", DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(10)]
        public int RequestedConnectionTimeoutSeconds { get; set; }

        [DataMember(IsRequired = false)]
        [JsonProperty("requestedHeartbeatSeconds", DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(10)]
        public ushort RequestedHeartbeatSeconds { get; set; }

        [DataMember(IsRequired = false)]
        [JsonProperty("consumers")]
        public IEnumerable<ConsumerConfiguration> Consumers { get; set; }

        [DataMember(IsRequired = false)]
        [JsonProperty("publishers")]
        public IEnumerable<PublisherConfiguration> Publishers { get; set; }

        public bool IsValid
        {
            get
            {
                if (string.IsNullOrEmpty(Uri.ToString()))
                    return false;

                if (string.IsNullOrEmpty(ClientCertificateSubjectName))
                    return false;

                if (Consumers != null && !Consumers.Any(c => c.IsValid))
                    return false;

                if (Publishers != null && !Publishers.Any(p => p.IsValid))
                    return false;

                return true;
            }
        }
    }
}
