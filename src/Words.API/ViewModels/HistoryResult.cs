using System;
using System.Collections.Generic;
using System.Linq;
using Words.API.DataModels;

namespace Words.API.ViewModels
{
    public class HistoryResult
    {
        public IEnumerable<TurnResult> Turns { get; set; }
        public int TilesLeft { get; set; }
        public string GameStatus { get; set; }

        public ErrorResult ErrorResult { get; set; }

        public HistoryResult(Exception exception, ErrorType errorType)
        {
            ErrorResult = new ErrorResult(exception, errorType);
        }

        public HistoryResult(History history, IEnumerable<Player> players)
        {
            if (history == null) throw new ArgumentNullException(nameof(history));
            if (players == null) throw new ArgumentNullException(nameof(players));

            var turnResults = new List<TurnResult>();
            foreach (var turn in history?.Turns)
            {
                var player = players.FirstOrDefault(p => p.PlayerId.Value == turn.PlayerId.Value);
                if (player != null)
                {
                    var turnResult = new TurnResult(player.PlayerName.Value, GetAction(turn.Action), turn.Score, turn.Words);
                    turnResults.Add(turnResult);
                }
            }

            TilesLeft = history.TilesLeft;
            GameStatus = history.GameStatus.ToString();
            Turns = turnResults;
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
