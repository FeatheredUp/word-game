using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Words.API.DataModels;
using Words.API.Exceptions;
using Words.API.Logic;
using Words.API.Repository;
using Xunit;

namespace Words.Test
{
    public class LogicTests
    {
        #region PlayChecker
        [Fact]
        public void TestPlayCheckerWhenNoPlacements_ThrowsException()
        {
            var players = CreatePlayers();
            var rules = new GameRules(Ruleset.SmallUpwords);
            var board = new Board(rules);
            var state = CreateGameState(players, rules, board);
            var player = GetCurrentPlayer(players, state);

            player.AddRack("ABCDEFG".Select(c => c + "").ToList());

            var placements = new List<TilePlacement>();

            var ex = Assert.Throws<ValidationException>(() => new PlayChecker(player, state, placements, Repository));
            Assert.Equal("You must place at least one letter.", ex.Message);
        }

        [Fact]
        public void TestPlayCheckerWhenOnePlacementOnFirstTurn_ThrowsException()
        {
            var players = CreatePlayers();
            var rules = new GameRules(Ruleset.SmallUpwords);
            var board = new Board(rules);
            var state = CreateGameState(players, rules, board);
            var player = GetCurrentPlayer(players, state);

            player.AddRack("ABCDEFG".Select(c => c + "").ToList());

            var placements = new List<TilePlacement> { new TilePlacement("A", 5, 5) };

            var ex = Assert.Throws<ValidationException>(() => new PlayChecker(player, state, placements, Repository));
            Assert.Equal("You must place at least two letters on the first turn.", ex.Message);
        }

        [Fact]
        public void TestPlayCheckerWhenNotOnRack_ThrowsException()
        {
            var players = CreatePlayers();
            var rules = new GameRules(Ruleset.SmallUpwords);
            var board = new Board(rules);
            var state = CreateGameState(players, rules, board);
            var player = GetCurrentPlayer(players, state);

            player.AddRack("ABCDEFG".Select(c => c + "").ToList());

            var placements = new List<TilePlacement>() { new TilePlacement("H", 4, 5) };

            var ex = Assert.Throws<ValidationException>(() => new PlayChecker(player, state, placements, Repository));
            Assert.Equal("You cannot play letters not on your rack.", ex.Message);
        }

        [Fact]
        public void TestPlayCheckerWhenNotOnRackTwice_ThrowsException()
        {
            var players = CreatePlayers();
            var rules = new GameRules(Ruleset.SmallUpwords);
            var board = new Board(rules);
            var state = CreateGameState(players, rules, board);
            var player = GetCurrentPlayer(players, state);

            player.AddRack("ABCDEFG".Select(c => c + "").ToList());

            var placements = new List<TilePlacement>() {
                new TilePlacement("D", 4, 5),
                new TilePlacement("D", 4, 6)
            };

            var ex = Assert.Throws<ValidationException>(() => new PlayChecker(player, state, placements, Repository));
            Assert.Equal("You cannot play letters not on your rack.", ex.Message);
        }

        [Fact]
        public void TestPlayCheckerWhenPositionInvalid_ThrowsException()
        {
            var players = CreatePlayers();
            var rules = new GameRules(Ruleset.SmallUpwords);
            var board = new Board(rules);
            var state = CreateGameState(players, rules, board);
            var player = GetCurrentPlayer(players, state);

            player.AddRack("ABCDEFG".Select(c => c + "").ToList());

            var placements = new List<TilePlacement>() { new TilePlacement(player.Rack.Letters[0], 0, 5) };

            var ex = Assert.Throws<ValidationException>(() => new PlayChecker(player, state, placements, Repository));
            Assert.Equal("You must play letters on valid squares.", ex.Message);
        }

        [Fact]
        public void TestPlayCheckerWhenPositionUnplayable_ThrowsException()
        {
            var players = CreatePlayers();
            var rules = new GameRules(Ruleset.SmallUpwords);
            var board = new Board(rules);
            var state = CreateGameState(players, rules, board);
            var player = GetCurrentPlayer(players, state);

            player.AddRack("ABCDEFG".Select(c => c + "").ToList());

            var placements = new List<TilePlacement>() {
                new TilePlacement(player.Rack.Letters[0], 1, 5),
                new TilePlacement(player.Rack.Letters[1], 2, 5),
                new TilePlacement(player.Rack.Letters[2], 3, 5),
                new TilePlacement(player.Rack.Letters[3], 4, 5),
                new TilePlacement(player.Rack.Letters[4], 5, 5),
            };

            var ex = Assert.Throws<ValidationException>(() => new PlayChecker(player, state, placements, Repository));
            Assert.Equal("You must play letters on valid squares.", ex.Message);
        }

        [Fact]
        public void TestPlayCheckerWhenNotInLineRowAndCol_ThrowsException()
        {
            var players = CreatePlayers();
            var rules = new GameRules(Ruleset.SmallUpwords);
            var board = new Board(rules);
            var state = CreateGameState(players, rules, board);
            var player = GetCurrentPlayer(players, state);

            player.AddRack("ABCDEFG".Select(c => c + "").ToList());

            var placements = new List<TilePlacement>() {
                new TilePlacement(player.Rack.Letters[0], 4, 5),
                new TilePlacement(player.Rack.Letters[1], 5, 4)
            };

            var ex = Assert.Throws<ValidationException>(() => new PlayChecker(player, state, placements, Repository));
            Assert.Equal("You must place all letters on the same row or column.", ex.Message);
        }

        [Fact]
        public void TestPlayCheckerWhenNotInLineRowWrong_ThrowsException()
        {
            var players = CreatePlayers();
            var rules = new GameRules(Ruleset.SmallUpwords);
            var board = new Board(rules);
            var state = CreateGameState(players, rules, board);
            var player = GetCurrentPlayer(players, state);

            player.AddRack("ABCDEFG".Select(c => c + "").ToList());

            var placements = new List<TilePlacement>() {
                new TilePlacement(player.Rack.Letters[0], 5, 5),
                new TilePlacement(player.Rack.Letters[1], 5, 6),
                new TilePlacement(player.Rack.Letters[2], 6, 7)
            };

            var ex = Assert.Throws<ValidationException>(() => new PlayChecker(player, state, placements, Repository));
            Assert.Equal("You must place all letters on the same row or column.", ex.Message);
        }

        [Fact]
        public void TestPlayCheckerWhenNotInLineColumnWrong_ThrowsException()
        {
            var players = CreatePlayers();
            var rules = new GameRules(Ruleset.SmallUpwords);
            var board = new Board(rules);
            var state = CreateGameState(players, rules, board);
            var player = GetCurrentPlayer(players, state);

            player.AddRack("ABCDEFG".Select(c => c + "").ToList());

            var placements = new List<TilePlacement>() {
                new TilePlacement(player.Rack.Letters[0], 5, 5),
                new TilePlacement(player.Rack.Letters[1], 6, 5),
                new TilePlacement(player.Rack.Letters[2], 7, 6)
            };

            var ex = Assert.Throws<ValidationException>(() => new PlayChecker(player, state, placements, Repository));
            Assert.Equal("You must place all letters on the same row or column.", ex.Message);
        }

        [Fact]
        public void TestPlayCheckerWhenTwoLettersOnSameSquare_ThrowsException()
        {
            var players = CreatePlayers();
            var rules = new GameRules(Ruleset.SmallUpwords);
            var board = new Board(rules);
            var state = CreateGameState(players, rules, board);
            var player = GetCurrentPlayer(players, state);

            player.AddRack("ABCDEFG".Select(c => c + "").ToList());

            var placements = new List<TilePlacement>() {
                new TilePlacement(player.Rack.Letters[0], 5, 5),
                new TilePlacement(player.Rack.Letters[1], 6, 5),
                new TilePlacement(player.Rack.Letters[2], 7, 5),
                new TilePlacement(player.Rack.Letters[3], 6, 5)
            };

            var ex = Assert.Throws<ValidationException>(() => new PlayChecker(player, state, placements, Repository));
            Assert.Equal("You must not place 2 letters on the same square.", ex.Message);
        }

        [Fact]
        public void TestPlayCheckerWhenSameLetterOnSquare_ThrowsException()
        {
            var players = CreatePlayers();
            var rules = new GameRules(Ruleset.SmallUpwords);
            var board = new Board(rules);
            board.GetSquare(4, 5).AddTile("A");

            var state = CreateGameState(players, rules, board);
            var player = GetCurrentPlayer(players, state);

            player.AddRack("ABCDEFG".Select(c => c + "").ToList());

            var placements = new List<TilePlacement>() {
                new TilePlacement(player.Rack.Letters[0], 4, 5),
                new TilePlacement(player.Rack.Letters[1], 5, 5)
            };

            var ex = Assert.Throws<ValidationException>(() => new PlayChecker(player, state, placements, Repository));
            Assert.Equal("You must not place a letter on top of the same letter.", ex.Message);
        }

        [Fact]
        public void TestPlayCheckerWhenSquareAlreadyOnHeightFive_ThrowsException()
        {
            var players = CreatePlayers();
            var rules = new GameRules(Ruleset.SmallUpwords);
            var board = new Board(rules);
            board.GetSquare(4, 5).AddTile("A");
            board.GetSquare(4, 5).AddTile("B");
            board.GetSquare(4, 5).AddTile("C");
            board.GetSquare(4, 5).AddTile("D");
            board.GetSquare(4, 5).AddTile("E");

            var state = CreateGameState(players, rules, board);
            var player = GetCurrentPlayer(players, state);

            player.AddRack("ABCDEFG".Select(c => c + "").ToList());

            var placements = new List<TilePlacement>() {
                new TilePlacement(player.Rack.Letters[0], 4, 5),
                new TilePlacement(player.Rack.Letters[1], 4, 6)
            };

            var ex = Assert.Throws<ValidationException>(() => new PlayChecker(player, state, placements, Repository));
            Assert.Equal("You must not place a letter on a square that is already at maximum height.", ex.Message);
        }

        [Fact]
        public void TestPlayCheckerWhenWordCovered_ThrowsException()
        {
            var players = CreatePlayers();
            var rules = new GameRules(Ruleset.SmallUpwords);
            var board = new Board(rules);
            board.GetSquare(4, 5).AddTile("A");
            board.GetSquare(4, 6).AddTile("B");
            board.GetSquare(4, 7).AddTile("C");

            var state = CreateGameState(players, rules, board);
            var player = GetCurrentPlayer(players, state);

            player.AddRack("DEFGHIJ".Select(c => c + "").ToList());

            var placements = new List<TilePlacement>() {
                new TilePlacement(player.Rack.Letters[0], 4, 5),
                new TilePlacement(player.Rack.Letters[1], 4, 6),
                new TilePlacement(player.Rack.Letters[2], 4, 7),
            };

            var ex = Assert.Throws<ValidationException>(() => new PlayChecker(player, state, placements, Repository));
            Assert.Equal("You must not entirely cover a word.", ex.Message);
        }

        [Fact]
        public void TestPlayCheckerWhenWordCoveredAtLeftEdge_ThrowsException()
        {
            var players = CreatePlayers();
            var rules = new GameRules(Ruleset.StandardUpwords);
            var board = new Board(rules);
            board.GetSquare(4, 1).AddTile("A");
            board.GetSquare(4, 2).AddTile("B");
            board.GetSquare(4, 3).AddTile("C");

            var state = CreateGameState(players, rules, board);
            var player = GetCurrentPlayer(players, state);

            player.AddRack("DEFGHIJ".Select(c => c + "").ToList());

            var placements = new List<TilePlacement>() {
                new TilePlacement(player.Rack.Letters[0], 4, 1),
                new TilePlacement(player.Rack.Letters[1], 4, 2),
                new TilePlacement(player.Rack.Letters[2], 4, 3),
            };

            var ex = Assert.Throws<ValidationException>(() => new PlayChecker(player, state, placements, Repository));
            Assert.Equal("You must not entirely cover a word.", ex.Message);
        }

        [Fact]
        public void TestPlayCheckerWhenWordCoveredAtRightEdge_ThrowsException()
        {
            var players = CreatePlayers();
            var rules = new GameRules(Ruleset.StandardUpwords);
            var board = new Board(rules);
            board.GetSquare(4, 8).AddTile("A");
            board.GetSquare(4, 9).AddTile("B");
            board.GetSquare(4, 10).AddTile("C");

            var state = CreateGameState(players, rules, board);
            var player = GetCurrentPlayer(players, state);

            player.AddRack("DEFGHIJ".Select(c => c + "").ToList());

            var placements = new List<TilePlacement>() {
                new TilePlacement(player.Rack.Letters[0], 4, 8),
                new TilePlacement(player.Rack.Letters[1], 4, 9),
                new TilePlacement(player.Rack.Letters[2], 4, 10),
            };

            var ex = Assert.Throws<ValidationException>(() => new PlayChecker(player, state, placements, Repository));
            Assert.Equal("You must not entirely cover a word.", ex.Message);
        }

        [Fact]
        public void TestPlayCheckerWhenTwoWordsChanged_ThrowsException()
        {
            var players = CreatePlayers();
            var rules = new GameRules(Ruleset.SmallUpwords);
            var board = new Board(rules);
            board.GetSquare(4, 2).AddTile("A");
            board.GetSquare(4, 3).AddTile("B");
            board.GetSquare(4, 4).AddTile("C");
            board.GetSquare(4, 7).AddTile("A");
            board.GetSquare(4, 8).AddTile("B");
            board.GetSquare(4, 9).AddTile("C");

            var state = CreateGameState(players, rules, board);
            var player = GetCurrentPlayer(players, state);

            player.AddRack("DEFGHIJ".Select(c => c + "").ToList());

            var placements = new List<TilePlacement>() {
                new TilePlacement(player.Rack.Letters[0], 4, 2),
                new TilePlacement(player.Rack.Letters[1], 4, 7),
            };

            var ex = Assert.Throws<ValidationException>(() => new PlayChecker(player, state, placements, Repository));
            Assert.Equal("You must only change one word in each row or column.", ex.Message);
        }

        [Fact]
        public void TestPlayCheckerWhenJustPlural_ThrowsException()
        {
            var players = CreatePlayers();
            var rules = new GameRules(Ruleset.SmallUpwords);
            var board = new Board(rules);
            board.GetSquare(4, 2).AddTile("A");
            board.GetSquare(4, 3).AddTile("B");
            board.GetSquare(4, 4).AddTile("C");

            var state = CreateGameState(players, rules, board);
            var player = GetCurrentPlayer(players, state);

            state.Turn.NextTurn();

            player.AddRack("SEFGHIJ".Select(c => c + "").ToList());

            var placements = new List<TilePlacement>() {
                new TilePlacement(player.Rack.Letters[0], 4, 5),
            };

            var ex = Assert.Throws<ValidationException>(() => new PlayChecker(player, state, placements, Repository));
            Assert.Equal("You must not just place an S on the end of a word.", ex.Message);
        }

        [Fact]
        public void TestPlayCheckerWhenDoublePlural_IsAccepted()
        {
            var players = CreatePlayers();
            var rules = new GameRules(Ruleset.SmallUpwords);
            var board = new Board(rules);
            board.GetSquare(4, 2).AddTile("C");
            board.GetSquare(4, 3).AddTile("A");
            board.GetSquare(4, 4).AddTile("T");
            board.GetSquare(1, 5).AddTile("D");
            board.GetSquare(2, 5).AddTile("O");
            board.GetSquare(3, 5).AddTile("G");

            var state = CreateGameState(players, rules, board);
            var player = GetCurrentPlayer(players, state);

            state.Turn.NextTurn();

            player.AddRack("SEFGHIJ".Select(c => c + "").ToList());

            var placements = new List<TilePlacement>() {
                new TilePlacement(player.Rack.Letters[0], 4, 5),
            };

            var actual = new PlayChecker(player, state, placements, Repository);

            Assert.NotNull(actual);
            Assert.Equal(16 ,actual.Score);
            Assert.NotNull(actual.Words);
            Assert.Equal(2, actual.Words.Count);
            Assert.Contains("CATS", actual.Words);
            Assert.Contains("DOGS", actual.Words);
        }

        [Fact]
        public void TestPlayCheckerWhenTwoLetterPlural_IsAccepted()
        {
            var players = CreatePlayers();
            var rules = new GameRules(Ruleset.SmallUpwords);
            var board = new Board(rules);
            board.GetSquare(1, 2).AddTile("C");
            board.GetSquare(1, 3).AddTile("A");
            board.GetSquare(1, 4).AddTile("T");

            var state = CreateGameState(players, rules, board);
            var player = GetCurrentPlayer(players, state);

            state.Turn.NextTurn();

            player.AddRack("SEFGHIJ".Select(c => c + "").ToList());

            var placements = new List<TilePlacement>() {
                new TilePlacement(player.Rack.Letters[0], 2, 3),
            };

            var actual = new PlayChecker(player, state, placements, Repository);

            Assert.NotNull(actual);
            Assert.Equal(4, actual.Score);
            Assert.NotNull(actual.Words);
            Assert.Equal(1, actual.Words.Count);
            Assert.Contains("AS", actual.Words);
        }

        [Fact]
        public void TestPlayCheckerWhenPluralInTheMiddle_IsAccepted()
        {
            var players = CreatePlayers();
            var rules = new GameRules(Ruleset.SmallUpwords);
            var board = new Board(rules);
            board.GetSquare(4, 2).AddTile("C");
            board.GetSquare(4, 3).AddTile("A");
            board.GetSquare(4, 4).AddTile("R");
            board.GetSquare(4, 5).AddTile("T");

            var state = CreateGameState(players, rules, board);
            var player = GetCurrentPlayer(players, state);

            state.Turn.NextTurn();

            player.AddRack("SEFGHIJ".Select(c => c + "").ToList());

            var placements = new List<TilePlacement>() {
                new TilePlacement(player.Rack.Letters[0], 4, 4),
            };

            var actual = new PlayChecker(player, state, placements, Repository);

            Assert.NotNull(actual);
            Assert.Equal(5, actual.Score);
            Assert.NotNull(actual.Words);
            Assert.Equal(1, actual.Words.Count);
            Assert.Contains("CAST", actual.Words);
        }

        [Fact]
        public void TestPlayCheckerWhenFirstTurnNotThroughMiddle_ThrowsException()
        {
            var players = CreatePlayers();
            var rules = new GameRules(Ruleset.SmallUpwords);
            var board = new Board(rules);

            var state = CreateGameState(players, rules, board);
            var player = GetCurrentPlayer(players, state);

            player.AddRack("ABCDEF".Select(c => c + "").ToList());

            var placements = new List<TilePlacement>() {
                new TilePlacement(player.Rack.Letters[0], 3, 2),
                new TilePlacement(player.Rack.Letters[1], 3, 3),
            };

            var ex = Assert.Throws<ValidationException>(() => new PlayChecker(player, state, placements, Repository));
            Assert.Equal("You must play the first word through one of the starting squares.", ex.Message);
        }

        [Fact]
        public void TestPlayCheckerWhenNotConnected_ThrowsException()
        {
            var players = CreatePlayers();
            var rules = new GameRules(Ruleset.SmallUpwords);
            var board = new Board(rules);
            board.GetSquare(4, 2).AddTile("A");
            board.GetSquare(4, 3).AddTile("B");
            board.GetSquare(4, 4).AddTile("C");

            var state = CreateGameState(players, rules, board);
            var player = GetCurrentPlayer(players, state);

            state.Turn.NextTurn();

            player.AddRack("DEFGHI".Select(c => c + "").ToList());

            var placements = new List<TilePlacement>() {
                new TilePlacement(player.Rack.Letters[0], 5, 5),
                new TilePlacement(player.Rack.Letters[1], 6, 5),
                new TilePlacement(player.Rack.Letters[2], 7, 5),
            };

            var ex = Assert.Throws<ValidationException>(() => new PlayChecker(player, state, placements, Repository));
            Assert.Equal("You must play letters that connect to a word already on the board.", ex.Message);
        }
        #endregion

        #region PlayInformation
        [Fact]
        public void TestPlayInformationWithFirstSingleWord_ReturnsWordAndScore()
        {
            var rules = new GameRules(Ruleset.SmallUpwords);
            var board = new Board(rules);
            var placements = new List<TilePlacement>() {
                new TilePlacement("C", 5, 5),
                new TilePlacement("A", 6, 5),
                new TilePlacement("T", 7, 5),
            };

            var info = new PlayInformation(board, placements);

            Assert.NotNull(info);
            Assert.Equal(1, info.Words.Count);
            Assert.Equal("CAT", info.Words[0]);
            Assert.Equal(6, info.Score);
        }

        [Fact]
        public void TestPlayInformationWithFirstSingleWordWithQ_ReturnsWordAndScore()
        {
            var rules = new GameRules(Ruleset.SmallUpwords);
            var board = new Board(rules);

            var placements = new List<TilePlacement>() {
                new TilePlacement("Qu", 5, 5),
                new TilePlacement("I", 5, 6),
                new TilePlacement("T", 5, 7),
            };

            var info = new PlayInformation(board, placements);

            Assert.NotNull(info);
            Assert.Equal(1, info.Words.Count);
            Assert.Equal("QUIT", info.Words[0]);
            Assert.Equal(8, info.Score);
        }

        [Fact]
        public void TestPlayInformationWithSecondSingleWordOneIntersection_ReturnsWordsAndScore()
        {
            var rules = new GameRules(Ruleset.SmallUpwords);
            var board = new Board(rules);
            board.GetSquare(5, 5).AddTile("C");
            board.GetSquare(6, 5).AddTile("A");
            board.GetSquare(7, 5).AddTile("T");

            var placements = new List<TilePlacement>() {
                new TilePlacement("D", 7, 5),
                new TilePlacement("O", 7, 6),
                new TilePlacement("G", 7, 7),
            };

            var info = new PlayInformation(board, placements);

            Assert.NotNull(info);
            Assert.Equal(2, info.Words.Count);
            Assert.Equal("DOG", info.Words[0]);
            Assert.Equal("CAD", info.Words[1]);
            Assert.Equal(8, info.Score);
        }

        [Fact]
        public void TestPlayInformationWithSecondSingleWordWithQOneIntersection_ReturnsWordsAndScore()
        {
            var rules = new GameRules(Ruleset.SmallUpwords);
            var board = new Board(rules);
            board.GetSquare(5, 5).AddTile("C");
            board.GetSquare(6, 5).AddTile("A");
            board.GetSquare(7, 5).AddTile("D");

            var placements = new List<TilePlacement>() {
                new TilePlacement("Qu", 5, 5),
                new TilePlacement("I", 5, 6),
                new TilePlacement("N", 5, 7),
            };

            var info = new PlayInformation(board, placements);

            Assert.NotNull(info);
            Assert.Equal(2, info.Words.Count);
            Assert.Equal("QUIN", info.Words[0]);
            Assert.Equal("QUAD", info.Words[1]);
            Assert.Equal(8, info.Score);
        }

        [Fact]
        public void TestPlayInformationWithSecondSingleWordNoIntersection_ReturnsWordsAndScore()
        {
            var rules = new GameRules(Ruleset.SmallUpwords);
            var board = new Board(rules);
            board.GetSquare(5, 5).AddTile("C");
            board.GetSquare(6, 5).AddTile("A");
            board.GetSquare(7, 5).AddTile("T");

            var placements = new List<TilePlacement>() {
                new TilePlacement("S", 8, 5),
                new TilePlacement("I", 8, 6),
                new TilePlacement("T", 8, 7),
            };

            var info = new PlayInformation(board, placements);

            Assert.NotNull(info);
            Assert.Equal(2, info.Words.Count);
            Assert.Equal("SIT", info.Words[0]);
            Assert.Equal("CATS", info.Words[1]);
            Assert.Equal(14, info.Score);
        }

        [Fact]
        public void TestPlayInformationWithSecondSingleWordWithQNoIntersection_ReturnsWordsAndScore()
        {
            var rules = new GameRules(Ruleset.SmallUpwords);
            var board = new Board(rules);
            board.GetSquare(5, 5).AddTile("C");
            board.GetSquare(6, 5).AddTile("A");
            board.GetSquare(7, 5).AddTile("T");

            var placements = new List<TilePlacement>() {
                new TilePlacement("S", 8, 5),
                new TilePlacement("Qu", 8, 6),
                new TilePlacement("A", 8, 7),
                new TilePlacement("T", 8, 8),
            };

            var info = new PlayInformation(board, placements);

            Assert.NotNull(info);
            Assert.Equal(2, info.Words.Count);
            Assert.Equal("SQUAT", info.Words[0]);
            Assert.Equal("CATS", info.Words[1]);
            Assert.Equal(18, info.Score);
        }

        [Fact]
        public void TestPlayInformationWhenAddLetterToTopOfVerticalWord_ReturnsWordAndScore()
        {
            var rules = new GameRules(Ruleset.SmallUpwords);
            var board = new Board(rules);
            board.GetSquare(4, 6).AddTile("H");
            board.GetSquare(5, 6).AddTile("A");
            board.GetSquare(6, 6).AddTile("T");

            var placements = new List<TilePlacement>() {
                new TilePlacement("T", 3, 6),
            };

            var info = new PlayInformation(board, placements);

            Assert.NotNull(info);
            Assert.Equal(1, info.Words.Count);
            Assert.Equal("THAT", info.Words[0]);
            Assert.Equal(8, info.Score);
        }

        [Fact]
        public void TestPlayInformationWhenChangeLetterInVerticalWord_ReturnsWordAndScore()
        {
            var rules = new GameRules(Ruleset.SmallUpwords);
            var board = new Board(rules);
            board.GetSquare(4, 6).AddTile("H");
            board.GetSquare(5, 6).AddTile("A");
            board.GetSquare(6, 6).AddTile("T");

            var placements = new List<TilePlacement>() {
                new TilePlacement("I", 5, 6),
            };

            var info = new PlayInformation(board, placements);

            Assert.NotNull(info);
            Assert.Equal(1, info.Words.Count);
            Assert.Equal("HIT", info.Words[0]);
            Assert.Equal(4, info.Score);
        }

        [Fact]
        public void TestPlayInformationWhenAddLetterToFrontOfHorizontalWord_ReturnsWordAndScore()
        {
            var rules = new GameRules(Ruleset.SmallUpwords);
            var board = new Board(rules);
            board.GetSquare(6, 4).AddTile("H");
            board.GetSquare(6, 5).AddTile("A");
            board.GetSquare(6, 6).AddTile("T");

            var placements = new List<TilePlacement>() {
                new TilePlacement("T", 6, 3),
            };

            var info = new PlayInformation(board, placements);

            Assert.NotNull(info);
            Assert.Equal(1, info.Words.Count);
            Assert.Equal("THAT", info.Words[0]);
            Assert.Equal(8, info.Score);
        }

        [Fact]
        public void TestPlayInformationWhenChangeLetterInHorizontalWord_ReturnsWordAndScore()
        {
            var rules = new GameRules(Ruleset.SmallUpwords);
            var board = new Board(rules);
            board.GetSquare(6, 4).AddTile("H");
            board.GetSquare(6, 5).AddTile("A");
            board.GetSquare(6, 6).AddTile("T");

            var placements = new List<TilePlacement>() {
                new TilePlacement("I", 6, 5),
            };

            var info = new PlayInformation(board, placements);

            Assert.NotNull(info);
            Assert.Equal(1, info.Words.Count);
            Assert.Equal("HIT", info.Words[0]);
            Assert.Equal(4, info.Score);
        }
        #endregion

        #region private

        private IRepository Repository
        {
            get
            {
                var mockRepository = new Mock<IRepository>();
                mockRepository.Setup(repo => repo.WordsNotInDictionary(It.IsAny<IReadOnlyList<string>>())).Returns(new List<string>());
                return mockRepository.Object;
            }
        }

        private static GameState CreateGameState(List<Player> players, GameRules rules, Board board)
        {
            var gameId = new GameId("ABCD");
            var bag = new TileBag(rules.Letters);
            var turn = new TurnOrder(players, 9876);

            return new GameState(gameId, players, bag, turn, board, rules, GameStatus.InProgress);
        }

        private static Player GetCurrentPlayer(List<Player> players, GameState state)
        {
            var playerId = state.Turn.CurrentPlayerId;
            var player = players.First(p => p.PlayerId.Value == playerId.Value);
            return player;
        }

        private static List<Player> CreatePlayers()
        {
            var anna = new Player(new PlayerName("Anna"), true);
            var bert = new Player(new PlayerName("Bert"), false);
            var carol = new Player(new PlayerName("Carol"), false);
            var dave = new Player(new PlayerName("Dave"), false);
            var players = new List<Player> { anna, bert, carol, dave };

            return players;
        }
        #endregion
    }
}
