using System.IO;
using System.Text;

namespace CommonUtils.Serializer
{
    /// <summary>
    /// creates a string writer with encoding
    /// </summary>
    public class StringWriterWithEncoding : StringWriter
    {
        /// <summary>
        /// The encoding
        /// </summary>
        private readonly Encoding encoding;

        /// <summary>
        /// Initializes a new instance of the <see cref="StringWriterWithEncoding"/> class.
        /// </summary>
        /// <param name="encoding">The encoding.</param>
        public StringWriterWithEncoding(Encoding encoding)
        {
            this.encoding = encoding;
        }

        /// <summary>
        /// Gets the encoding.
        /// </summary>
        /// <value>
        /// The encoding.
        /// </value>
        public override Encoding Encoding
        {
            get
            {
                return this.encoding;
            }
        }
    }
}
