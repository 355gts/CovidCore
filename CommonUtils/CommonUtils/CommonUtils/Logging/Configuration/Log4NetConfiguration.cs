using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace CommonUtils.Logging.Configuration
{
    [DataContract]
    [Serializable]
    public class Log4NetConfiguration : ILog4NetConfiguration
    {
        [DataMember(IsRequired = true)]
        [JsonProperty("configurationFileName")]
        public string ConfigurationFileName { get; set; }

        [DataMember(IsRequired = true)]
        [JsonProperty("componentName")]
        public string ComponentName { get; set; }
    }
}
