using Covid.Common.HttpClientHelper;
using Covid.Common.Mapper;
using Covid.Message.Model.Publisher;
using Covid.Message.Model.Users;
using Covid.Rabbit;
using Covid.Rabbit.Consumer;
using log4net;
using System;
using System.Threading.Tasks;
using Dom = Covid.Web.Model.Users;

namespace Covid.UserService.EventListeners
{
    public class UserEventListener : EventListener<CreateUser>
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(UserEventListener));
        private readonly IMessagePublisher _messagePublisher;
        private readonly IMapper _mapper;
        private readonly ICovidApiHelper _covidApiHelper;

        public UserEventListener(
            QueueConsumer<CreateUser> userQueueConsumer,
            IMessagePublisher messagePublisher,
            IMapper mapper,
            ICovidApiHelper covidApiHelper)
            : base(userQueueConsumer)
        {
            _messagePublisher = messagePublisher ?? throw new ArgumentNullException(nameof(messagePublisher));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _covidApiHelper = covidApiHelper ?? throw new ArgumentNullException(nameof(covidApiHelper));
        }

        public override async Task ProcessMessageAsync(CreateUser message, ulong deliveryTag, string routingKey = null)
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
