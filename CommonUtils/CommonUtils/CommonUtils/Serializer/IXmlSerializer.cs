using System.Text;
using System.Xml.Serialization;

namespace CommonUtils.Serializer
{
    public interface IXmlSerializer : ISerializer
    {
        /// <summary>
        /// Serializes the specified <paramref name="value"/> into XML using the specified <paramref name="encoding"/>.
        /// </summary>
        /// <typeparam name="T">The type of the value to serialize.</typeparam>
        /// <param name="value">The value to serialize.</param>
        /// <param name="encoding">The encoding to use when serializing.</param>
        /// <returns></returns>
        string SerializeObject<T>(T value, Encoding encoding);

        /// <summary>
        /// Serializes the specified <paramref name="value"/> into XML using the specified <paramref name="namespaces"/>.
        /// </summary>
        /// <typeparam name="T">The type of the value to serialize.</typeparam>
        /// <param name="value">The value to serialize.</param>
        /// <param name="namespaces">The namespaces to use when serializing.</param>
        /// <returns></returns>
        string SerializeObject<T>(T value, XmlSerializerNamespaces namespaces);

        /// <summary>
        /// Serializes the specified <paramref name="value"/> into XML using the specified <paramref name="encoding"/> and <paramref name="namespaces"/>.
        /// </summary>
        /// <typeparam name="T">The type of the value to serialize.</typeparam>
        /// <param name="value">The value to serialize.</param>
        /// <param name="encoding">The encoding to use when serializing.</param>
        /// <param name="namespaces">The namespaces to use when serializing.</param>
        /// <returns></returns>
        string SerializeObject<T>(T value, Encoding encoding, XmlSerializerNamespaces namespaces);
    }
}