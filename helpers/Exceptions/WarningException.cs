
using System;
using System.Runtime.Serialization;

namespace helpers.Exceptions
{
    [Serializable]
    public class WarningException : Exception
    {
        public WarningException()
        {
        }

        public WarningException(string message) : base(message)
        {
        }

        public WarningException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected WarningException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}