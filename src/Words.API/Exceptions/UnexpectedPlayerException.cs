using System;
using System.Runtime.Serialization;
using Words.API.DataModels;

namespace Words.API.Exceptions
{
    [Serializable]
    public class UnexpectedPlayerException : ValidationException
    {
        public UnexpectedPlayerException(GameId gameId, PlayerId playerId, string task) : base($"Player {playerId} in game {gameId} cannot currently do task '{task}'."){ }

        protected UnexpectedPlayerException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public UnexpectedPlayerException() : base("That player cannot currently do that task.") { }

        public UnexpectedPlayerException(string message) : base(message) { }

        public UnexpectedPlayerException(string message, Exception innerException) : base(message, innerException) { }
    }
}