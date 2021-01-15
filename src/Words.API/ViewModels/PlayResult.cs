using System;
using System.Collections.Generic;
using System.Linq;
using Words.API.DataModels;

namespace Words.API.ViewModels
{
    public class PlayResult : StateResult
    {
        public List<string> LastWords { get; }
        public int LastScore { get; set; }

        public PlayResult(GameState gameState, string currentPlayerId) : base(gameState, currentPlayerId)
        {
            var currentPlayer = gameState.Players.FirstOrDefault(p => p.PlayerId.Value == currentPlayerId);
            LastWords = currentPlayer.LastTurn?.Words?.ToList();
            LastScore = currentPlayer.LastTurn.Score;
        }

        public PlayResult(Exception exception, ErrorType errorType) : base(exception, errorType) { }
    }
}
