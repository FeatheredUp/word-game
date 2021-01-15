using System;
using System.Collections.Generic;
using System.Linq;
using Words.API.Exceptions;

namespace Words.API.DataModels
{
    public class Rack
    {
        public List<string> Letters { get; private set; }
        public int MaxLength { get; }

        public Rack(List<string> letters)
        {
            if (letters == null) throw new ArgumentNullException(nameof(letters));

            Letters = letters;
            MaxLength = letters.Count;
        }

        public Rack(string commaSeparatedLetters)
        {
            if (string.IsNullOrEmpty(commaSeparatedLetters))
            {
                Letters = new List<string>();
                MaxLength = 0;
            }
            else
            {
                Letters = commaSeparatedLetters.Split(",").ToList();
                MaxLength = Letters.Count;
            }
        }

        public void RemoveLetter(string letter)
        {
            if (!Letters.Contains(letter)) throw new LetterNotOnRackException(Letters.ToString(), letter);
            if (Letters.Count == 0) throw new EmptyRackException(letter);

            Letters.Remove(letter);
        }

        public void AddLetter(string letter)
        {
            if (letter == null) throw new ArgumentNullException(nameof(letter));
            if (Letters.Count == MaxLength) throw new FullRackException(Letters.ToString(), letter);

            Letters.Add(letter);
        }

        public override string ToString()
        {
            return $"[{string.Join(", ",Letters)}]";
        }
    }
}