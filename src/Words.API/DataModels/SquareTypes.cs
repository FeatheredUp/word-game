using System;

namespace Words.API.DataModels
{
    public enum SquareType
    {
        NotSet,
        Standard,
        Starting,
        Unplayable
    }

    public class SquareTypes
    {
        private readonly SquareType[][] _squareTypes;

        public SquareTypes()
        {
            _squareTypes = new SquareType[GameRules.MaxRows][];
            for (int row = 1; row <= GameRules.MaxRows; row++)
            {
                _squareTypes[row - 1] = new SquareType[GameRules.MaxColumns];
                for (int col = 1; col <= GameRules.MaxColumns; col++)
                {
                    _squareTypes[row - 1][col - 1] = SquareType.Standard;
                }
            }
        }

        public SquareTypes WithRow(int row, SquareType squareType)
        {
            if (row < 1 || row > GameRules.MaxRows) throw new ArgumentOutOfRangeException(nameof(row));

            for (int i = 1; i <= GameRules.MaxColumns; i++)
            {
                _squareTypes[row - 1][i - 1] = squareType;
            }

            return this;
        }

        public SquareTypes WithColumn(int column, SquareType squareType)
        {
            if (column < 1 || column > GameRules.MaxColumns) throw new ArgumentOutOfRangeException(nameof(column));

            for (int i = 1; i <= GameRules.MaxRows; i++)
            {
                _squareTypes[i - 1][column - 1] = squareType;
            }

            return this;
        }

        public SquareTypes WithSquare(int row, int column, SquareType squareType)
        {
            if (row < 1 || row > GameRules.MaxRows) throw new ArgumentOutOfRangeException(nameof(row));
            if (column < 1 || column > GameRules.MaxColumns) throw new ArgumentOutOfRangeException(nameof(column));

            _squareTypes[row - 1][column - 1] = squareType;
            return this;
        }

        internal SquareTypes WithRowEndsUnplayable(int row, SquareType squareType, int ends)
        {
            if (row < 1 || row > GameRules.MaxRows) throw new ArgumentOutOfRangeException(nameof(row));

            for (int i = 1; i <= GameRules.MaxColumns; i++)
            {
                if ( i <= ends || i > GameRules.MaxColumns - ends) _squareTypes[row - 1][i - 1] = squareType;
            }

            return this;
        }

        public SquareType GetSquare(int row, int column)
        {
            if (row < 1 || row > GameRules.MaxRows) throw new ArgumentOutOfRangeException(nameof(row));
            if (column < 1 || column > GameRules.MaxColumns) throw new ArgumentOutOfRangeException(nameof(column));

            return _squareTypes[row - 1][column - 1];
        }

    }
}
