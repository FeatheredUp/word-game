using Words.API.Exceptions;

namespace Words.API.DataModels
{
    public class PlayerName
    {
        public string Value { get; }

        public PlayerName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new InvalidPlayerNameException("<blank>");

            Value = name;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}