using System;
using System.Runtime.Serialization;

namespace CommonUtils.Exceptions
{
    [Serializable]
    public sealed class FatalErrorException : Exception
    {
        public FatalErrorException() : base() { }

        public FatalErrorException(string message) : base(message) { }

        public FatalErrorException(string message, Exception innerException)
            : base(message, innerException)
        { }

        private FatalErrorException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
