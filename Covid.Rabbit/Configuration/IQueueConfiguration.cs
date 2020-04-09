using System.Collections.Generic;

namespace Covid.Rabbit.Configuration
{
    public interface IQueueConfiguration
    {
        string Hostname { get; set; }

        string Uri { get; set; }

        int? Port { get; set; }

        string Username { get; set; }

        string Password { get; set; }

        string CertificatePath { get; set; }

        bool AutomaticRecoveryEnabled { get; set; }

        int NetworkRecoveryIntervalSeconds { get; set; }

        int ContinuationTimeoutSeconds { get; set; }

        int HandshakeContinuationTimeoutSeconds { get; set; }

        int RequestedConnectionTimeoutSeconds { get; set; }

        ushort RequestedHeartbeatSeconds { get; set; }

        IEnumerable<QueueConfig> Consumers { get; set; }

        IEnumerable<QueueConfig> Publishers { get; set; }

        QueueConfig this[string key] { get; }
    }
}
