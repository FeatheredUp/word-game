using System;
using System.Runtime.Serialization;

namespace Words.API.Exceptions
{
    [Serializable]
    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message) { }

        protected ValidationException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public ValidationException() : base("That is not valid.") { }

        public ValidationException(string message, Exception innerException) : base(message, innerException) { }
    }
}