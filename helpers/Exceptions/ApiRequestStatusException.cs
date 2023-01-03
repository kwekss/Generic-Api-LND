
using System;

namespace helpers.Exceptions
{
    [Serializable]
    public class ApiRequestStatusException : Exception
    {
        public ApiRequestStatusException(int httpStatusCode, string message) : base($"{httpStatusCode}||{message}")
        {
        }
        public ApiRequestStatusException(int httpStatusCode, string message, Exception innerException) : base($"{httpStatusCode}||{message}", innerException)
        {
        }
    }
}