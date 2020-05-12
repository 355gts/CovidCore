using Newtonsoft.Json;
using System;

namespace CommonUtils.Serializer
{
    /// <summary>
    ///     Serializes an object to json.
    /// </summary>
    public sealed class JsonSerializer : IJsonSerializer, ISerializer
    {
        /// <summary>
        /// Serializes the specified <paramref name="value"/> to a string representation.
        /// </summary>
        /// <typeparam name="T">The type of the object to serialize.</typeparam>
        /// <param name="value">The value to serialize.</param>
        /// <returns>The serialized representation of <paramref name="value"/>.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public string SerializeObject<T>(T value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return SerializeObject(value, null);
        }

        /// <summary>
        /// Serializes the specified <paramref name="value"/> to a string representation.
        /// </summary>
        /// <typeparam name="T">The type of the object to serialize.</typeparam>
        /// <param name="value">The value to serialize.</param>
        /// <param name="serializerSettings">The serializer settings to apply.</param>
        /// <returns>The serialized representation of <paramref name="value"/>.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public string SerializeObject<T>(T value, JsonSerializerSettings serializerSettings)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return serializerSettings == null 
                        ? JsonConvert.SerializeObject(value)
                        : JsonConvert.SerializeObject(value, serializerSettings);
        }

        /// <summary>
        /// Deserializes the specified string into an object of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize.</typeparam>
        /// <param name="value">The string to deserialize.</param>
        /// <returns>The deserialized object.</returns>
        public T Deserialize<T>(string value)
        {
            return Deserialize<T>(value, null);
        }

        /// <summary>
        /// Deserializes the specified string into an object of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize.</typeparam>
        /// <param name="value">The string to deserialize.</param>
        /// <param name="serializerSettings">The serializer settings to apply.</param>
        /// <returns>The deserialized object.</returns>
        public T Deserialize<T>(string value, JsonSerializerSettings serializerSettings)
        {
            return serializerSettings == null
                        ? JsonConvert.DeserializeObject<T>(value)
                        : JsonConvert.DeserializeObject<T>(value, serializerSettings);
        }
    }
}
