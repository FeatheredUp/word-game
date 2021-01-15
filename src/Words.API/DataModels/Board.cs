using System;
using System.Collections.Generic;
using System.Text;

namespace Words.API.DataModels
{
    public class Board
    {
        public bool IsEmpty { get
            {
                for (int row = 1; row <= GameRules.MaxRows; row++)
                {
                    for (int column = 1; column <= GameRules.MaxColumns; column++)
                    {
                        if (!string.IsNullOrEmpty(Squares[row - 1][column - 1].Letter)) return false;
                    }
                }

                return true;
            }
        }

        private List<List<BoardSquare>> Squares { get; }

        public Board(GameRules gameRules)
        {
            if (gameRules == null) throw new ArgumentNullException(nameof(gameRules));

            Squares = new List<List<BoardSquare>>();

            for (int rowIndex = 1; rowIndex <= GameRules.MaxRows; rowIndex++)
            {
                Squares.Add(CreateRow(gameRules, rowIndex));
            }
        }

        public BoardSquare GetSquare(TilePlacement placement)
        {
            if (placement == null) throw new ArgumentNullException(nameof(placement));

            return GetSquare(placement.Row, placement.Column);
        }

        public BoardSquare GetSquare(int row, int column)
        {
            return Squares[row - 1][column - 1];
        }

        public BoardSquare GetSquareOrNull(int row, int column)
        {
            if (row < 1 || row > GameRules.MaxRows) return null;
            if (column < 1 || column > GameRules.MaxColumns) return null;

            return Squares[row - 1][column - 1];
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            for (int rowIndex = 1; rowIndex <= GameRules.MaxRows; rowIndex++)
            {
                builder.Append(string.Join(", ", Squares[rowIndex - 1]));
            }

            return builder.ToString();
        }

        private static List<BoardSquare> CreateRow(GameRules gameRules, int rowIndex)
        {
            var row = new List<BoardSquare>();
            for (int colIndex = 1; colIndex <= GameRules.MaxColumns; colIndex++)
            {
                var squareType = gameRules.SquareTypes.GetSquare(rowIndex, colIndex);
                row.Add(new BoardSquare("", 0, squareType));
            }

            return row;
        }
    }
}
