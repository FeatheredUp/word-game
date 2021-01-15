using System;
using System.Runtime.Serialization;

namespace Words.API.Exceptions
{
    [Serializable]
    public class NoMoreLettersException : ValidationException
    {
        public NoMoreLettersException() : base("There are no more tiles available.") { }

        protected NoMoreLettersException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public NoMoreLettersException(string message, Exception innerException) : base(message, innerException) { }

        public NoMoreLettersException(string message) : base(message) { }
    }
}
