using Newtonsoft.Json;

namespace Covid.CommonUtils.Serializers
{
    public interface IJsonSerializer
    {
        T DeserializeObject<T>(string input, JsonSerializerSettings serializerSettings = null);

        string SerializeObject<T>(T input, JsonSerializerSettings serializerSettings = null);
    }
}
