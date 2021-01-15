using System;
using System.Runtime.Serialization;
using Words.API.DataModels;

namespace Words.API.Exceptions
{
    [Serializable]
    public class PlayerNotInGameException : ValidationException
    {
        public PlayerNotInGameException(GameId gameId, PlayerId playerId) : base($"Game {gameId} does not contain player with Id {playerId}.") { }

        protected PlayerNotInGameException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public PlayerNotInGameException() : base("That game does not contain a player with that Id.") { }

        public PlayerNotInGameException(string message) : base(message) { }

        public PlayerNotInGameException(string message, Exception innerException) : base(message, innerException) { }
    }
}