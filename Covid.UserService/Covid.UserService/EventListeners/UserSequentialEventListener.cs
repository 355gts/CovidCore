using Covid.Common.HttpClientHelper;
using Covid.Common.Mapper;
using Covid.Message.Model.Publisher;
using Covid.Message.Model.Users;
using log4net;
using RabbitMQWrapper.Consumer;
using RabbitMQWrapper.EventListeners;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dom = Covid.Web.Model.Users;

namespace Covid.UserService.EventListeners
{
    sealed class UserSequentialEventListener : SequentialProcessingEventListener<CreateUser2>
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(UserSequentialEventListener));
        private readonly IMessagePublisher _messagePublisher;
        private readonly IMapper _mapper;
        private readonly ICovidApiHelper _covidApiHelper;

        public UserSequentialEventListener(
            IQueueConsumer<CreateUser2> userQueueConsumer,
            IMessagePublisher messagePublisher,
            IMapper mapper,
            ICovidApiHelper covidApiHelper)
            : base(userQueueConsumer)
        {
            _messagePublisher = messagePublisher ?? throw new ArgumentNullException(nameof(messagePublisher));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _covidApiHelper = covidApiHelper ?? throw new ArgumentNullException(nameof(covidApiHelper));
        }

        protected override string GetProcessingSequenceIdentifier(string routingKey)
        {
            return routingKey.Split('.').Skip(1).First();
        }

        protected override async Task ProcessMessageAsync(CreateUser2 message, ulong deliveryTag, CancellationToken cancellationToken, string routingKey = null)
        {
            var newUser = _mapper.Map<CreateUser, Dom.CreateUser>(message);

            var result = await _covidApiHelper.Users.CreateUserAsync(newUser);
            if (!result.Success)
            {
                _logger.Error($"Failed to create user '{message.Firstname} {message.Surname}'");
                return;
            }
            var userId = result.Result;

            var createdUserResult = await _covidApiHelper.Users.GetUserByIdAsync(userId);
            if (!createdUserResult.Success)
            {
                _logger.Error($"Failed to retrieve user with id '{userId}'");
                return;
            }

            _logger.Info($"Successfully created user with id '{userId}', publishing message");

            _messagePublisher.PublishUserMessage(_mapper.Map<Dom.User, User>(createdUserResult.Result));
        }
    }
}
