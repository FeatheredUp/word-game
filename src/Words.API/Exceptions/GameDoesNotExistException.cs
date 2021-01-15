using System;
using System.Runtime.Serialization;
using Words.API.DataModels;

namespace Words.API.Exceptions
{
    [Serializable]
    public class GameDoesNotExistException : ValidationException
    {
        public GameDoesNotExistException(GameId gameId) : base($"Game {gameId} does not exist.") { }

        protected GameDoesNotExistException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public GameDoesNotExistException() : base("That game does not exist.") { }

        public GameDoesNotExistException(string message, Exception innerException) : base(message, innerException) { }

        public GameDoesNotExistException(string message) : base(message) { }
    }
}