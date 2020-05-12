using System.Collections;
using System.IO;
using System.Text;
using System.Xml;
using Ser = System.Xml.Serialization;

namespace CommonUtils.Serializer
{
    /// <summary>
    /// Serializes an object to XML.
    /// </summary>
    public sealed class XmlSerializer : IXmlSerializer, ISerializer
    {
        private XmlReaderSettings xmlReaderSettings;
        private XmlWriterSettings xmlWriterSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlSerializer" /> class.
        /// </summary>
        public XmlSerializer()
        {
            xmlReaderSettings = new XmlReaderSettings();
            xmlWriterSettings = new XmlWriterSettings();
            xmlWriterSettings.Indent = true;
        }

        /// <summary>
        /// Serializes the specified <paramref name="value"/> into XML using the specified <paramref name="encoding"/> and <paramref name="namespaces"/>.
        /// </summary>
        /// <typeparam name="T">The type of the value to serialize.</typeparam>
        /// <param name="value">The value to serialize.</param>
        /// <param name="encoding">The encoding to use when serializing.</param>
        /// <param name="namespaces">The namespaces to use when serializing.</param>
        /// <returns></returns>
        public string SerializeObject<T>(T value, Encoding encoding, Ser.XmlSerializerNamespaces namespaces)
        {
            string result = string.Empty;

            xmlWriterSettings.Encoding = encoding;

            using (StringWriterWithEncoding stringWriter = new StringWriterWithEncoding(encoding))
            {
                XmlWriter xmlWriter = XmlWriter.Create(stringWriter, xmlWriterSettings);

                Ser.XmlSerializer serializer = new Ser.XmlSerializer(typeof(T));
                if (namespaces == null)
                {
                    serializer.Serialize(xmlWriter, value);
                }
                else
                {
                    serializer.Serialize(xmlWriter, value, namespaces);
                }
                result = stringWriter.ToString();
            }

            return result;
        }

        /// <summary>
        /// Serializes the specified <paramref name="value"/> into XML using the specified <paramref name="namespaces"/>.
        /// </summary>
        /// <typeparam name="T">The type of the value to serialize.</typeparam>
        /// <param name="value">The value to serialize.</param>
        /// <param name="namespaces">The namespaces to use when serializing.</param>
        /// <returns></returns>
        public string SerializeObject<T>(T value, Ser.XmlSerializerNamespaces namespaces)
        {
            return this.SerializeObject(value, Encoding.UTF8, namespaces);
        }

        /// <summary>
        /// Serializes the specified <paramref name="value"/> into XML using the specified <paramref name="encoding"/>.
        /// </summary>
        /// <typeparam name="T">The type of the value to serialize.</typeparam>
        /// <param name="value">The value to serialize.</param>
        /// <param name="encoding">The encoding to use when serializing.</param>
        /// <returns></returns>
        public string SerializeObject<T>(T value, Encoding encoding)
        {
            return this.SerializeObject(value, encoding, null);
        }

        /// <summary>
        /// Serializes the specified <paramref name="value"/> to a string representation.
        /// </summary>
        /// <typeparam name="T">The type of the object to serialize.</typeparam>
        /// <param name="value">The value to serialize.</param>
        /// <returns>The serialized representation of <paramref name="value"/>.</returns>
        public string SerializeObject<T>(T value)
        {
            return this.SerializeObject(value, Encoding.UTF8);
        }

        /// <summary>
        /// Deserializes the specified string into an object of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize.</typeparam>
        /// <param name="value">The string to deserialize.</param>
        /// <returns>The deserialized object.</returns>
        public T Deserialize<T>(string value)
        {
            object obj = null;

            using (XmlReader xmlReader = XmlReader.Create(new StringReader(value), xmlReaderSettings))
            {
                Ser.XmlSerializer deserializer = new Ser.XmlSerializer(typeof(T));
                obj = (T)deserializer.Deserialize(xmlReader);
            }

            return (T)obj;
        }
    }
}
