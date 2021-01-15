using System;
using System.Runtime.Serialization;
using Words.API.DataModels;

namespace Words.API.Exceptions
{
    [Serializable]
    public class GameNotStartedException : ValidationException
    {
        public GameNotStartedException(GameId gameId) : base($"Game {gameId} has not started.") { }

        protected GameNotStartedException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public GameNotStartedException() : base("That game has not started.") { }

        public GameNotStartedException(string message, Exception innerException) : base(message, innerException) { }

        public GameNotStartedException(string message) : base(message) { }
    }
}