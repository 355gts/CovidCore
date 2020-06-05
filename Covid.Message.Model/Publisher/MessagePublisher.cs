using Covid.Message.Model.Users;
using RabbitMQWrapper.Publisher;
using System;

namespace Covid.Message.Model.Publisher
{
    public class MessagePublisher : IMessagePublisher
    {
        private readonly IQueuePublisher<User> _userQueuePublisher;
        public MessagePublisher(IQueuePublisher<User> userQueuePublisher)
        {
            _userQueuePublisher = userQueuePublisher ?? throw new ArgumentNullException(nameof(userQueuePublisher));
        }

        public void PublishUserMessage(User message)
        {
            _userQueuePublisher.Publish(message);
        }
    }
}
