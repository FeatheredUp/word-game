using System;
using System.Collections.Generic;
using System.Linq;
using Words.API.Exceptions;

namespace Words.API.DataModels
{
    public class TileBag
    {
        public int Count => Letters.Count;
        public List<string> Letters { get; private set; }

        private readonly Random _random = new Random();

        public TileBag(List<string> letters)
        {
            Letters = letters ?? throw new ArgumentNullException(nameof(letters));
            Shuffle(Letters);
        }

        public TileBag(string commaSeparatedLetters)
        {
            if (string.IsNullOrEmpty(commaSeparatedLetters))
            {
                Letters = new List<string>();
            }
            else
            {
                Letters = commaSeparatedLetters.Split(",").ToList();
                Shuffle(Letters);
            }
        }

        public string GetNext()
        {
            if (Letters.Count == 0) return null;

            var next = Letters[0];
            Letters.RemoveAt(0);

            return next;
        }

        public List<string> GetNext(int count)
        {
            if (count < 1) throw new ArgumentOutOfRangeException(nameof(count), $"Value was {count} but should be at least 1.");
            if (count < 1 || Letters.Count < count) throw new NoMoreLettersException();

            var next = Letters.Take(count).ToList();
            Letters.RemoveRange(0, count);

            return next;
        }

        public override string ToString()
        {
            return $"[{string.Join(", ", Letters)}]";
        }

        private void Shuffle<T>(List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = _random.Next(n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        internal void AddLetter(string letter)
        {
            Letters.Add(letter);
        }
    }
}
