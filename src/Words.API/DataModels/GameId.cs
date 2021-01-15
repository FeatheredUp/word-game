using System;
using System.Text;
using Words.API.Exceptions;

namespace Words.API.DataModels
{
    public class GameId
    {
        private const int Length = 4;
        private readonly Random _random = new Random();

        public string Value { get; }

        public GameId()
        {
            Value = GetValidCharacters(Length);
        }

        public GameId(string gameId)
        {
            if (!IsValid(gameId)) throw new InvalidGameIdException(gameId);
            Value = gameId;
        }

        private static bool IsValid(string gameId)
        {
            if (gameId is null) return false;
            if (gameId.Length != Length) return false;
            foreach (var ch in gameId)
            {
                if (!CharacterIsValid(ch)) return false;
            }
            return true;
        }

        private static bool CharacterIsValid(char ch)
        {
            return ch >= 'A' && ch <= 'Z';
        }

        public override string ToString()
        {
            return Value?.ToString();
        }

        private string GetValidCharacters(int length)
        {
            var result = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                result.Append(GetValidCharacter());
            }

            return result.ToString();
        }

        private char GetValidCharacter()
        {
            return (char)_random.Next('A', 'Z');
        }
    }
}