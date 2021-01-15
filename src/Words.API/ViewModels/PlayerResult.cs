using System;
using System.Collections.Generic;
using System.Linq;
using Words.API.DataModels;

namespace Words.API.ViewModels
{
    public class PlayerResult
    {
        public string PlayerName { get; set; }
        public bool IsCurrentPlayer { get; set; }
        public int Score { get; set; }
        public int TurnsToWait { get; set; }

        public int LastScore { get; set; }
        public List<string> LastWords { get; set; }
        public TurnActionResult LastAction { get; set; }

        public PlayerResult(Player player, string playerId, TurnOrder turn)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));
            if (playerId == null) throw new ArgumentNullException(nameof(playerId));
            if (turn == null) throw new ArgumentNullException(nameof(turn));

            PlayerName = player.PlayerName.Value;
            IsCurrentPlayer = playerId == player.PlayerId.Value;
            Score = player.Score;
            TurnsToWait = turn.TurnsToWait(player.PlayerId);

            if (player.LastTurn != null)
            {
                LastScore = player.LastTurn.Score;
                LastWords = player.LastTurn.Words?.ToList();
                LastAction = GetAction(player.LastTurn.Action);
            }
        }

        private static TurnActionResult GetAction(TurnAction action)
        {
            switch (action)
            {
                case TurnAction.Play:
                    return TurnActionResult.Play;
                case TurnAction.Swap:
                    return TurnActionResult.Swap;
                case TurnAction.Pass:
                    return TurnActionResult.Pass;
                case TurnAction.EndGame:
                    return TurnActionResult.EndGame;
                default:
                    return TurnActionResult.NotSet;
            }
        }
    }
}