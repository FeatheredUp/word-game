using System;
using System.Runtime.Serialization;
using Words.API.DataModels;

namespace Words.API.Exceptions
{
    [Serializable]
    public class PlayerAlreadyInGameException : ValidationException
    {
        public PlayerAlreadyInGameException(GameId gameId, PlayerName playerName) : base($"Game {gameId} already contains a player named {playerName}.") { }

        protected PlayerAlreadyInGameException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public PlayerAlreadyInGameException() : base("That game already contains a player with that name.") { }

        public PlayerAlreadyInGameException(string message) : base(message) { }

        public PlayerAlreadyInGameException(string message, Exception innerException) : base(message, innerException) { }
    }
}