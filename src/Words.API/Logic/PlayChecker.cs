using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Words.API.DataModels;
using Words.API.Exceptions;
using Words.API.Repository;

namespace Words.API.Logic
{
    public class PlayChecker
    {
        public int Score { get; }
        public IReadOnlyList<string> Words { get; }

        /// <summary>
        /// This checks the play is valid.  If anything is invalid it will throw a ValidationException.
        /// If it's valid, it will return the score for that play.
        /// </summary>
        public PlayChecker(Player player, GameState state, List<TilePlacement> placements, IRepository repository)
        {
            if (placements == null) throw new ArgumentNullException(nameof(placements));
            if (player == null) throw new ArgumentNullException(nameof(player));
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (repository == null) throw new ArgumentNullException(nameof(repository));

            // Basic validations to make sure assumptions in following rules and PlayInformation construtor are valid
            if (placements.Count == 0) throw new ValidationException("You must place at least one letter.");
            if (!AreAllLettersOnRack(player.Rack, placements)) throw new ValidationException("You cannot play letters not on your rack.");
            if (!AreAllPositionsValid(placements, state.Board)) throw new ValidationException("You must play letters on valid squares.");
            if (!AreLettersInUniquePositions(placements)) throw new ValidationException("You must not place 2 letters on the same square.");
            if (!AreLettersOnDifferentLetters(placements, state.Board)) throw new ValidationException("You must not place a letter on top of the same letter.");
            if (!AreAllLettersValidHeight(placements, state.Board, state.GameRules)) throw new ValidationException("You must not place a letter on a square that is already at maximum height.");

            var info = new PlayInformation(state.Board, placements);

            if (!LettersAreInALine(info)) throw new ValidationException("You must place all letters on the same row or column.");
            if (!FirstTurnHasEnoughLetters(placements, state.Board.IsEmpty)) throw new ValidationException("You must place at least two letters on the first turn.");
            if (!FirstTurnThroughValidSquare(placements, state.Board, state.Board.IsEmpty)) throw new ValidationException("You must play the first word through one of the starting squares.");
            if (!AcceptablePluralisation(placements, state.Board)) throw new ValidationException("You must not just place an S on the end of a word.");
            if (!LettersInWordLeftVisible(info)) throw new ValidationException("You must not entirely cover a word.");
            if (!OneWordChanged(info)) throw new ValidationException("You must only change one word in each row or column.");
            if (!WordConnected(placements, state.Board, state.Board.IsEmpty)) throw new ValidationException("You must play letters that connect to a word already on the board.");

            var wordsNotInDictionary = repository.WordsNotInDictionary(info.Words);
            if (wordsNotInDictionary.Count > 0) throw new ValidationException($"Word(s) not in dictionary: {string.Join(", ", wordsNotInDictionary)}");

            Words = info.Words;
            Score = info.Score;
        }

        private static bool FirstTurnHasEnoughLetters(List<TilePlacement> placements, bool boardIsEmpty)
        {
            if (!boardIsEmpty) return true;
            if (placements.Count >= 2) return true;

            return false;
        }

        private static bool WordConnected(List<TilePlacement> placements, Board board, bool boardIsEmpty)
        {
            if (boardIsEmpty) return true;

            foreach (var placement in placements)
            {
                if (board.GetSquareOrNull(placement.Row, placement.Column)?.Height > 0) return true;
                if (board.GetSquareOrNull(placement.Row + 1, placement.Column)?.Height > 0) return true;
                if (board.GetSquareOrNull(placement.Row, placement.Column + 1)?.Height > 0) return true;
                if (board.GetSquareOrNull(placement.Row - 1, placement.Column)?.Height > 0) return true;
                if (board.GetSquareOrNull(placement.Row, placement.Column - 1)?.Height > 0) return true;
            }

            return false;
        }

        private static bool FirstTurnThroughValidSquare(List<TilePlacement> placements, Board board, bool boardIsEmpty)
        {
            if (!boardIsEmpty) return true;

            foreach (var placement in placements)
            {
                if (board.GetSquare(placement).SquareType == SquareType.Starting) return true;
            }

            return false;
        }

        private static bool AcceptablePluralisation(List<TilePlacement> placements, Board board)
        {
            if (placements.Count > 1) return true;

            var placement = placements.First();
            if (placement.Letter != "S") return true;
            if (board.GetSquare(placement).Height > 0) return true;

            // So there is just one 'S' being played on an empty square

            // If there's a square to the right OR below this one, it's fine.
            var right = board.GetSquareOrNull(placement.Row + 1, placement.Column)?.Height > 0;
            var below = board.GetSquareOrNull(placement.Row, placement.Column + 1)?.Height > 0;
            if (right || below) return true;

            // If there's a square above AND to the left, it's fine.
            var left = board.GetSquareOrNull(placement.Row - 1, placement.Column)?.Height > 0;
            var above = board.GetSquareOrNull(placement.Row, placement.Column - 1)?.Height > 0;
            if (left && above) return true;

            // If adding an S to a single letter to make IS for example, it's fine
            var twoLeft = board.GetSquareOrNull(placement.Row - 2, placement.Column)?.Height > 0;
            if (left && !twoLeft) return true;

            var twoAbove = board.GetSquareOrNull(placement.Row, placement.Column - 2)?.Height > 0;
            if (above && !twoAbove) return true;

            // Must be a single S on empty square, with only one adjacent tile, either above or to the left.
            // I.e. just put an S on the end.
            return false;
        }

        private static bool AreAllLettersValidHeight(List<TilePlacement> placements, Board board, GameRules rules)
        {
            foreach (var placement in placements)
            {
                if (board.GetSquare(placement).Height == rules.StackHeight) return false;
            }

            return true;
        }

        private static bool AreLettersOnDifferentLetters(List<TilePlacement> placements, Board board)
        {
            foreach (var placement in placements)
            {
                if (board.GetSquare(placement).Letter == placement.Letter) return false;
            }

            return true;
        }

        private static bool AreLettersInUniquePositions(List<TilePlacement> placements)
        {
            foreach (var placement in placements)
            {
                var matches = placements.Where(p => p.Row == placement.Row && p.Column == placement.Column);
                if (matches.Count() > 1) return false;
            }

            return true;
        }

        private static bool AreAllPositionsValid(List<TilePlacement> placements, Board board)
        {
            foreach (var placement in placements)
            {
                if (placement.Row <= 0 || placement.Row > GameRules.MaxRows) return false;
                if (placement.Column <= 0 || placement.Column > GameRules.MaxColumns) return false;

                if (board.GetSquare(placement).SquareType == SquareType.Unplayable) return false;
            }

            return true;
        }

        private static bool AreAllLettersOnRack(Rack rack, List<TilePlacement> placements)
        {
            var letters = rack.Letters.ToList();
            foreach (var placement in placements)
            {
                if (!letters.Contains(placement.Letter)) return false;
                letters.Remove(placement.Letter);
            }

            return true;
        }

        private static bool LettersAreInALine(PlayInformation information)
        {
            return information.Row > 0 || information.Column > 0;
        }

        private static bool LettersInWordLeftVisible(PlayInformation information)
        {
            bool inWord = false;
            bool wordCovered = false;
            int wordLength = -1;

            for (int index = 0; index < information.OriginalLine.Count; index++)
            {
                // This was a blank square previously, so any previous word has ended.
                if (string.IsNullOrEmpty(information.OriginalLine[index]))
                {
                    inWord = false;
                    // If there was a previous word that was entirely covered, that's not valid
                    if (wordCovered && wordLength > 1) return false;
                }
                else
                {
                    // First letter in a word
                    if (!inWord)
                    {
                        inWord = true;
                        wordCovered = true;
                        wordLength = 0;
                    }

                    // This tile is not covered
                    if (string.IsNullOrEmpty(information.PlacedLine[index]))
                    {
                        wordCovered = false;
                    }
                    else
                    {
                        wordLength += 1;
                    }
                }
            }

            // If there was a previous word that was entirely covered, that's not valid
            if (wordCovered && wordLength > 1) return false;

            return true;
        }

        internal static bool OneWordChanged(PlayInformation information)
        {
            var mainWordStarted = false;
            var mainWordEnded = false;
            for (int index = 0; index < information.OriginalLine.Count; index++)
            {
                // Main word has ended but there's an extra letter changed
                if (mainWordEnded && !string.IsNullOrEmpty(information.PlacedLine[index]))
                {
                    return false;
                }

                // Main word has started and this tile has neither an original letter nor a placed one, so the word has ended
                if (mainWordStarted && string.IsNullOrEmpty(information.PlacedLine[index]) && string.IsNullOrEmpty(information.OriginalLine[index]))
                {
                    mainWordEnded = true;
                }

                if (!string.IsNullOrEmpty(information.PlacedLine[index]))
                {
                    mainWordStarted = true;
                }
            }

            return true;
        }
    }
}
