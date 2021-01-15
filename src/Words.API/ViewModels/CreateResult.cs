using System;
using System.Collections.Generic;
using Words.API.DataModels;

namespace Words.API.ViewModels
{
    public class CreateResult
    {
        public string GameId { get; set; }
        public string PlayerId { get; set; }
        public IEnumerable<string> Rulesets { get; set; }
        public ErrorResult ErrorResult { get; set; }

        public CreateResult()  { }

        public CreateResult(CreatedGame createdGame) 
        {
            if (createdGame == null) throw new ArgumentNullException(nameof(createdGame));

            GameId = createdGame.GameId.Value;
            PlayerId = createdGame.Player.PlayerId.Value;
            Rulesets = GameRules.Rulesets;
        }

        public CreateResult(Exception exception, ErrorType errorType)
        {
            ErrorResult = new ErrorResult(exception, errorType);
        }
    }
}
