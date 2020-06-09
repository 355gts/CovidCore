using CommonUtils.Serializer;
using CommonUtils.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RabbitMQ.Client;
using RabbitMQWrapper.Configuration;
using RabbitMQWrapper.Connection;
using RabbitMQWrapper.Factories;
using RabbitMQWrapper.UnitTest.Extensions;
using RabbitMQWrapper.UnitTest.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using PUB = RabbitMQWrapper.Publisher;

namespace RabbitMQWrapper.UnitTest.Publisher
{
    [TestClass]
    public class QueuePublisher
    {
        private Mock<IQueueConfiguration> _queueConfigurationMock;
        private Mock<IQueueConnectionFactory> _connectionFactoryMock;
        private Mock<IConnectionHandler> _connectionHandlerMock;
        private Mock<IModel> _modelMock;
        private Mock<IJsonSerializer> _serializerMock;
        private Mock<IValidationHelper> _validationHelperMock;
        private string _publisherName = "publisherName";
        private CancellationToken _cancellationToken = new CancellationTokenSource().Token;
        private TestMessage message;
        private Dictionary<string, object> headers;
        private string _dynamicRoutingKey = "dynamicRoutingKey";
        private string _messageString = "messageString";
        private string _validationOutput;
        private string _exchangeNameCallback;
        private string _routingKeyCallback;
        private IBasicProperties _basicPropertiesCallback;
        private byte[] _messageBodyCallback;


        PUB.QueuePublisher<TestMessage> _queuePublisher;

        [TestInitialize]
        public void TestSetup()
        {
            _queueConfigurationMock = new Mock<IQueueConfiguration>().InitialiseMock();
            _connectionFactoryMock = new Mock<IQueueConnectionFactory>();
            _connectionHandlerMock = new Mock<IConnectionHandler>();
            _modelMock = new Mock<IModel>();
            _serializerMock = new Mock<IJsonSerializer>();
            _validationHelperMock = new Mock<IValidationHelper>();

            message = new TestMessage();
            headers = new Dictionary<string, object>();

            _queueConfigurationMock.Setup(q => q.IsValid).Returns(true);
            _modelMock.Setup(m => m.CreateBasicProperties())
                      .Returns(new Mock<IBasicProperties>().Object);
            _modelMock.Setup(m => m.BasicPublish(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<IBasicProperties>(), It.IsAny<byte[]>()))
                .Callback<string, string, bool, IBasicProperties, byte[]>((a, b, c, d, e) =>
                {
                    _exchangeNameCallback = a;
                    _routingKeyCallback = b;
                    _basicPropertiesCallback = d;
                    _messageBodyCallback = e;
                });
            _validationHelperMock.Setup(v => v.TryValidate(It.IsAny<TestMessage>(), out _validationOutput))
                                 .Returns(true);
            _serializerMock.Setup(s => s.SerializeObject(It.IsAny<TestMessage>()))
                           .Returns(_messageString);
            _connectionFactoryMock.Setup(c => c.CreateConnection(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                                  .Returns(_connectionHandlerMock.Object);
            _connectionHandlerMock.Setup(c => c.CreateModel())
                                  .Returns(_modelMock.Object);


            _queuePublisher = new PUB.QueuePublisher<TestMessage>(
                _queueConfigurationMock.Object,
                _connectionFactoryMock.Object,
                _serializerMock.Object,
                _validationHelperMock.Object,
                _publisherName,
                _cancellationToken);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_QueueConfigurationNull_ThrowsArgumentNullException()
        {
            new PUB.QueuePublisher<TestMessage>(
                null,
                _connectionFactoryMock.Object,
                _serializerMock.Object,
                _validationHelperMock.Object,
                _publisherName,
                _cancellationToken);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_ConnectionFactoryNull_ThrowsArgumentNullException()
        {
            new PUB.QueuePublisher<TestMessage>(
                _queueConfigurationMock.Object,
                null,
                _serializerMock.Object,
                _validationHelperMock.Object,
                _publisherName,
                _cancellationToken);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_SerializerNull_ThrowsArgumentNullException()
        {
            new PUB.QueuePublisher<TestMessage>(
                _queueConfigurationMock.Object,
                _connectionFactoryMock.Object,
                null,
                _validationHelperMock.Object,
                _publisherName,
                _cancellationToken);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_ValidationHelperNull_ThrowsArgumentNullException()
        {
            new PUB.QueuePublisher<TestMessage>(
                _queueConfigurationMock.Object,
                _connectionFactoryMock.Object,
                _serializerMock.Object,
                null,
                _publisherName,
                _cancellationToken);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_PublisherNameNull_ThrowsArgumentNullException()
        {
            new PUB.QueuePublisher<TestMessage>(
                _queueConfigurationMock.Object,
                _connectionFactoryMock.Object,
                _serializerMock.Object,
                _validationHelperMock.Object,
                null,
                _cancellationToken);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Constructor_QueueConfigurationInvalid_ThrowsArgumentException()
        {
            // setup
            _queueConfigurationMock.Setup(q => q.IsValid).Returns(false);

            // test
            new PUB.QueuePublisher<TestMessage>(
                _queueConfigurationMock.Object,
                _connectionFactoryMock.Object,
                _serializerMock.Object,
                _validationHelperMock.Object,
                _publisherName,
                _cancellationToken);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_PublisherConfigNull_ThrowsArgumentNullException()
        {
            new PUB.QueuePublisher<TestMessage>(
                _queueConfigurationMock.Object,
                _connectionFactoryMock.Object,
                _serializerMock.Object,
                _validationHelperMock.Object,
                "invalidPublisherName",
                _cancellationToken);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Publish_MessageNull_ThrowsArgumentNullException()
        {
            // test
            _queuePublisher.Publish(null);
        }

        [TestMethod]
        public void PublishMessage_TryValidateFails_Disconnects()
        {
            // setup
            _validationHelperMock.Setup(v => v.TryValidate(It.IsAny<TestMessage>(), out _validationOutput))
                                 .Returns(false);

            // test
            _queuePublisher.Publish(message);

            // verify
            _validationHelperMock.Verify(v => v.TryValidate(It.IsAny<TestMessage>(), out _validationOutput), Times.Once);
            _serializerMock.Verify(s => s.SerializeObject(It.IsAny<TestMessage>()), Times.Never);
            _connectionFactoryMock.Verify(c => c.CreateConnection(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            _connectionHandlerMock.Verify(c => c.CreateModel(), Times.Never);
            _modelMock.Verify(m => m.CreateBasicProperties(), Times.Never);
            _modelMock.Verify(m => m.BasicPublish(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<IBasicProperties>(), It.IsAny<byte[]>()), Times.Never);

            Assert.IsFalse(IsConnected());
        }

        [TestMethod]
        public void PublishMessage_SerializeObjectThrowsException_Disconnects()
        {
            // setup
            _serializerMock.Setup(s => s.SerializeObject(It.IsAny<TestMessage>()))
                           .Throws(new Exception());

            // test
            _queuePublisher.Publish(message);

            // verify
            _validationHelperMock.Verify(v => v.TryValidate(It.IsAny<TestMessage>(), out _validationOutput), Times.Once);
            _serializerMock.Verify(s => s.SerializeObject(It.IsAny<TestMessage>()), Times.Once);
            _connectionFactoryMock.Verify(c => c.CreateConnection(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            _connectionHandlerMock.Verify(c => c.CreateModel(), Times.Never);
            _modelMock.Verify(m => m.CreateBasicProperties(), Times.Never);
            _modelMock.Verify(m => m.BasicPublish(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<IBasicProperties>(), It.IsAny<byte[]>()), Times.Never);

            Assert.IsFalse(IsConnected());
        }

        [TestMethod]
        public void PublishMessage_CreateConnectionThrowsException_Disconnects()
        {
            // setup
            _connectionFactoryMock.Setup(c => c.CreateConnection(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                                  .Throws(new Exception());

            // test
            _queuePublisher.Publish(message);

            // verify
            _validationHelperMock.Verify(v => v.TryValidate(It.IsAny<TestMessage>(), out _validationOutput), Times.Once);
            _serializerMock.Verify(s => s.SerializeObject(It.IsAny<TestMessage>()), Times.Once);
            _connectionFactoryMock.Verify(c => c.CreateConnection(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            _connectionHandlerMock.Verify(c => c.CreateModel(), Times.Never);
            _modelMock.Verify(m => m.CreateBasicProperties(), Times.Never);
            _modelMock.Verify(m => m.BasicPublish(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<IBasicProperties>(), It.IsAny<byte[]>()), Times.Never);

            Assert.IsFalse(IsConnected());
        }

        [TestMethod]
        public void Publish_Success()
        {
            // test
            _queuePublisher.Publish(message);

            // verify
            _validationHelperMock.Verify(v => v.TryValidate(It.IsAny<TestMessage>(), out _validationOutput), Times.Once);
            _serializerMock.Verify(s => s.SerializeObject(It.IsAny<TestMessage>()), Times.Once);
            _connectionFactoryMock.Verify(c => c.CreateConnection(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            _connectionHandlerMock.Verify(c => c.CreateModel(), Times.Once);
            _modelMock.Verify(m => m.CreateBasicProperties(), Times.Once);
            _modelMock.Verify(m => m.BasicPublish(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<IBasicProperties>(), It.IsAny<byte[]>()), Times.Once);

            // verify message publishing contents
            Assert.AreEqual(_queueConfigurationMock.Object.Publishers.First().ExchangeName, _exchangeNameCallback);
            Assert.AreEqual(_queueConfigurationMock.Object.Publishers.First().RoutingKey, _routingKeyCallback);
            Assert.AreEqual(_messageString, Encoding.UTF8.GetString(_messageBodyCallback));

            Assert.IsTrue(IsConnected());
        }

        [TestMethod]
        public void Publish_DynamicRoutingKey_Success()
        {
            // test
            _queuePublisher.Publish(message, _dynamicRoutingKey);

            // verify
            _validationHelperMock.Verify(v => v.TryValidate(It.IsAny<TestMessage>(), out _validationOutput), Times.Once);
            _serializerMock.Verify(s => s.SerializeObject(It.IsAny<TestMessage>()), Times.Once);
            _connectionFactoryMock.Verify(c => c.CreateConnection(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            _connectionHandlerMock.Verify(c => c.CreateModel(), Times.Once);
            _modelMock.Verify(m => m.CreateBasicProperties(), Times.Once);
            _modelMock.Verify(m => m.BasicPublish(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<IBasicProperties>(), It.IsAny<byte[]>()), Times.Once);

            // verify message publishing contents
            Assert.AreEqual(_queueConfigurationMock.Object.Publishers.First().ExchangeName, _exchangeNameCallback);
            Assert.AreEqual(_dynamicRoutingKey, _routingKeyCallback);
            Assert.AreEqual(_messageString, Encoding.UTF8.GetString(_messageBodyCallback));

            Assert.IsTrue(IsConnected());
        }

        private bool IsConnected()
        {
            return (bool)((FieldInfo)GetPrivateMemberInfo("_connected")).GetValue(_queuePublisher);
        }

        private MemberInfo GetPrivateMemberInfo(string memberName)
        {
            return _queuePublisher.GetType().GetMembers(BindingFlags.NonPublic | BindingFlags.Instance)
                                            .Where(x => x.Name == memberName)
                                            .FirstOrDefault();
        }
    }
}
