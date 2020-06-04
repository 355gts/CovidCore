using Moq;
using RabbitMQWrapper.Configuration;
using System;
using System.Collections.Generic;

namespace RabbitMQWrapper.UnitTest.Extensions
{
    public static class QueueConfigurationExtensions
    {
        public static Mock<IQueueConfiguration> InitialiseMock(this Mock<IQueueConfiguration> queueConfiguration)
        {
            queueConfiguration.Setup(q => q.Uri).Returns(new Uri("amqp://MockRabbitMqUri:15672"));
            queueConfiguration.Setup(q => q.ClientCertificateSubjectName).Returns("MockCertificateSubjectName");
            queueConfiguration.Setup(q => q.IsValid).Returns(true);
            queueConfiguration.Setup(q => q.TemporaryQueueNamePrefix).Returns("TMP_");
            queueConfiguration.Setup(q => q.Consumers)
                              .Returns(new List<ConsumerConfiguration>()
                              {
                                  new ConsumerConfiguration()
                                  {
                                      Name = "consumerName",
                                      QueueName = "queueName",
                                  }
                              });
            queueConfiguration.Setup(q => q.Publishers)
                              .Returns(new List<PublisherConfiguration>()
                              {
                                  new PublisherConfiguration()
                                  {
                                      Name = "publisherName",
                                      ExchangeName = "exchangeName",
                                      RoutingKey = "routingKey",
                                  }
                              });
            return queueConfiguration;
        }
    }
}
