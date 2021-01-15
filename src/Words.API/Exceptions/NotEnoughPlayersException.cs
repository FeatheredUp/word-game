using System;
using System.Runtime.Serialization;
using Words.API.DataModels;

namespace Words.API.Exceptions
{
    [Serializable]
    public class NotEnoughPlayersException : ValidationException
    {
        public NotEnoughPlayersException(GameId gameId) : base($"Not enough players in game {gameId}.") { }

        protected NotEnoughPlayersException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public NotEnoughPlayersException() : base("Not enough players in that game.") { }

        public NotEnoughPlayersException(string message) : base(message) { }

        public NotEnoughPlayersException(string message, Exception innerException) : base(message, innerException) { }
    }
}