using System;
using System.Collections.Generic;
using System.Linq;
using Words.API.DataModels;

namespace Words.API.Logic
{
    public class PlayInformation
    {
        public Direction Direction { get; }

        public int Row { get; }
        public int Column { get; }
        
        public IReadOnlyList<string> OriginalLine => _previousSquaresInLine.Select(l => l.Letter).ToList();
        public IReadOnlyList<string> PlacedLine { get; }

        public IReadOnlyList<string> Words { get; }
        public int Score { get; }

        private readonly IReadOnlyList<BoardSquare> _previousSquaresInLine;
        private readonly List<IReadOnlyList<BoardSquare>> _wordTiles;

        public PlayInformation(Board board, List<TilePlacement> placements)
        {
            if (board == null) throw new ArgumentNullException(nameof(board));
            if (placements == null) throw new ArgumentNullException(nameof(placements));

            var rows = placements.Select(p => p.Row).Distinct().ToList();
            var columns = placements.Select(p => p.Column).Distinct().ToList();

            _wordTiles = new List<IReadOnlyList<BoardSquare>>();

            Direction = GetDirection(placements, board);

            if (Direction == Direction.Horizontal)
            {
                Row = rows.First();
                _previousSquaresInLine = GetRow(board, Row).ToList();
                PlacedLine = GetPlacedRow(placements).ToList();

                _wordTiles = GetWordsForHorizontalPlay(board, placements);
            }
            else if (Direction == Direction.Vertical)
            {
                Column = columns.First();
                _previousSquaresInLine = GetColumn(board, columns.First()).ToList();
                PlacedLine = GetPlacedColumn(placements).ToList();

                _wordTiles = GetWordsForVerticalPlay(board, placements);
            }

            Words = GetWords(_wordTiles);
            Score = CalculateScore(_wordTiles, placements.Count);
        }

        private static Direction GetDirection(List<TilePlacement> placements, Board board)
        {
            var rows = placements.Select(p => p.Row).Distinct().ToList();
            var columns = placements.Select(p => p.Column).Distinct().ToList();

            if (rows.Count == 1 && columns.Count == 1)
            {
                // One single letter was placed, so look to left and right to see if it's an amendment to a horizontal word.
                // If not assume it's vertical, as this letter must be attached to the main grid.
                var row = rows.First();
                var column = columns.First();
                var left = board.GetSquareOrNull(row, column - 1);
                var right = board.GetSquareOrNull(row, column + 1);
                if (!string.IsNullOrEmpty(left?.Letter) || !string.IsNullOrEmpty(right?.Letter)) return Direction.Horizontal;
                return Direction.Vertical;
            }

            if (rows.Count == 1) return Direction.Horizontal;
            if (columns.Count == 1) return Direction.Vertical;

            return Direction.Unknown;
        }

        private static IReadOnlyList<string> GetWords(List<IReadOnlyList<BoardSquare>> wordTiles)
        {
            var words = new List<string>();
            foreach (var word in wordTiles)
            {
                var toAdd = string.Join("", word.Select(w => w.Letter.ToUpper()));
                words.Add(toAdd);
            }

            return words;
        }

        private static int CalculateScore(List<IReadOnlyList<BoardSquare>> wordTiles, int lettersPlayed)
        {
            var score = 0;
            foreach (var word in wordTiles)
            {
                if (word.All(l=> l.Height == 1))
                {
                    score += word.Count * 2;
                    if (word.Any(l => l.Letter == "Qu")) score += 2;
                }
                else
                { 
                    foreach (var letter in word)
                    {
                        score += letter.Height;
                    }
                }
            }

            if (lettersPlayed == 7) score += 20;

            return score;
        }

        private List<IReadOnlyList<BoardSquare>> GetWordsForHorizontalPlay(Board board, List<TilePlacement> placements)
        {
            var words = new List<IReadOnlyList<BoardSquare>>();

            var mainWord = GetUpdatedWord(_previousSquaresInLine, PlacedLine);
            words.Add(mainWord);

            // For each colum, if a letter is changed, work out the updated word
            for (int col = 1; col <= GameRules.MaxColumns; col++)
            {
                // Has player touched this column?
                if (placements.Any(p => p.Column == col))
                {
                    var original = GetColumn(board, col).ToList();
                    var placed = GetPlacedColumn(placements.Where(p => p.Column == col).ToList());

                    var word = GetUpdatedWord(original, placed);
                    if (word.Count > 1)
                    {
                        words.Add(word);
                    }
                }
            }

            return words;
        }

        private List<IReadOnlyList<BoardSquare>> GetWordsForVerticalPlay(Board board, List<TilePlacement> placements)
        {
            var words = new List<IReadOnlyList<BoardSquare>>();

            var mainWord = GetUpdatedWord(_previousSquaresInLine, PlacedLine);
            words.Add(mainWord);

            // For each row, if a letter is changed, work out the updated word
            for (int row = 1; row <= GameRules.MaxRows; row++)
            {
                // Has player touched this row?
                if (placements.Any(p => p.Row == row))
                {
                    var original = GetRow(board, row).ToList();
                    var placed = GetPlacedRow(placements.Where(p => p.Row == row).ToList());

                    var word = GetUpdatedWord(original, placed);
                    if (word.Count > 1)
                    {
                        words.Add(word);
                    }
                }
            }

            return words;
        }

        private static IReadOnlyList<BoardSquare> GetUpdatedWord(IReadOnlyList<BoardSquare> originalLine, IReadOnlyList<string> placedLine)
        {
            // index of first non-blank entry
            var firstChangedLetter = -1;
            for (int i = 0; i < placedLine.Count; i++)
            {
                if (!string.IsNullOrEmpty(placedLine[i]))
                {
                    firstChangedLetter = i;
                    break;
                }
            }

            // Look backwards to find the first letter of the word
            var startOfWord = 0;
            for (int i = firstChangedLetter - 1; i >= 0; i--)
            {
                if (string.IsNullOrEmpty(originalLine[i].Letter))
                {
                    startOfWord = i + 1;
                    break;
                }
            }

            var word = new List<BoardSquare>();
            // Go from start of word and get each letter until blank or no more tiles
            for (int i = startOfWord; i < placedLine.Count; i++)
            {
                if (!string.IsNullOrEmpty(placedLine[i]))
                {
                    // New letter, with old height plus one
                    word.Add(new BoardSquare(placedLine[i], originalLine[i].Height + 1, SquareType.Standard));
                }
                else if (!string.IsNullOrEmpty(originalLine[i].Letter))
                {
                    // Unchanged letter
                    word.Add(originalLine[i]);
                }
                else
                {
                    // Blank tile, so end of word
                    break;
                }
            }

            return word;
        }

        private static List<string> GetPlacedColumn(List<TilePlacement> placements)
        {
            var line = GetEmptyList(GameRules.MaxRows);

            foreach (var placement in placements)
            {
                line[placement.Row - 1] = placement.Letter;
            }

            return line;
        }

        private static List<string> GetPlacedRow(List<TilePlacement> placements)
        {
            var line = GetEmptyList(GameRules.MaxColumns);

            foreach (var placement in placements)
            {
                line[placement.Column - 1] = placement.Letter;
            }

            return line;
        }

        private static IEnumerable<BoardSquare> GetColumn(Board board, int column)
        {
            for (int row = 1; row <= GameRules.MaxRows; row++)
            {
                yield return board.GetSquare(row, column);
            }
        }

        private static IEnumerable<BoardSquare> GetRow(Board board, int row)
        {
            for (int column = 1; column <= GameRules.MaxColumns; column++)
            {
                yield return board.GetSquare(row, column);
            }
        }

        private static List<string> GetEmptyList(int length)
        {
            var placed = new List<string>();
            for (int i = 0; i < length; i++)
            {
                placed.Add("");
            }

            return placed;
        }
    }
}