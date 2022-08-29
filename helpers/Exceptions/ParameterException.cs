using System;
using System.Runtime.Serialization;

namespace helpers.Exceptions
{
    [Serializable]
    internal class ParameterException : Exception
    {
        public ParameterException()
        {
        }

        public ParameterException(string message) : base(message)
        {
        }

        public ParameterException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ParameterException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}