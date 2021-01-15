using System;
using Words.API.DataModels;

namespace Words.API.ViewModels
{
    public class SwapResult : StateResult
    {
        public SwapResult(GameState gameState, string currentPlayerId) : base(gameState, currentPlayerId) { }
        public SwapResult(Exception exception, ErrorType errorType) : base(exception, errorType) { }
    }
}
