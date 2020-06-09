using CommonUtils.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using RabbitMQWrapper.Consumer;
using RabbitMQWrapper.Model;
using RabbitMQWrapper.UnitTest.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using EVT = RabbitMQWrapper.EventListeners;

namespace RabbitMQWrapper.UnitTest.EventListeners
{
    [TestClass]
    public class SequentialProcessingEventListener
    {
        public class TestSequentialEventListener : EVT.SequentialProcessingEventListener<TestMessage>
        {
            public TestSequentialEventListener(IQueueConsumer<TestMessage> queueConsumer)
                : base(queueConsumer)
            {
            }

            protected override Task ProcessMessageAsync(TestMessage message, ulong deliveryTag, CancellationToken cancellationToken, string routingKey = null)
            {
                if (message.Property == "AlreadyClosedException")
                    throw new AlreadyClosedException(new ShutdownEventArgs(ShutdownInitiator.Peer, 1, null));
                if (message.Property == "FatalErrorException")
                    throw new FatalErrorException();
                if (message.Property == "Exception")
                    throw new Exception();

                return Task.CompletedTask;
            }
        }

        private Mock<IQueueConsumer<TestMessage>> _queueConsumerMock;
        private QueueMessage<TestMessage> _message;
        private ulong _deliveryTag = 111;
        private string _routingKey = "routingKey";
        private Dictionary<string, object> _headers = new Dictionary<string, object>();

        private CancellationToken _cancellationToken = new CancellationTokenSource().Token;
        private MethodInfo messageReceivedAsyncMethodInfo;

        TestSequentialEventListener _eventListener;

        [TestInitialize]
        public void TestSetup()
        {
            _queueConsumerMock = new Mock<IQueueConsumer<TestMessage>>();

            _message = new QueueMessage<TestMessage>(new TestMessage(), _deliveryTag, _routingKey, _headers);

            _eventListener = new TestSequentialEventListener(_queueConsumerMock.Object);

            // get a reference to the MessageReceivedAsync private method so it can be tested
            messageReceivedAsyncMethodInfo = _eventListener.GetType().BaseType.GetMethod("MessageReceivedAsync", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_QueueConsumerNull_ThrowsArgumentNullException()
        {
            new TestSequentialEventListener(null);
        }

        [TestMethod]
        public void MessageReceivedAsync_Success()
        {
            // test
            //    ((Task)messageReceivedAsyncMethodInfo.Invoke(_eventListener, new object[]
            //    {
            //        _message,
            //        _cancellationToken
            //    })).GetAwaiter().GetResult();

            //    var s = ProcessingQueue();
            //    var tasks = Tasks();
            //    Task.WhenAll(tasks.Select(t => t.Value)).GetAwaiter().GetResult();

            //    _queueConsumerMock.Verify(q => q.AcknowledgeMessage(It.IsAny<ulong>()), Times.Once);
            //    _queueConsumerMock.Verify(q => q.NegativelyAcknowledge(It.IsAny<ulong>()), Times.Never);
            //    _queueConsumerMock.Verify(q => q.NegativelyAcknowledgeAndRequeue(It.IsAny<ulong>()), Times.Never);
        }

        private ConcurrentDictionary<string, ProcessingQueue<TestMessage>> ProcessingQueue()
        {
            return (ConcurrentDictionary<string, ProcessingQueue<TestMessage>>)((FieldInfo)GetPrivateMemberInfo("_processingQueues")).GetValue(_eventListener);
        }

        private ConcurrentDictionary<string, Task> Tasks()
        {
            return (ConcurrentDictionary<string, Task>)((FieldInfo)GetPrivateMemberInfo("_tasks")).GetValue(_eventListener);
        }

        private MemberInfo GetPrivateMemberInfo(string memberName)
        {
            return _eventListener.GetType().GetMembers(BindingFlags.NonPublic | BindingFlags.Instance)
                                           .Where(x => x.Name == memberName)
                                           .FirstOrDefault();
        }
    }
}
