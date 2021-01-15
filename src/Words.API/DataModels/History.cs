using System;
using System.Collections.Generic;
using System.Linq;

namespace Words.API.DataModels
{
    public class History
    {
        public GameStatus GameStatus { get;  }
        public IReadOnlyList<Turn> Turns {get;}
        public int TilesLeft { get; }

        public History(GameStatus gameStatus, IEnumerable<Turn> turns, int tilesLeft)
        {
            GameStatus = gameStatus;
            Turns = turns.ToList();
            TilesLeft = tilesLeft;
        }
    }
}
