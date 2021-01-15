using System;
using System.Collections.Generic;
using System.Linq;

namespace Words.API.DataModels
{
    public class Player
    {
        public PlayerId PlayerId { get; }
        public PlayerName PlayerName { get; }
        public bool IsCreator { get; }
        public Rack Rack { get; private set; }
        public int Score { get; private set; }
        public Turn LastTurn { get; private set; }

        public Player(PlayerName playerName, bool isCreator)
        {
            PlayerName = playerName;
            PlayerId = new PlayerId();
            IsCreator = isCreator;
        }

        /// <summary>
        /// Create a player from loaded data.
        /// </summary>
        public Player(string playerId, string playerName, bool isCreator, int score, string rack)
        {
            if (playerId == null) throw new ArgumentNullException(nameof(playerId));
            if (playerName == null) throw new ArgumentNullException(nameof(playerName));
            if (rack == null) throw new ArgumentNullException(nameof(rack));

            PlayerId = new PlayerId(playerId);
            PlayerName = new PlayerName(playerName);
            IsCreator = isCreator;
            Score = score;
            Rack = new Rack(rack);
        }

        public void AddRack(List<string> letters)
        {
            Rack = new Rack(letters);
        }

        public void AddTurn(Turn turn)
        {
            LastTurn = turn;
        }

        public void IncreaseScore(int increment, List<string> words)
        {
            Score += increment;
            LastTurn = new Turn(PlayerId, TurnAction.Play, increment, words);
        }

        public void DecreaseScore(int decrement)
        {
            Score -= decrement;
            LastTurn = new Turn(PlayerId, TurnAction.EndGame, -decrement);
        }

        public void Pass()
        {
            LastTurn = new Turn(PlayerId, TurnAction.Pass, 0);
        }

        public void Swap()
        {
            LastTurn = new Turn(PlayerId, TurnAction.Swap, 0);
        }

        public override string ToString()
        {
            return $"{PlayerName} ({PlayerId.Value}) - " +
            $"{Rack?.ToString() ?? "[EMPTY]"} - " +
            $"{Score}"; ;
        }
    }
}
