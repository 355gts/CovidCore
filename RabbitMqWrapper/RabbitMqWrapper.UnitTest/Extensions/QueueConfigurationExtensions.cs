using Moq;
using RabbitMQWrapper.Configuration;
using System;

namespace RabbitMQWrapper.UnitTest.Extensions
{
    public static class QueueConfigurationExtensions
    {
        public static Mock<IQueueConfiguration> InitialiseMock(this Mock<IQueueConfiguration> queueConfiguration)
        {
            queueConfiguration.Setup(q => q.Uri).Returns(new Uri("amqp://MockRabbitMqUri:15672"));
            queueConfiguration.Setup(q => q.ClientCertificateSubjectName).Returns("MockCertificateSubjectName");
            queueConfiguration.Setup(q => q.IsValid).Returns(true);
            return queueConfiguration;
        }
    }
}
