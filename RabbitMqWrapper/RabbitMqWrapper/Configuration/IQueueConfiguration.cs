using System;
using System.Collections.Generic;

namespace RabbitMQWrapper.Configuration
{
    public interface IQueueConfiguration
    {
        bool AutomaticRecoveryEnabled { get; set; }
        int ChannelConfirmTimeoutIntervalSeconds { get; set; }
        string ClientCertificateSubjectName { get; set; }
        IEnumerable<ConsumerConfiguration> Consumers { get; set; }
        int ContinuationTimeoutSeconds { get; set; }
        int HandshakeContinuationTimeoutSeconds { get; set; }
        ushort MessagePrefetchCount { get; set; }
        int MillisecondsBetweenConnectionRetries { get; set; }
        int NetworkRecoveryIntervalSeconds { get; set; }
        int ProtocolTimeoutIntervalSeconds { get; set; }
        IEnumerable<PublisherConfiguration> Publishers { get; set; }
        int PublishMessageConfirmationTimeoutSeconds { get; set; }
        ushort RabbitMQHeartbeatSeconds { get; set; }
        int RequestedConnectionTimeoutSeconds { get; set; }
        ushort RequestedHeartbeatSeconds { get; set; }
        string TemporaryQueueNamePrefix { get; set; }
        Uri Uri { get; set; }
        bool IsValid { get; }
    }
}