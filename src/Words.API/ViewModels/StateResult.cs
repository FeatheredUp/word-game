using System;
using System.Collections.Generic;
using System.Linq;
using Words.API.DataModels;

namespace Words.API.ViewModels
{
    public class StateResult
    {
        // Note this needs to be a jagged array (not rectangular) so javascript can receive it.
        public SquareResult[][] Board { get; set; }
        public List<string> Rack { get; }
        public IEnumerable<PlayerResult> Players { get; set; }
        public int TilesLeft { get; set; }
        public bool IsMyTurn { get; set; }
        public int TurnNumber { get; set; }
        public bool GameOver { get; set; }

        public ErrorResult ErrorResult { get; set; }

        public StateResult(GameState gameState, string currentPlayerId)
        {
            if (gameState == null) throw new ArgumentNullException(nameof(gameState));

            var currentPlayer = gameState.Players.FirstOrDefault(p => p.PlayerId.Value == currentPlayerId);
            Rack = currentPlayer?.Rack.Letters;
            Players = GetPlayers(gameState, currentPlayerId);
            TilesLeft = gameState.TileBag.Count;
            IsMyTurn = gameState.Turn.CurrentPlayerId.Value == currentPlayerId;
            TurnNumber = gameState.Turn.TurnNumber;
            Board = GetBoard(gameState.Board, gameState.GameRules);
            GameOver = gameState.GameStatus == GameStatus.Completed;
        }

        private static SquareResult[][] GetBoard(Board board, GameRules rules)
        {
            if (board == null) return null;

            SquareResult[][] result = new SquareResult[GameRules.MaxRows][];
            for (int row = 1; row <= GameRules.MaxRows; row++)
            {
                result[row-1] = new SquareResult[GameRules.MaxColumns];
                for (int col = 1; col <= GameRules.MaxColumns; col++)
                {
                    var square = board.GetSquare(row, col);

                    var isMaxHeight = square.Height == rules.StackHeight;
                    var squareType = GetSquareType(square.SquareType, board.IsEmpty, isMaxHeight);

                    result[row-1][col-1] = new SquareResult(square.Letter, square.Height, squareType);
                }
            }

            return result;
        }

        public StateResult(Exception exception, ErrorType errorType)
        {
            ErrorResult = new ErrorResult(exception, errorType);
        }

        private static SquareTypeResult GetSquareType(SquareType squareType, bool includeStarting, bool isMaxHeight)
        {
            if (squareType == SquareType.Unplayable) return SquareTypeResult.Unplayable;

            if (squareType == SquareType.Starting && includeStarting) return SquareTypeResult.Starting;

            if (isMaxHeight) return SquareTypeResult.MaxHeight;

            return SquareTypeResult.Standard;
        }

        private static IEnumerable<PlayerResult> GetPlayers(GameState gameState, string currentPlayerId)
        {
            var players = new List<PlayerResult>();
            foreach (var player in gameState.Players)
            {
                players.Add(new PlayerResult(player, currentPlayerId, gameState.Turn));
            }
            return players;
        }
    }
}
