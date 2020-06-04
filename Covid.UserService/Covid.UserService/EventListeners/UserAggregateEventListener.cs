using CommonUtils.Serializer;
using CommonUtils.Validation;
using Covid.Common.HttpClientHelper;
using Covid.Common.Mapper;
using Covid.Message.Model.Publisher;
using Covid.Message.Model.Users;
using Covid.UserService.Model;
using log4net;
using RabbitMQWrapper.Consumer;
using RabbitMQWrapper.EventListeners;
using RabbitMQWrapper.Model;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dom = Covid.Web.Model.Users;

namespace Covid.UserService.EventListeners
{
    sealed class UserAggregateEventListener : AggregateEventListener<CreateUser3, CreateUserGroup>
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(UserAggregateEventListener));

        private readonly IDictionary<string, QueueGroup<CreateUserGroup>> _queueGroups;
        private readonly object _queueGroupLock = new object();
        private readonly IMessagePublisher _messagePublisher;
        private readonly IMapper _mapper;
        private readonly ICovidApiHelper _covidApiHelper;

        public UserAggregateEventListener(
            IQueueConsumer<CreateUser3> queueConsumer,
            IJsonSerializer serializer,
            IValidationHelper validationHelper,
            IMessagePublisher messagePublisher,
            IMapper mapper,
            ICovidApiHelper covidApiHelper
            )
            : base(queueConsumer, serializer, validationHelper)
        {
            _messagePublisher = messagePublisher ?? throw new ArgumentNullException(nameof(messagePublisher));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _covidApiHelper = covidApiHelper ?? throw new ArgumentNullException(nameof(covidApiHelper));

            _queueGroups = new Dictionary<string, QueueGroup<CreateUserGroup>>();
        }

        protected override TimeSpan TimeoutTimeSpan => TimeSpan.FromSeconds(30);

        protected override TimeSpan MaxTimeoutTimeSpan => TimeSpan.FromSeconds(60);

        protected override Task<QueueGroup<CreateUserGroup>> GetAggregationGroup(CreateUser3 message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            lock (_queueGroupLock)
            {
                if (!_queueGroups.ContainsKey(message.Firstname))
                    _queueGroups.Add(message.Firstname, new QueueGroup<CreateUserGroup>()
                    {
                        Group = new CreateUserGroup()
                        {
                            Firstname = message.Firstname
                        },
                        Success = true
                    });

                return Task.FromResult(_queueGroups[message.Firstname]);
            }
        }

        protected override async Task<bool> TryProcessAggregationGroup(IEnumerable<CreateUser3> messages, CreateUserGroup group, CancellationToken cancellationToken)
        {
            int i = 0;
            foreach (var message in messages)
            {
                var newUser = _mapper.Map<CreateUser, Dom.CreateUser>(message);
                newUser.Surname = $"{newUser.Surname}-{i++}";

                var result = await _covidApiHelper.Users.CreateUserAsync(newUser);
                if (!result.Success)
                {
                    _logger.Error($"Failed to create user '{message.Firstname} {message.Surname}'");
                    return false;
                }
                var userId = result.Result;

                var createdUserResult = await _covidApiHelper.Users.GetUserByIdAsync(userId);
                if (!createdUserResult.Success)
                {
                    _logger.Error($"Failed to retrieve user with id '{userId}'");
                    return false;
                }

                _logger.Info($"Successfully created user with id '{userId}', publishing message");

                _messagePublisher.PublishUserMessage(_mapper.Map<Dom.User, User>(createdUserResult.Result));
            }

            return true;
        }
    }
}
