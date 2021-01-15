using System;
using Words.API.DataModels;

namespace Words.API.ViewModels
{
    public class WaitResult : StateResult
    {
        public WaitResult(GameState gameState, string currentPlayerId) : base(gameState, currentPlayerId) { }
        public WaitResult(Exception exception, ErrorType errorType) : base(exception, errorType) { }
    }
}
