using System;
using System.Runtime.Serialization;

namespace helpers.Exceptions
{
    [Serializable]
    internal class ParamerException : Exception
    {
        public ParamerException()
        {
        }

        public ParamerException(string message) : base(message)
        {
        }

        public ParamerException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ParamerException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}