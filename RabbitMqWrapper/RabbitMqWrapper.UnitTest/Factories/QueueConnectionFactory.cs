using CommonUtils.Certificates;
using CommonUtils.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RabbitMQ.Client;
using RabbitMQWrapper.Configuration;
using RabbitMQWrapper.UnitTest.Extensions;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using FAC = RabbitMQWrapper.Factories;

namespace RabbitMQWrapper.UnitTest.Factories
{
    [TestClass]
    public class QueueConnectionFactory
    {
        private Mock<ConnectionFactory> _connectionFactoryMock;
        private Mock<IConnection> _connection;
        private Mock<IModel> _model;
        private Mock<IQueueConfiguration> _queueWrapperConfigMock;
        private Mock<ICertificateHelper> _certificateHelperMock;
        private X509Certificate2Collection x509Certificate2Collection;
        private string _connectionName = "connectionName";
        private CancellationToken _cancellationToken = new CancellationTokenSource().Token;

        private FAC.QueueConnectionFactory _queueConnectionFactory;

        [TestInitialize]
        public void TestSetup()
        {
            _connectionFactoryMock = new Mock<ConnectionFactory>();
            _connection = new Mock<IConnection>();
            _model = new Mock<IModel>();
            _queueWrapperConfigMock = new Mock<IQueueConfiguration>().InitialiseMock();
            _certificateHelperMock = new Mock<ICertificateHelper>();

            _connectionFactoryMock.Setup(c => c.CreateConnection())
                                  .Returns(_connection.Object);
            _certificateHelperMock.Setup(c => c.TryFindCertificate(It.IsAny<string>(), out x509Certificate2Collection))
                                  .Returns(true);

            _queueConnectionFactory = new FAC.QueueConnectionFactory(
                _connectionFactoryMock.Object,
                _queueWrapperConfigMock.Object,
                _certificateHelperMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_ConnectionFactoryNull_ThrowsArgumentNullException()
        {
            new FAC.QueueConnectionFactory(
                null,
                _queueWrapperConfigMock.Object,
                _certificateHelperMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_QueueWrapperConfigNull_ThrowsArgumentNullException()
        {
            new FAC.QueueConnectionFactory(
                _connectionFactoryMock.Object,
                null,
                _certificateHelperMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_CertificateHelperNull_ThrowsArgumentNullException()
        {
            new FAC.QueueConnectionFactory(
                _connectionFactoryMock.Object,
                _queueWrapperConfigMock.Object,
                null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Constructor_QueueConfigInvalid_ThrowsArgumentException()
        {
            // setup
            _queueWrapperConfigMock.Setup(q => q.IsValid).Returns(false);

            // test
            new FAC.QueueConnectionFactory(
                _connectionFactoryMock.Object,
                _queueWrapperConfigMock.Object,
                _certificateHelperMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(FatalErrorException))]
        public void Constructor_TryFindCertificateFails_ThrowsFatalErrorException()
        {
            // setup
            _certificateHelperMock = new Mock<ICertificateHelper>();
            _certificateHelperMock.Setup(c => c.TryFindCertificate(It.IsAny<string>(), out x509Certificate2Collection))
                                  .Returns(false);

            // test
            _queueConnectionFactory = new FAC.QueueConnectionFactory(
                _connectionFactoryMock.Object,
                _queueWrapperConfigMock.Object,
                _certificateHelperMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateConnection_ConnectionNameNull_ThrowsArgumentNullException()
        {
            // test
            var connection = _queueConnectionFactory.CreateConnection(null, _cancellationToken);
        }

        [TestMethod]
        public void CreateConnection_Success_CreatesConnection()
        {
            // test
            var connection = _queueConnectionFactory.CreateConnection(_connectionName, _cancellationToken);

            // verify
            _connectionFactoryMock.Verify(c => c.CreateConnection(), Times.Once);
            _certificateHelperMock.Verify(c => c.TryFindCertificate(It.IsAny<string>(), out x509Certificate2Collection), Times.Once);

            Assert.IsNotNull(connection);
        }
    }
}
