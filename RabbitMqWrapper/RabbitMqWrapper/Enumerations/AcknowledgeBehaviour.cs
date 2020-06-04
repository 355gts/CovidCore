namespace RabbitMQWrapper.Enumerations
{
    public enum AcknowledgeBehaviour
    {
        /// <summary>
        /// Acknowledge the message once it has been processed
        /// </summary>
        AfterProcess,

        /// <summary>
        /// Acknowledge the message before it is processed
        /// </summary>
        BeforeProcess,

        /// <summary>
        /// Do not acknowledge the message
        /// </summary>
        Never,

        /// <summary>
        /// Acknowledge the message asynchronously
        /// </summary>
        Async,
    }
}
