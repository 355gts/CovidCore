using Covid.Message.Model.Users;
using Covid.Rabbit.Publisher;
using System;

namespace Covid.Message.Model.Publisher
{
    public class MessagePublisher : IMessagePublisher
    {
        private readonly QueuePublisher<User> _userQueuePublisher;
        public MessagePublisher(QueuePublisher<User> userQueuePublisher)
        {
            _userQueuePublisher = userQueuePublisher ?? throw new ArgumentNullException(nameof(userQueuePublisher));
        }

        public void PublishUserMessage(User message)
        {
            _userQueuePublisher.Publish(message);
        }
    }
}
