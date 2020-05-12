using Newtonsoft.Json;

namespace CommonUtils.Serializer
{
    public interface IJsonSerializer : ISerializer
    {
        /// <summary>
        /// Serializes the specified <paramref name="value"/> to a string representation.
        /// </summary>
        /// <typeparam name="T">The type of the object to serialize.</typeparam>
        /// <param name="value">The value to serialize.</param>
        /// <param name="serializerSettings">The serializer settings to apply.</param>
        /// <returns>The serialized representation of <paramref name="value"/>.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        string SerializeObject<T>(T value, JsonSerializerSettings serializerSettings);

        /// <summary>
        /// Deserializes the specified string into an object of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize.</typeparam>
        /// <param name="value">The string to deserialize.</param>
        /// <param name="serializerSettings">The serializer settings to apply.</param>
        /// <returns>The deserialized object.</returns>
        T Deserialize<T>(string value, JsonSerializerSettings serializerSettings);
    }
}
