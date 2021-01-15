using System.Collections.Generic;

namespace Words.API.ViewModels
{
    public class TurnResult
    {
        public string Player { get; set; }
        public TurnActionResult Action { get; set; }
        public int Score { get; set; }
        public IEnumerable<string> Words { get; }

        public TurnResult(string player, TurnActionResult action, int score, IEnumerable<string> words)
        {
            Player = player;
            Action = action;
            Score = score;
            Words = words;
        }
    }
}