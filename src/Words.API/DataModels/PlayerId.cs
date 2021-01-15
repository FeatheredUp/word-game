using System;

namespace Words.API.DataModels
{
    public class PlayerId
    {
        public string Value { get; }

        public PlayerId()
        {
            Value = Guid.NewGuid().ToString();
        }

        public PlayerId(string playerId)
        {
            Value = playerId;
        }

        public override string ToString()
        {
            return Value?.ToString();
        }
    }
}