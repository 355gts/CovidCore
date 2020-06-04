using CommonUtils.Serializer;
using CommonUtils.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQWrapper.Configuration;
using RabbitMQWrapper.Connection;
using RabbitMQWrapper.Factories;
using RabbitMQWrapper.Model;
using RabbitMQWrapper.UnitTest.Extensions;
using RabbitMQWrapper.UnitTest.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Q = RabbitMQWrapper.Consumer;

namespace RabbitMQWrapper.UnitTest.Consumer
{
    [TestClass]
    public class QueueConsumer
    {
        private Mock<IQueueConfiguration> _queueConfigurationMock;
        private Mock<IQueueConnectionFactory> _connectionFactoryMock;
        private Mock<IConnectionHandler> _connectionHandlerMock;
        private Mock<IModel> _modelMock;
        private Mock<IJsonSerializer> _serializerMock;
        private Mock<IValidationHelper> _validationHelperMock;
        private string _consumerName = "consumerName";
        private ulong _deliveryTag = 111;
        private string _routingKey = "routingKey";
        private Dictionary<string, object> _headers = new Dictionary<string, object>();
        private TestMessage _message;
        private byte[] _messageBody;
        private BasicDeliverEventArgs _messageEventArgs;
        private QueueDeclareOk _queueDeclareOk = new QueueDeclareOk(It.IsAny<string>(), It.IsAny<uint>(), It.IsAny<uint>());
        private QueueMessage<TestMessage> _queueMessage;
        private CancellationToken _cancellationToken = new CancellationTokenSource().Token;
        private Func<QueueMessage<TestMessage>, CancellationToken, Task> _onMessageReceived = (a, b) => { return Task.CompletedTask; };
        private string validationOutput;
        private MethodInfo processMessageAsyncMethodInfo;
        private QueueMessage<TestMessage> queueMessageCallback;

        private Q.QueueConsumer<TestMessage> _queueConsumer;

        [TestInitialize]
        public void TestSetup()
        {
            _queueConfigurationMock = new Mock<IQueueConfiguration>().InitialiseMock();
            _connectionFactoryMock = new Mock<IQueueConnectionFactory>();
            _connectionHandlerMock = new Mock<IConnectionHandler>();
            _modelMock = new Mock<IModel>();
            _serializerMock = new Mock<IJsonSerializer>();
            _validationHelperMock = new Mock<IValidationHelper>();

            _message = new TestMessage();
            _messageBody = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_message));
            var basicProperties = new Mock<IBasicProperties>();
            basicProperties.Setup(b => b.Headers).Returns(_headers);
            _messageEventArgs = new BasicDeliverEventArgs()
            {
                Body = _messageBody,
                DeliveryTag = _deliveryTag,
                RoutingKey = _routingKey,
                BasicProperties = basicProperties.Object,
            };
            _queueMessage = new QueueMessage<TestMessage>(_message, _deliveryTag, _routingKey, _headers);
            _onMessageReceived = (a, b) =>
            {
                queueMessageCallback = a;
                return Task.CompletedTask;
            };

            _modelMock.Setup(m => m.IsOpen)
                      .Returns(true);
            _modelMock.Setup(m => m.QueueDeclare(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<IDictionary<string, object>>()))
                      .Returns(_queueDeclareOk);
            _connectionHandlerMock.Setup(c => c.CreateModel())
                                  .Returns(_modelMock.Object);
            _connectionFactoryMock.Setup(c => c.CreateConnection(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                                  .Returns(_connectionHandlerMock.Object);
            _serializerMock.Setup(s => s.Deserialize<TestMessage>(It.IsAny<string>()))
                           .Returns(_message);
            _validationHelperMock.Setup(v => v.TryValidate(It.IsAny<TestMessage>(), out validationOutput))
                                 .Returns(true);

            _queueConsumer = new Q.QueueConsumer<TestMessage>(
                _queueConfigurationMock.Object,
                _connectionFactoryMock.Object,
                _serializerMock.Object,
                _validationHelperMock.Object,
                _consumerName);

            // get a reference to the ProcessMessageAsync private method so it can be tested
            processMessageAsyncMethodInfo = _queueConsumer.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                                                          .Where(x => x.Name == "ProcessMessageAsync" && x.IsPrivate)
                                                          .First();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_QueueConfigurationNull_ThrowsArgumentNullException()
        {
            new Q.QueueConsumer<TestMessage>(
                null,
                _connectionFactoryMock.Object,
                _serializerMock.Object,
                _validationHelperMock.Object,
                _consumerName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_QueueConnectionFactoryNull_ThrowsArgumentNullException()
        {
            new Q.QueueConsumer<TestMessage>(
                _queueConfigurationMock.Object,
                null,
                _serializerMock.Object,
                _validationHelperMock.Object,
                _consumerName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_SerializerNull_ThrowsArgumentNullException()
        {
            new Q.QueueConsumer<TestMessage>(
                _queueConfigurationMock.Object,
                _connectionFactoryMock.Object,
                null,
                _validationHelperMock.Object,
                _consumerName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_ValidationHelperNull_ThrowsArgumentNullException()
        {
            new Q.QueueConsumer<TestMessage>(
                _queueConfigurationMock.Object,
                _connectionFactoryMock.Object,
                _serializerMock.Object,
                null,
                _consumerName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_ConsumerNameNull_ThrowsArgumentNullException()
        {
            new Q.QueueConsumer<TestMessage>(
                _queueConfigurationMock.Object,
                _connectionFactoryMock.Object,
                _serializerMock.Object,
                _validationHelperMock.Object,
                null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_ConsumerNotFound_ThrowsArgumentNullException()
        {
            new Q.QueueConsumer<TestMessage>(
                _queueConfigurationMock.Object,
                _connectionFactoryMock.Object,
                _serializerMock.Object,
                _validationHelperMock.Object,
                "invalidConsumerName");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Run_OnMessageReceivedNull_ThrowsArgumentNullException()
        {
            // test
            _queueConsumer.Run(null, _cancellationToken);

            // verify
            _connectionFactoryMock.Verify(c => c.CreateConnection(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            _connectionHandlerMock.Verify(c => c.CreateModel(), Times.Never);
            _modelMock.Verify(m => m.QueueDeclare(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<IDictionary<string, object>>()), Times.Never);
            _modelMock.Verify(m => m.QueueBind(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IDictionary<string, object>>()), Times.Never);

            Assert.IsFalse(IsConnected());
        }

        [TestMethod]
        public void Run_DynamicQueue_QueueDeclareFails_Disconnects()
        {
            // setup
            _queueConfigurationMock.Object.Consumers.First().QueueName = null;
            _modelMock.Setup(m => m.QueueDeclare(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<IDictionary<string, object>>()))
                      .Returns((QueueDeclareOk)null);
            // test
            _queueConsumer.Run(_onMessageReceived, _cancellationToken);

            // verify
            _connectionFactoryMock.Verify(c => c.CreateConnection(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            _connectionHandlerMock.Verify(c => c.CreateModel(), Times.Once);
            _modelMock.Verify(m => m.QueueDeclare(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<IDictionary<string, object>>()), Times.Once);
            _modelMock.Verify(m => m.QueueBind(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IDictionary<string, object>>()), Times.Never);

            Assert.IsFalse(IsConnected());
        }

        [TestMethod]
        public void Run_CreateConnectionThrowsException_ThrowsIOException()
        {
            // setup
            _connectionFactoryMock.Setup(c => c.CreateConnection(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                                  .Throws(new Exception());
            // test
            _queueConsumer.Run(_onMessageReceived, _cancellationToken);

            // verify
            _connectionFactoryMock.Verify(c => c.CreateConnection(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            _connectionHandlerMock.Verify(c => c.CreateModel(), Times.Never);
            _modelMock.Verify(m => m.QueueDeclare(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<IDictionary<string, object>>()), Times.Never);
            _modelMock.Verify(m => m.QueueBind(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IDictionary<string, object>>()), Times.Never);

            Assert.IsFalse(IsConnected());
        }

        [TestMethod]
        public void Run_DynamicQueue_Success()
        {
            // setup
            _queueConfigurationMock.Object.Consumers.First().QueueName = null;

            // test
            _queueConsumer.Run(_onMessageReceived, _cancellationToken);

            // verify
            _connectionFactoryMock.Verify(c => c.CreateConnection(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            _connectionHandlerMock.Verify(c => c.CreateModel(), Times.Once);
            _modelMock.Verify(m => m.QueueDeclare(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<IDictionary<string, object>>()), Times.Once);
            _modelMock.Verify(m => m.QueueBind(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IDictionary<string, object>>()), Times.Once);

            Assert.IsTrue(IsConnected());
            Assert.IsTrue(QueueName().StartsWith(_queueConfigurationMock.Object.TemporaryQueueNamePrefix));
        }

        [TestMethod]
        public void Run_Success()
        {
            // test
            _queueConsumer.Run(_onMessageReceived, _cancellationToken);

            // verify
            _connectionFactoryMock.Verify(c => c.CreateConnection(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            _connectionHandlerMock.Verify(c => c.CreateModel(), Times.Once);
            _modelMock.Verify(m => m.QueueDeclare(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<IDictionary<string, object>>()), Times.Never);
            _modelMock.Verify(m => m.QueueBind(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IDictionary<string, object>>()), Times.Never);

            Assert.IsTrue(IsConnected());
            Assert.AreEqual(_queueConfigurationMock.Object.Consumers.First().QueueName, QueueName());
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProcessMessageAsync_ModelNull_ThrowsArgumentNullException()
        {
            // test
            ((Task)processMessageAsyncMethodInfo.Invoke(_queueConsumer, new object[]
            {
                null,
                _messageEventArgs,
                _onMessageReceived,
                _cancellationToken
            })).GetAwaiter().GetResult();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProcessMessageAsync_MessageNull_ThrowsArgumentNullException()
        {
            // test
            ((Task)processMessageAsyncMethodInfo.Invoke(_queueConsumer, new object[]
            {
                new object(),
                null,
                _onMessageReceived,
                _cancellationToken
            })).GetAwaiter().GetResult();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProcessMessageAsync_OnMessageReceivedFuncNull_ThrowsArgumentNullException()
        {
            // test
            ((Task)processMessageAsyncMethodInfo.Invoke(_queueConsumer, new object[]
            {
                new object(),
                _messageEventArgs,
                null,
                _cancellationToken
            })).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void ProcessMessageAsync_DeserializeFails_NegativelyAcknowledgesMessage()
        {
            // setup
            _serializerMock.Setup(s => s.Deserialize<TestMessage>(It.IsAny<string>()))
                           .Throws(new Exception());
            // test
            _queueConsumer.Run(_onMessageReceived, _cancellationToken);

            // invoke the private process message method
            ((Task)processMessageAsyncMethodInfo.Invoke(_queueConsumer, new object[]
            {
                new object(),
                _messageEventArgs,
                _onMessageReceived,
                _cancellationToken
            })).GetAwaiter().GetResult();

            //verify
            _serializerMock.Verify(s => s.Deserialize<TestMessage>(It.IsAny<string>()), Times.Once);
            _validationHelperMock.Verify(v => v.TryValidate(It.IsAny<TestMessage>(), out validationOutput), Times.Never);
            _modelMock.Verify(m => m.BasicNack(It.IsAny<ulong>(), false, false), Times.Once);
        }

        [TestMethod]
        public void ProcessMessageAsync_TryValidateFails_NegativelyAcknowledgesMessage()
        {
            // setup
            _validationHelperMock.Setup(v => v.TryValidate(It.IsAny<TestMessage>(), out validationOutput))
                                 .Returns(false);
            // test
            _queueConsumer.Run(_onMessageReceived, _cancellationToken);

            // invoke the private process message method
            ((Task)processMessageAsyncMethodInfo.Invoke(_queueConsumer, new object[]
            {
                new object(),
                _messageEventArgs,
                _onMessageReceived,
                _cancellationToken
            })).GetAwaiter().GetResult();

            //verify
            _serializerMock.Verify(s => s.Deserialize<TestMessage>(It.IsAny<string>()), Times.Once);
            _validationHelperMock.Verify(v => v.TryValidate(It.IsAny<TestMessage>(), out validationOutput), Times.Once);
            _modelMock.Verify(m => m.BasicNack(It.IsAny<ulong>(), false, false), Times.Once);
        }

        [TestMethod]
        public void ProcessMessageAsync_Success()
        {
            // test
            _queueConsumer.Run(_onMessageReceived, _cancellationToken);

            ((Task)processMessageAsyncMethodInfo.Invoke(_queueConsumer, new object[]
            {
                new object(),
                _messageEventArgs,
                _onMessageReceived,
                _cancellationToken
            })).GetAwaiter().GetResult();

            //verify
            _serializerMock.Verify(s => s.Deserialize<TestMessage>(It.IsAny<string>()), Times.Once);
            _validationHelperMock.Verify(v => v.TryValidate(It.IsAny<TestMessage>(), out validationOutput), Times.Once);

            Assert.IsNotNull(queueMessageCallback);
            Assert.AreEqual(_deliveryTag, queueMessageCallback.DeliveryTag);
            Assert.AreEqual(_headers, queueMessageCallback.MessageHeaders);
            Assert.AreEqual(_routingKey, queueMessageCallback.RoutingKey);
            Assert.IsInstanceOfType(queueMessageCallback.Message, typeof(TestMessage));
        }

        private bool IsConnected()
        {
            return (bool)((FieldInfo)GetPrivateMemberInfo("_connected")).GetValue(_queueConsumer);
        }

        private string QueueName()
        {
            return (string)((FieldInfo)GetPrivateMemberInfo("_queueName")).GetValue(_queueConsumer);
        }

        private MemberInfo GetPrivateMemberInfo(string memberName)
        {
            return _queueConsumer.GetType().GetMembers(BindingFlags.NonPublic | BindingFlags.Instance)
                                           .Where(x => x.Name == memberName)
                                           .FirstOrDefault();
        }
    }
}
