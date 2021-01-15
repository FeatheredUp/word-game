using System;
using System.Runtime.Serialization;

namespace Words.API.Exceptions
{
    [Serializable]
    public class InvalidGameIdException : ValidationException
    {
        public InvalidGameIdException(string gameId) : base($"Game {gameId} is not valid.") { }

        protected InvalidGameIdException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public InvalidGameIdException() : base("That game is not valid.") { }

        public InvalidGameIdException(string message, Exception innerException) : base(message, innerException) { }
    }
}