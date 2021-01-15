using System;
using System.Collections.Generic;
using System.Linq;

namespace Words.API.DataModels
{
    public class TurnOrder
    {
        public PlayerId CurrentPlayerId => new PlayerId(_players[_index]);
        public int TurnNumber { get; private set; }

        private readonly List<string> _players;
        private int _index = 0;
        private readonly Random _random = new Random();

        public TurnOrder(IEnumerable<Player> players, int seed = 0) 
        {
            if (players == null) throw new ArgumentNullException(nameof(players));

            if (seed > 0 ) _random = new Random(seed);
            _players = players.Select(p=> p.PlayerId.Value).ToList();
            Shuffle(_players);

            TurnNumber = 1;
        }

        public TurnOrder(IEnumerable<string> playOrder, short turnNumber)
        {
            TurnNumber = turnNumber;
            _players = playOrder.ToList();

            _index = TurnNumber % _players.Count;
        }

        public int TurnsToWait(PlayerId playerId)
        {
            var indexOfCurrentPlayer = _players.FindIndex(p => p == CurrentPlayerId.Value);
            var indexOfThisPlayer = _players.FindIndex(p => p == playerId.Value);

            var result = indexOfThisPlayer - indexOfCurrentPlayer;
            if (result < 0) result += _players.Count;

            return result;
        }

        public void NextTurn()
        {
            TurnNumber += 1;
            _index += 1;
            if (_index == _players.Count) _index = 0;
        }

        public override string ToString()
        {
            return $"[{string.Join(", ", _players)}] - {TurnNumber} ({_index})";
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
    }
}
