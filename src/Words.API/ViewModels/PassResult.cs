using System;
using Words.API.DataModels;

namespace Words.API.ViewModels
{
    public class PassResult : StateResult
    {
        public PassResult(GameState gameState, string currentPlayerId) : base(gameState, currentPlayerId) { }
        public PassResult(Exception exception, ErrorType errorType) : base(exception, errorType) { }
    }
}
