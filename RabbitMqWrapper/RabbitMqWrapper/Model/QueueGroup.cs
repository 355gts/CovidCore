namespace RabbitMQWrapper.Model
{
    public sealed class QueueGroup<TGroup> where TGroup : struct
    {
        public bool Success { get; set; }
        public TGroup Group { get; set; }
    }
}
