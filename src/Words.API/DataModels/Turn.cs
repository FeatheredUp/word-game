using System;
using System.Collections.Generic;

namespace Words.API.DataModels
{
    public class Turn
    {
        public PlayerId PlayerId { get; }
        public TurnAction Action { get;  }
        public int Score { get; }
        public IEnumerable<string> Words { get; }

        public Turn(PlayerId playerId, TurnAction action, int score, string words)
        {
            PlayerId = playerId;
            Action = action;
            Score = score;
            if (string.IsNullOrEmpty(words))
            {
                Words = null;
            }
            else
            {
                Words = words.Split(',');
            }
        }

        public Turn(PlayerId playerId, TurnAction action, int score, IEnumerable<string> words)
        {
            PlayerId = playerId;
            Action = action;
            Score = score;
            Words = words;
        }

        public Turn(PlayerId playerId, TurnAction action, int score)
        {
            PlayerId = playerId;
            Action = action;
            Score = score;
            Words = null;
        }
    }
}
