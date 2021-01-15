using System;
using System.Collections.Generic;
using System.Linq;
using Words.API.DataModels;
using Words.API.Exceptions;
using Xunit;

namespace Words.Test
{
    public class DataModelTests
    {
        #region PlayerName
        [Fact]
        public void TestPlayerNameWithValidName_CreatesPlayerName()
        {
            var playerName = new PlayerName("George");

            Assert.NotNull(playerName);
            Assert.NotNull(playerName.Value);
            Assert.Equal("George", playerName.Value);
        }

        [Fact]
        public void TestPlayerNameWithBlankName_ThrowsException()
        {
            Assert.Throws<InvalidPlayerNameException>(() => new PlayerName(""));
        }
        #endregion

        #region PlayerId
        [Fact]
        public void TestPlayerIdWithValidValue_CreatesPlayerId()
        {
            var playerId = new PlayerId("Anything");

            Assert.NotNull(playerId);
            Assert.NotNull(playerId.Value);
            Assert.Equal("Anything", playerId.Value);
        }
        #endregion

        #region GameId
        [Fact]
        public void TestGameIdWithValidValue_CreatesGameId()
        {
            var gameId = new GameId("ZYXW");

            Assert.NotNull(gameId);
            Assert.NotNull(gameId.Value);
            Assert.Equal("ZYXW", gameId.Value);
        }

        [Fact]
        public void TestGameIdWithBlankId_ThrowsException()
        {
            Assert.Throws<InvalidGameIdException>(() => new GameId(""));
        }

        [Fact]
        public void TestGameIdWithTooShortId_ThrowsException()
        {
            Assert.Throws<InvalidGameIdException>(() => new GameId("XYZ"));
        }

        [Fact]
        public void TestGameIdWithTooLongId_ThrowsException()
        {
            Assert.Throws<InvalidGameIdException>(() => new GameId("ABCDE"));
        }

        [Fact]
        public void TestGameIdWithNumericId_ThrowsException()
        {
            Assert.Throws<InvalidGameIdException>(() => new GameId("AB1D"));
        }
        #endregion

        #region Player
        [Fact]
        public void TestPlayerWithValidData_CreatesPlayer()
        {
            var player = new Player(new PlayerName("Bob"), false);

            Assert.NotNull(player);

            Assert.NotNull(player.PlayerId);
            Assert.NotNull(player.PlayerId.Value);
            Assert.True(ValidPlayerId(player.PlayerId.Value), $"PlayerId is {player.PlayerId.Value} which is not valid.");

            Assert.NotNull(player.PlayerName);
            Assert.NotNull(player.PlayerName.Value);
            Assert.Equal("Bob", player.PlayerName.Value);
            Assert.False(player.IsCreator);
            Assert.Equal(0, player.Score);
            Assert.Null(player.Rack);
            Assert.False(player.IsCreator);

            var expectedPlayerString = "Bob (" + player.PlayerId + ") - [EMPTY] - 0";
            Assert.Equal(expectedPlayerString, player.ToString());
        }

        [Fact]
        public void TestPlayerFromLoadedValues_CreatesPlayer()
        {
            var player = new Player("Id1", "Bob", false, 22, "A,B,C,D,E,F");

            Assert.NotNull(player);

            Assert.NotNull(player.PlayerName);
            Assert.NotNull(player.PlayerName.Value);
            Assert.Equal("Id1", player.PlayerId.Value);
            Assert.Equal("Bob", player.PlayerName.Value);
            Assert.False(player.IsCreator);
            Assert.Equal(22, player.Score);
            Assert.Equal(6, player.Rack.Letters.Count);

            var expectedPlayerString = "Bob (" + player.PlayerId + ") - [A, B, C, D, E, F] - 22";
            Assert.Equal(expectedPlayerString, player.ToString());
        }
        #endregion

        #region CreatedGame
        [Fact]
        public void TestCreatedGameWithValidName_CreatesGame()
        {
            var name = new PlayerName("George");

            var createdGame = new CreatedGame(name);

            Assert.NotNull(createdGame);
            Assert.NotNull(createdGame.GameId);
            Assert.NotNull(createdGame.GameId.Value);
            Assert.True(ValidGameId(createdGame.GameId.Value), $"GameId is {createdGame.GameId.Value} which is not valid.");

            Assert.NotNull(createdGame.Player.PlayerId);
            Assert.NotNull(createdGame.Player.PlayerId.Value);
            Assert.True(ValidPlayerId(createdGame.Player.PlayerId.Value), $"PlayerId is {createdGame.Player.PlayerId.Value} which is not valid.");

            Assert.NotNull(createdGame.Player.PlayerName);
            Assert.NotNull(createdGame.Player.PlayerName.Value);
            Assert.Equal("George", createdGame.Player.PlayerName.Value);
            Assert.True(createdGame.Player.IsCreator);
            Assert.Equal(0, createdGame.Player.Score);
            Assert.Null(createdGame.Player.Rack);

            Assert.True(createdGame.Player.IsCreator);

            var expectedGameString = createdGame.GameId + " - " + createdGame.Player.PlayerName + " (" + createdGame.Player.PlayerId.Value + ") - [EMPTY] - 0";

            Assert.Equal(expectedGameString, createdGame.ToString()); ;
        }
        #endregion

        #region GameState
        [Fact]
        public void TestGameStateWithValidRules_CreatesGameState()
        {
            var rules = new GameRules(Ruleset.SmallUpwords);
            var players = new List<Player> { new Player(new PlayerName("Anna"), true), new Player(new PlayerName("Bob"), false) };
            var gameState = new GameState(new GameId("ABCD"), rules, players, GameStatus.InProgress);

            var expectedTiles = rules.Letters.Count - rules.RackSize - rules.RackSize;
            Assert.Equal(expectedTiles, gameState.TileBag.Count);

            var returnedPlayers = gameState.Players.ToList();
            Assert.Equal(2, returnedPlayers.Count);

            var anna = returnedPlayers.FirstOrDefault(p => p.PlayerName.Value == "Anna");
            Assert.True(anna.IsCreator);
            Assert.Equal(rules.RackSize, anna.Rack.Letters.Count);
            Assert.Equal(rules.RackSize, anna.Rack.MaxLength);

            var bob = returnedPlayers.FirstOrDefault(p => p.PlayerName.Value == "Bob");
            Assert.False(bob.IsCreator);
            Assert.Equal(rules.RackSize, bob.Rack.Letters.Count);
            Assert.Equal(rules.RackSize, bob.Rack.MaxLength);

            Assert.NotNull(gameState.Board);
        }
        #endregion

        #region Rack
        [Fact]
        public void TestRackValidCreation_CreatesRack()
        {
            var rack = new Rack("ABCDEFG".Select(c => c + "").ToList());

            Assert.Equal(7, rack.MaxLength);
            Assert.Equal("ABCDEFG".Select(c => c + "").ToList(), rack.Letters);
        }

        [Fact]
        public void TestRackNullCreation_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => new Rack((List<string>)null));
        }

        [Fact]
        public void TestRackValidRemove_RemovesLetter()
        {
            var rack = new Rack("ABCDEFG".Select(c => c + "").ToList());
            rack.RemoveLetter("B");

            Assert.Equal("ACDEFG".Select(c => c + "").ToList(), rack.Letters);
            Assert.Equal(7, rack.MaxLength);
        }

        [Fact]
        public void TestRackRemoveNotPresent_ThrowsException()
        {
            var rack = new Rack("ABCDEFG".Select(c => c + "").ToList());
            Assert.Throws<LetterNotOnRackException>(() => rack.RemoveLetter("H"));
        }

        [Fact]
        public void TestRackRemoveWhenEmpty_ThrowsException()
        {
            var rack = new Rack("".Select(c => c + "").ToList());
            Assert.Throws<LetterNotOnRackException>(() => rack.RemoveLetter("H"));
        }

        [Fact]
        public void TestRackValidAdd_AddsLetterToEnd()
        {
            var rack = new Rack("ABCDEFG".Select(c => c + "").ToList());
            rack.RemoveLetter("B");
            rack.AddLetter("H");

            Assert.Equal("ACDEFGH".Select(c => c + "").ToList(), rack.Letters);
            Assert.Equal(7, rack.MaxLength);
        }

        [Fact]
        public void TestRackAddWhenFull_ThrowsException()
        {
            var rack = new Rack("ABCDEFG".Select(c => c + "").ToList());
            Assert.Throws<FullRackException>(() => rack.AddLetter("H"));
        }

        [Fact]
        public void TestRackToString_ReturnsLetters()
        {
            var rack = new Rack("ABCDEFG".Select(c => c + "").ToList());

            Assert.Equal("ABCDEFG".Select(c => c + "").ToList(), rack.Letters);
        }

        #endregion

        #region TileBag
        [Fact]
        public void TestTileBagValidCreation_CreatesTileBag()
        {
            var bag = new TileBag("AABCDEEFGHIIJKLMNOOPYQSTUUVWXYZ".Select(c => c + "").ToList());

            Assert.Equal(31, bag.Count);
        }

        [Fact]
        public void TestTileBagNullCreation_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => new TileBag((List<string>)null));
        }

        [Fact]
        public void TestTileBagWhenValidGetNextSingle_ReturnsAndUpdates()
        {
            var bag = new TileBag("AABCDEEFGHIIJKLMNOOPYQSTUUVWXYZ".Select(c => c + "").ToList());

            var next = bag.GetNext();

            Assert.NotNull(next);
            Assert.Equal(30, bag.Count);
        }

        [Fact]
        public void TestTileBagWhenGetNextSeveral_ReturnsAndUpdates()
        {
            var bag = new TileBag("AABCDEEFGHIIJKLMNOOPYQSTUUVWXYZ".Select(c => c + "").ToList());

            var next = bag.GetNext(3);

            Assert.NotNull(next);
            Assert.Equal(3, next.Count);
            Assert.Equal(28, bag.Count);
        }

        [Fact]
        public void TestTileBagWhenGetNextAll_ReturnsAndUpdates()
        {
            var bag = new TileBag("AEIOU".Select(c => c + "").ToList());

            var next = bag.GetNext(5);

            Assert.NotNull(next);
            Assert.Equal(5, next.Count);
            Assert.Equal(0, bag.Count);
        }

        [Fact]
        public void TestTileBagWhenGetNextTooMany_ThrowsException()
        {
            var bag = new TileBag("AEIOU".Select(c => c + "").ToList());

            Assert.Throws<NoMoreLettersException>(() => bag.GetNext(6));
        }

        [Fact]
        public void TestTileBagWhenGetNextZero_ThrowsException()
        {
            var bag = new TileBag("AEIOU".Select(c => c + "").ToList());

            Assert.Throws<ArgumentOutOfRangeException>(() => bag.GetNext(0));
        }

        [Fact]
        public void TestTileBagToString_ReturnsLetters()
        {
            var bag = new TileBag("AAA".Select(c => c + "").ToList());

            Assert.Equal("[A, A, A]", bag.ToString());
        }
        #endregion

        #region Turn

        [Fact]
        public void TestTurnWithNullPlayers_ThrowsException()
        {

            Assert.Throws<ArgumentNullException>(() => new TurnOrder(null));
        }

        [Fact]
        public void TestTurnWithFourPlayers_WorksAsExpected()
        {
            var anna = new Player(new PlayerName("Anna"), true);
            var bert = new Player(new PlayerName("Bert"), true);
            var carol = new Player(new PlayerName("Carol"), true);
            var dave = new Player(new PlayerName("Dave"), true);
            var players = new List<Player> { anna, bert, carol, dave };

            var seed = 1234567;

            var turn = new TurnOrder(players, seed);

            Assert.Equal(bert.PlayerId.Value, turn.CurrentPlayerId.Value);
            Assert.Equal(0, turn.TurnsToWait(bert.PlayerId));
            Assert.Equal(1, turn.TurnsToWait(anna.PlayerId));
            Assert.Equal(2, turn.TurnsToWait(carol.PlayerId));
            Assert.Equal(3, turn.TurnsToWait(dave.PlayerId));

            turn.NextTurn();

            Assert.Equal(anna.PlayerId.Value, turn.CurrentPlayerId.Value);
            Assert.Equal(0, turn.TurnsToWait(anna.PlayerId));
            Assert.Equal(1, turn.TurnsToWait(carol.PlayerId));
            Assert.Equal(2, turn.TurnsToWait(dave.PlayerId));
            Assert.Equal(3, turn.TurnsToWait(bert.PlayerId));
        }

        [Fact]
        public void TestTurnToString_ReturnsTurns()
        {
            var anna = new Player(new PlayerName("Anna"), true);
            var bert = new Player(new PlayerName("Bert"), true);
            var carol = new Player(new PlayerName("Carol"), true);
            var dave = new Player(new PlayerName("Dave"), true);
            var players = new List<Player> { anna, bert, carol, dave };

            var seed = 1234567;

            var turn = new TurnOrder(players, seed);

            var expected = $"[{bert.PlayerId}, {anna.PlayerId}, {carol.PlayerId}, {dave.PlayerId}] - 1 (0)";
            Assert.Equal(expected, turn.ToString());
        }
        #endregion

        #region Board
        [Fact]
        public void TestBoardBlank_Creates()
        {
            var rules = new GameRules(Ruleset.SmallUpwords);
            var board = new Board(rules);

            Assert.IsType<Board>(board);
            Assert.NotNull(board);

            for (int row = 1; row <= GameRules.MaxRows; row++)
            {
                for (int column = 1; column <= GameRules.MaxColumns; column++)
                {
                    Assert.Equal("", board.GetSquare(row, column).Letter);
                    Assert.Equal(0, board.GetSquare(row, column).Height);
                }
            }
        }

        [Fact]
        public void TestBoardChangeOne_Updates()
        {
            var rules = new GameRules(Ruleset.SmallUpwords);
            var board = new Board(rules);

            board.GetSquare(4, 4).AddTile("C");
            Assert.Equal("C", board.GetSquare(4, 4).Letter);
            Assert.Equal(1, board.GetSquare(4, 4).Height);

            for (int row = 1; row <= GameRules.MaxRows; row++)
            {
                for (int column = 1; column <= GameRules.MaxColumns; column++)
                {
                    if (row == 4 && column == 4) continue;
                    Assert.True("" == board.GetSquare(row, column).Letter, $"({row}, {column}) is set to '{board.GetSquare(row, column).Letter}'.");
                    Assert.Equal(0, board.GetSquare(row, column).Height);
                }
            }
        }

        [Fact]
        public void TestBoardChangeTwo_Updates()
        {
            var rules = new GameRules(Ruleset.SmallUpwords);
            var board = new Board(rules);

            board.GetSquare(4, 4).AddTile("A");
            board.GetSquare(5, 5).AddTile("F");

            Assert.Equal("A", board.GetSquare(4, 4).Letter);
            Assert.Equal(1, board.GetSquare(4, 4).Height);

            Assert.Equal("F", board.GetSquare(5, 5).Letter);
            Assert.Equal(1, board.GetSquare(5, 5).Height);

            for (int row = 1; row <= GameRules.MaxRows; row++)
            {
                for (int column = 1; column <= GameRules.MaxColumns; column++)
                {
                    if (row == 4 && column == 4) continue;
                    if (row == 5 && column == 5) continue;
                    Assert.True("" == board.GetSquare(row, column).Letter, $"({row}, {column}) is set to '{board.GetSquare(row, column).Letter}'.");
                    Assert.Equal(0, board.GetSquare(row, column).Height);
                }
            }
        }

        #endregion

        #region GameRule
        [Fact]
        public void TestGameRuleWhenSmallUpwords_CreatedAsExpected()
        {
            var rules = new GameRules(Ruleset.SmallUpwords);

            Assert.Equal(7, rules.RackSize);
            Assert.Equal(64, rules.Letters.Count);
        }

        [Fact]
        public void TestGameRuleFromName_CreatedAsExpected()
        {
            var rules = new GameRules("SmallUpwords");

            Assert.Equal(Ruleset.SmallUpwords, rules.RuleSet);
            Assert.Equal(7, rules.RackSize);
            Assert.Equal(64, rules.Letters.Count);
        }


        #endregion

        private bool ValidPlayerId(string playerId)
        {
            if (Guid.TryParse(playerId, out _)) return true;
            return false;
        }

        private bool ValidGameId(string gameId)
        {
            foreach (var ch in gameId)
            {
                if (!"ABCDEFGHIJKLMNOPQRSTUVWXYZ".Contains(ch)) return false;
            }

            return true;
        }
    }
}
