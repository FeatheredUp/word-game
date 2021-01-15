using System;
using System.Collections.Generic;
using System.Linq;

namespace Words.API.DataModels
{
    public class GameState
    {
        public GameRules GameRules { get; }
        public GameId GameId { get; }
        public TileBag TileBag { get; }
        public List<Player> Players { get; }
        public Board Board { get; }
        public TurnOrder Turn { get; }
        public GameStatus GameStatus { get; private set; }

        public GameState(GameId gameId, GameRules gameRules, List<Player> players, GameStatus status)
        {
            if (gameId == null) throw new ArgumentNullException(nameof(gameId));
            if (gameRules == null) throw new ArgumentNullException(nameof(gameRules));
            if (players == null) throw new ArgumentNullException(nameof(players));

            GameId = gameId;
            GameRules = gameRules;
            Players = players;
            GameStatus = status;

            TileBag = new TileBag(gameRules.Letters.ToList());
            Turn = new TurnOrder(Players);
            Board = new Board(gameRules);
            foreach (var p in Players)
            {
                p.AddRack(TileBag.GetNext(gameRules.RackSize));
            }
        }

        public GameState(GameId gameId, List<Player> players, TileBag bag, TurnOrder turn, Board board, GameRules rules, GameStatus status)
        {
            GameId = gameId;
            Players = players;
            TileBag = bag;
            Turn = turn;
            Board = board;
            GameRules = rules;
            GameStatus = status;
        }

        public void SetFinished()
        {
            GameStatus = GameStatus.Completed;
        }
    }
}
