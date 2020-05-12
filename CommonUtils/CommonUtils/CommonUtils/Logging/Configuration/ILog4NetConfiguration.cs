namespace CommonUtils.Logging.Configuration
{
    public interface ILog4NetConfiguration
    {
        string ComponentName { get; set; }

        string ConfigurationFileName { get; set; }
    }
}