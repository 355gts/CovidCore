using log4net;
using Newtonsoft.Json;
using System;

namespace Covid.CommonUtils.Serializers
{
    public class JsonSerializer : IJsonSerializer
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(JsonSerializer));

        private readonly JsonSerializerSettings _defaultSerializerSettings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented, DateFormatString = "yyyy/MM/ddTHH:mm:ssZ" };

        public T DeserializeObject<T>(string input, JsonSerializerSettings serializerSettings = null)
        {
            try
            {
                var result = JsonConvert.DeserializeObject<T>(input, serializerSettings != null ? serializerSettings : _defaultSerializerSettings);

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to serialize object, error details '{ex.Message}'", ex);
            }

            return default(T);
        }

        public string SerializeObject<T>(T input, JsonSerializerSettings serializerSettings = null)
        {
            try
            {
                var result = JsonConvert.SerializeObject(input, serializerSettings != null ? serializerSettings : _defaultSerializerSettings);

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to serialize object, error details '{ex.Message}'", ex);
            }
            return null;
        }
    }
}
