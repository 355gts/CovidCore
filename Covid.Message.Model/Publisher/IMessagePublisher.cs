using Covid.Message.Model.Users;

namespace Covid.Message.Model.Publisher
{
    public interface IMessagePublisher
    {
        void PublishUserMessage(User message);
    }
}
