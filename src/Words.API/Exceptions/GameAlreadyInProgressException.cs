using System;
using System.Runtime.Serialization;
using Words.API.DataModels;

namespace Words.API.Exceptions
{
    [Serializable]
    public class GameAlreadyInProgressException : ValidationException
    {
        public GameAlreadyInProgressException(GameId gameId) : base($"Game {gameId} is already in progress.") { }

        protected GameAlreadyInProgressException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public GameAlreadyInProgressException() : base("That game is already in progress.") { }

        public GameAlreadyInProgressException(string message, Exception innerException) : base(message, innerException) { }

        public GameAlreadyInProgressException(string message) : base(message) { }
    }
}