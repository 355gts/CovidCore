namespace CommonUtils.Serializer
{
    /// <summary>
    /// A generic serializer
    /// </summary>
    public interface ISerializer
    {
        /// <summary>
        /// Serializes the specified <paramref name="value"/> to a string representation.
        /// </summary>
        /// <typeparam name="T">The type of the object to serialize.</typeparam>
        /// <param name="value">The value to serialize.</param>
        /// <returns>The serialized representation of <paramref name="value"/>.</returns>
        string SerializeObject<T>(T value);

        /// <summary>
        /// Deserializes the specified string into an object of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize.</typeparam>
        /// <param name="value">The string to deserialize.</param>
        /// <returns>The deserialized object.</returns>
        T Deserialize<T>(string value);
    }
}
