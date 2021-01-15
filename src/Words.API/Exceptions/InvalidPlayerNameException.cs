using System;
using System.Runtime.Serialization;

namespace Words.API.Exceptions
{
    [Serializable]
    public class InvalidPlayerNameException : ValidationException
    {
        public InvalidPlayerNameException(string name) : base($"Player name {name} is not valid.") { }

        protected InvalidPlayerNameException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public InvalidPlayerNameException() : base("That player name is not valid.") { }

        public InvalidPlayerNameException(string message, Exception innerException) : base(message, innerException) { }
    }
}