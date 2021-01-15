using System;
using System.Runtime.Serialization;
using Words.API.DataModels;

namespace Words.API.Exceptions
{
    [Serializable]
    public class GameAtCapacityException : ValidationException
    {
        public GameAtCapacityException(GameId gameId) : base($"Game {gameId} already has the maximum number of players.") { }

        protected GameAtCapacityException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public GameAtCapacityException() : base("That game already has the maximum number of players.") { }

        public GameAtCapacityException(string message, Exception innerException) : base(message, innerException) { }

        public GameAtCapacityException(string message) : base(message) { }
    }
}