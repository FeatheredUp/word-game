using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Words.API.Controllers;
using Words.API.DataModels;
using Words.API.Repository;
using Xunit;
using Words.API.ViewModels;

namespace Words.Test
{
    public class ControllerTests
    {
        #region Create
        [Fact]
        public void TestCreateWithValidName_SavesGame()
        {
            var mockLogger = new Mock<ILogger<GameController>>();

            var mockRepository = new Mock<IRepository>(MockBehavior.Strict);
            mockRepository.Setup(repo => repo.SaveCreatedGame(It.IsAny<CreatedGame>()));
            mockRepository.Setup(repo => repo.SynchLock).Returns(1);

            var gameController = new GameController(mockLogger.Object, mockRepository.Object);

            var createResult = gameController.Create("Susan");

            Assert.NotNull(createResult);
            Assert.NotNull(createResult.GameId);
            Assert.NotNull(createResult.PlayerId);
            Assert.Null(createResult.ErrorResult);

            mockRepository.Verify(repo => repo.SaveCreatedGame(It.IsAny<CreatedGame>()), Times.Once);
            mockRepository.Verify(repo => repo.SynchLock, Times.AtLeastOnce);
            mockRepository.VerifyNoOtherCalls();
        }

        [Fact]
        public void TestCreateWithBlankName_ReturnsError()
        {
            var mockLogger = new Mock<ILogger<GameController>>();

            var mockRepository = new Mock<IRepository>(MockBehavior.Strict);
            mockRepository.Setup(repo => repo.SynchLock).Returns(1);

            var gameController = new GameController(mockLogger.Object, mockRepository.Object);

            var createResult = gameController.Create("");

            Assert.NotNull(createResult);
            Assert.Null(createResult.GameId);
            Assert.Null(createResult.PlayerId);
            Assert.Equal("Player name <blank> is not valid.", createResult.ErrorResult.ErrorMessage);

            mockRepository.Verify(repo => repo.SynchLock);
            mockRepository.VerifyNoOtherCalls();
        }
        #endregion

        #region Join

        [Fact]
        public void TestJoinGameWithValidDetails_ReturnsPlayerId()
        {
            var mockLogger = new Mock<ILogger<GameController>>();

            var mockRepository = new Mock<IRepository>(MockBehavior.Strict);
            mockRepository.Setup(repo => repo.DoesGameExist(It.IsAny<GameId>())).Returns(true);
            mockRepository.Setup(repo => repo.HasGameStarted(It.IsAny<GameId>())).Returns(false);
            mockRepository.Setup(repo => repo.IsGameAtCapacity(It.IsAny<GameId>(), It.IsAny<int>())).Returns(false);
            var existingPlayers = new List<Player>() { new Player(new PlayerName("Bob"), true) };
            mockRepository.Setup(repo => repo.GetPlayers(It.IsAny<GameId>())).Returns(existingPlayers);
            mockRepository.Setup(repo => repo.JoinGame(It.IsAny<GameId>(), It.IsAny<Player>()));
            mockRepository.Setup(repo => repo.SynchLock).Returns(1);

            var gameController = new GameController(mockLogger.Object, mockRepository.Object);

            var joinResult = gameController.Join("ABCD", "Susan");

            Assert.NotNull(joinResult?.PlayerId);
            Assert.Null(joinResult?.ErrorResult);

            mockRepository.Verify(repo => repo.DoesGameExist(It.IsAny<GameId>()));
            mockRepository.Verify(repo => repo.HasGameStarted(It.IsAny<GameId>()));
            mockRepository.Verify(repo => repo.IsGameAtCapacity(It.IsAny<GameId>(), It.IsAny<int>()));
            mockRepository.Verify(repo => repo.GetPlayers(It.IsAny<GameId>()));
            mockRepository.Verify(repo => repo.JoinGame(It.IsAny<GameId>(), It.IsAny<Player>()));
            mockRepository.Verify(repo => repo.SynchLock);
            mockRepository.VerifyNoOtherCalls();
        }

        [Fact]
        public void TestJoinGameWithInvalidGameId_ReturnsError()
        {
            var mockLogger = new Mock<ILogger<GameController>>();

            var mockRepository = new Mock<IRepository>(MockBehavior.Strict);
            mockRepository.Setup(repo => repo.SynchLock).Returns(1);

            var gameController = new GameController(mockLogger.Object, mockRepository.Object);

            var joinResult = gameController.Join("1ABC", "Susan");

            Assert.Null(joinResult?.PlayerId);
            Assert.Equal("Game 1ABC is not valid.", joinResult?.ErrorResult.ErrorMessage);

            mockRepository.Verify(repo => repo.SynchLock);
            mockRepository.VerifyNoOtherCalls();
        }

        [Fact]
        public void TestJoinGameWithNonExistentGameId_ReturnsError()
        {
            var mockLogger = new Mock<ILogger<GameController>>();

            var mockRepository = new Mock<IRepository>(MockBehavior.Strict);
            mockRepository.Setup(repo => repo.DoesGameExist(It.IsAny<GameId>())).Returns(false);
            mockRepository.Setup(repo => repo.SynchLock).Returns(1);

            var gameController = new GameController(mockLogger.Object, mockRepository.Object);

            var joinResult = gameController.Join("ABCD", "Susan");

            Assert.Null(joinResult?.PlayerId);
            Assert.Equal("Game ABCD does not exist.", joinResult?.ErrorResult.ErrorMessage);

            mockRepository.Verify(repo => repo.DoesGameExist(It.IsAny<GameId>()));
            mockRepository.Verify(repo => repo.SynchLock);
            mockRepository.VerifyNoOtherCalls();
        }

        [Fact]
        public void TestJoinGameWithPreExistingPlayerId_ReturnsError()
        {
            var mockLogger = new Mock<ILogger<GameController>>();

            var mockRepository = new Mock<IRepository>(MockBehavior.Strict);
            mockRepository.Setup(repo => repo.DoesGameExist(It.IsAny<GameId>())).Returns(true);
            mockRepository.Setup(repo => repo.HasGameStarted(It.IsAny<GameId>())).Returns(false);
            mockRepository.Setup(repo => repo.IsGameAtCapacity(It.IsAny<GameId>(), It.IsAny<int>())).Returns(false);
            var existingPlayers = new List<Player>() { new Player(new PlayerName("Susan"), true) };
            mockRepository.Setup(repo => repo.GetPlayers(It.IsAny<GameId>())).Returns(existingPlayers);
            mockRepository.Setup(repo => repo.SynchLock).Returns(1);

            var gameController = new GameController(mockLogger.Object, mockRepository.Object);

            var joinResult = gameController.Join("ABCD", "Susan");

            Assert.Null(joinResult?.PlayerId);
            Assert.Equal("Game ABCD already contains a player named Susan.", joinResult?.ErrorResult.ErrorMessage);

            mockRepository.Verify(repo => repo.DoesGameExist(It.IsAny<GameId>()));
            mockRepository.Verify(repo => repo.HasGameStarted(It.IsAny<GameId>()));
            mockRepository.Verify(repo => repo.IsGameAtCapacity(It.IsAny<GameId>(), It.IsAny<int>()));
            mockRepository.Verify(repo => repo.GetPlayers(It.IsAny<GameId>()));
            mockRepository.Verify(repo => repo.SynchLock);
            mockRepository.VerifyNoOtherCalls();
        }

        [Fact]
        public void TestJoinGameWithAlreadyStartedGame_ReturnsError()
        {
            var mockLogger = new Mock<ILogger<GameController>>();

            var mockRepository = new Mock<IRepository>(MockBehavior.Strict);
            mockRepository.Setup(repo => repo.DoesGameExist(It.IsAny<GameId>())).Returns(true);
            mockRepository.Setup(repo => repo.HasGameStarted(It.IsAny<GameId>())).Returns(true);
            mockRepository.Setup(repo => repo.SynchLock).Returns(1);

            var gameController = new GameController(mockLogger.Object, mockRepository.Object);

            var joinResult = gameController.Join("ABCD", "Susan");

            Assert.Null(joinResult?.PlayerId);
            Assert.Equal("Game ABCD is already in progress.", joinResult?.ErrorResult.ErrorMessage);

            mockRepository.Verify(repo => repo.DoesGameExist(It.IsAny<GameId>()));
            mockRepository.Verify(repo => repo.HasGameStarted(It.IsAny<GameId>()));
            mockRepository.Verify(repo => repo.SynchLock);
            mockRepository.VerifyNoOtherCalls();
        }
        #endregion

        #region Creating
        [Fact]
        public void TestCreatingWithValidDetails_ReturnsExpectedDetails()
        {
            var existingPlayers = new List<Player>() { new Player(new PlayerName("Susan"), true), new Player(new PlayerName("Bob"), false) };

            var mockLogger = new Mock<ILogger<GameController>>();

            var mockRepository = new Mock<IRepository>(MockBehavior.Strict);
            mockRepository.Setup(repo => repo.GetPlayers(It.IsAny<GameId>())).Returns(existingPlayers);
            mockRepository.Setup(repo => repo.DoesGameExist(It.IsAny<GameId>())).Returns(true);
            mockRepository.Setup(repo => repo.HasGameStarted(It.IsAny<GameId>())).Returns(false);
            mockRepository.Setup(repo => repo.SynchLock).Returns(1);

            var gameController = new GameController(mockLogger.Object, mockRepository.Object);

            var creatorId = existingPlayers.First(p => p.IsCreator).PlayerId.Value;

            var creatingResult = gameController.Creating("ABCD", creatorId);

            Assert.NotNull(creatingResult);
            Assert.False(creatingResult.HasStarted);
            Assert.True(creatingResult.CanStart);
            Assert.Equal(2, creatingResult.Players.Count());
            Assert.Null(creatingResult?.ErrorResult);

            mockRepository.Verify(repo => repo.GetPlayers(It.IsAny<GameId>()));
            mockRepository.Verify(repo => repo.DoesGameExist(It.IsAny<GameId>()));
            mockRepository.Verify(repo => repo.HasGameStarted(It.IsAny<GameId>()));
            mockRepository.Verify(repo => repo.SynchLock);
            mockRepository.VerifyNoOtherCalls();
        }

        [Fact]
        public void TestCreatingWithNonCreator_ReturnsExpectedDetails()
        {
            var existingPlayers = new List<Player>() { new Player(new PlayerName("Susan"), true), new Player(new PlayerName("Bob"), false) };

            var mockLogger = new Mock<ILogger<GameController>>();

            var mockRepository = new Mock<IRepository>(MockBehavior.Strict);
            mockRepository.Setup(repo => repo.GetPlayers(It.IsAny<GameId>())).Returns(existingPlayers);
            mockRepository.Setup(repo => repo.DoesGameExist(It.IsAny<GameId>())).Returns(true);
            mockRepository.Setup(repo => repo.HasGameStarted(It.IsAny<GameId>())).Returns(false);
            mockRepository.Setup(repo => repo.SynchLock).Returns(1);

            var gameController = new GameController(mockLogger.Object, mockRepository.Object);

            var nonCreatorId = existingPlayers.First(p => !p.IsCreator).PlayerId.Value;

            var creatingResult = gameController.Creating("ABCD", nonCreatorId);

            Assert.NotNull(creatingResult);
            Assert.False(creatingResult.HasStarted);
            Assert.False(creatingResult.CanStart);
            Assert.Equal(2, creatingResult.Players.Count());
            Assert.Null(creatingResult?.ErrorResult);

            mockRepository.Verify(repo => repo.GetPlayers(It.IsAny<GameId>()));
            mockRepository.Verify(repo => repo.DoesGameExist(It.IsAny<GameId>()));
            mockRepository.Verify(repo => repo.HasGameStarted(It.IsAny<GameId>()));
            mockRepository.Verify(repo => repo.SynchLock);
            mockRepository.Verify(repo => repo.SynchLock);
            mockRepository.VerifyNoOtherCalls();
        }

        [Fact]
        public void TestCreatingWithStartedGame_ReturnsExpectedDetails()
        {
            var existingPlayers = new List<Player>() { new Player(new PlayerName("Susan"), true), new Player(new PlayerName("Bob"), false) };

            var mockLogger = new Mock<ILogger<GameController>>();

            var mockRepository = new Mock<IRepository>(MockBehavior.Strict);
            mockRepository.Setup(repo => repo.GetPlayers(It.IsAny<GameId>())).Returns(existingPlayers);
            mockRepository.Setup(repo => repo.DoesGameExist(It.IsAny<GameId>())).Returns(true);
            mockRepository.Setup(repo => repo.HasGameStarted(It.IsAny<GameId>())).Returns(true);
            mockRepository.Setup(repo => repo.SynchLock).Returns(1);

            var gameController = new GameController(mockLogger.Object, mockRepository.Object);

            var nonCreatorId = existingPlayers.First(p => !p.IsCreator).PlayerId.Value;

            var creatingResult = gameController.Creating("ABCD", nonCreatorId);

            Assert.NotNull(creatingResult);
            Assert.True(creatingResult.HasStarted);
            Assert.False(creatingResult.CanStart);
            Assert.Equal(2, creatingResult.Players.Count());
            Assert.Null(creatingResult?.ErrorResult);

            mockRepository.Verify(repo => repo.GetPlayers(It.IsAny<GameId>()));
            mockRepository.Verify(repo => repo.DoesGameExist(It.IsAny<GameId>()));
            mockRepository.Verify(repo => repo.HasGameStarted(It.IsAny<GameId>()));
            mockRepository.Verify(repo => repo.SynchLock);
            mockRepository.VerifyNoOtherCalls();
        }

        [Fact]
        public void TestCreatingWithOnePlayer_ReturnsExpectedDetails()
        {
            var existingPlayers = new List<Player>() { new Player(new PlayerName("Susan"), true) };

            var mockLogger = new Mock<ILogger<GameController>>();

            var mockRepository = new Mock<IRepository>(MockBehavior.Strict);
            mockRepository.Setup(repo => repo.GetPlayers(It.IsAny<GameId>())).Returns(existingPlayers);
            mockRepository.Setup(repo => repo.DoesGameExist(It.IsAny<GameId>())).Returns(true);
            mockRepository.Setup(repo => repo.HasGameStarted(It.IsAny<GameId>())).Returns(false);
            mockRepository.Setup(repo => repo.SynchLock).Returns(1);

            var gameController = new GameController(mockLogger.Object, mockRepository.Object);

            var creatorId = existingPlayers.First(p => p.IsCreator).PlayerId.Value;

            var creatingResult = gameController.Creating("ABCD", creatorId);

            Assert.NotNull(creatingResult);
            Assert.False(creatingResult.HasStarted);
            Assert.False(creatingResult.CanStart);
            Assert.Single(creatingResult.Players);
            Assert.Null(creatingResult?.ErrorResult);

            mockRepository.Verify(repo => repo.GetPlayers(It.IsAny<GameId>()));
            mockRepository.Verify(repo => repo.DoesGameExist(It.IsAny<GameId>()));
            mockRepository.Verify(repo => repo.HasGameStarted(It.IsAny<GameId>()));
            mockRepository.Verify(repo => repo.SynchLock);
            mockRepository.VerifyNoOtherCalls();
        }

        [Fact]
        public void TestCreatingWithNonExistentGameId_ReturnsError()
        {
            var existingPlayers = new List<Player>() { new Player(new PlayerName("Susan"), true) };

            var mockLogger = new Mock<ILogger<GameController>>();

            var mockRepository = new Mock<IRepository>(MockBehavior.Strict);
            mockRepository.Setup(repo => repo.DoesGameExist(It.IsAny<GameId>())).Returns(false);
            mockRepository.Setup(repo => repo.SynchLock).Returns(1);

            var gameController = new GameController(mockLogger.Object, mockRepository.Object);

            var creatorId = existingPlayers.First(p => p.IsCreator).PlayerId.Value;

            var creatingResult = gameController.Creating("ABCD", creatorId);

            Assert.NotNull(creatingResult);
            Assert.False(creatingResult.CanStart);
            Assert.Null(creatingResult.Players);
            Assert.Equal("Game ABCD does not exist.", creatingResult.ErrorResult.ErrorMessage);

            mockRepository.Verify(repo => repo.DoesGameExist(It.IsAny<GameId>()));
            mockRepository.Verify(repo => repo.SynchLock);
            mockRepository.VerifyNoOtherCalls();
        }

        [Fact]
        public void TestCreatingWithPlayerNotInGame_ReturnsError()
        {
            var existingPlayers = new List<Player>() { new Player(new PlayerName("Susan"), true) };

            var mockLogger = new Mock<ILogger<GameController>>();

            var mockRepository = new Mock<IRepository>(MockBehavior.Strict);
            mockRepository.Setup(repo => repo.DoesGameExist(It.IsAny<GameId>())).Returns(true);
            mockRepository.Setup(repo => repo.GetPlayers(It.IsAny<GameId>())).Returns(existingPlayers);
            mockRepository.Setup(repo => repo.SynchLock).Returns(1);

            var gameController = new GameController(mockLogger.Object, mockRepository.Object);

            var madeUpId = Guid.NewGuid().ToString();

            var creatingResult = gameController.Creating("ABCD", madeUpId);

            Assert.NotNull(creatingResult);
            Assert.False(creatingResult.CanStart);
            Assert.Null(creatingResult.Players);
            Assert.Equal($"Game ABCD does not contain player with Id {madeUpId}.", creatingResult.ErrorResult.ErrorMessage);

            mockRepository.Verify(repo => repo.DoesGameExist(It.IsAny<GameId>()));
            mockRepository.Verify(repo => repo.GetPlayers(It.IsAny<GameId>()));
            mockRepository.Verify(repo => repo.SynchLock);
            mockRepository.VerifyNoOtherCalls();
        }

        [Fact]
        public void TestCreatingWithInvalidGameId_ReturnsError()
        {
            var mockLogger = new Mock<ILogger<GameController>>();

            var mockRepository = new Mock<IRepository>(MockBehavior.Strict);

            var gameController = new GameController(mockLogger.Object, mockRepository.Object);

            var creatingResult = gameController.Creating("ABC", "anything");

            Assert.NotNull(creatingResult);
            Assert.False(creatingResult.CanStart);
            Assert.Null(creatingResult.Players);
            Assert.Equal($"Game ABC is not valid.", creatingResult.ErrorResult.ErrorMessage);

            mockRepository.VerifyNoOtherCalls();
        }
        #endregion

        #region Start
        [Fact]
        public void TestStartWithValidDetails_ReturnsExpectedDetails()
        {
            var creator = new Player(new PlayerName("Anna"), true);
            var joiner = new Player(new PlayerName("Bob"), false);
            var existingPlayers = new List<Player>() { creator, joiner };

            var mockLogger = new Mock<ILogger<GameController>>();

            var mockRepository = new Mock<IRepository>(MockBehavior.Strict);
            mockRepository.Setup(repo => repo.DoesGameExist(It.IsAny<GameId>())).Returns(true);
            mockRepository.Setup(repo => repo.HasGameStarted(It.IsAny<GameId>())).Returns(false);
            mockRepository.Setup(repo => repo.GetPlayers(It.IsAny<GameId>())).Returns(existingPlayers);
            mockRepository.Setup(repo => repo.StartGame(It.IsAny<GameState>()));
            mockRepository.Setup(repo => repo.SynchLock).Returns(1);

            var gameController = new GameController(mockLogger.Object, mockRepository.Object);

            var startResult = gameController.Start("ABCD", creator.PlayerId.Value, "");

            Assert.Null(startResult.ErrorResult);

            mockRepository.Verify(repo => repo.DoesGameExist(It.IsAny<GameId>()));
            mockRepository.Verify(repo => repo.HasGameStarted(It.IsAny<GameId>()));
            mockRepository.Verify(repo => repo.GetPlayers(It.IsAny<GameId>()));
            mockRepository.Verify(repo => repo.StartGame(It.IsAny<GameState>()));
            mockRepository.Verify(repo => repo.SynchLock);
            mockRepository.VerifyNoOtherCalls();
        }

        [Fact]
        public void TestStartWithAlreadyStartedGame_ReturnsError()
        {
            var creator = new Player(new PlayerName("Anna"), true);
            var joiner = new Player(new PlayerName("Bob"), false);
            var existingPlayers = new List<Player>() { creator, joiner };

            var mockLogger = new Mock<ILogger<GameController>>();

            var mockRepository = new Mock<IRepository>(MockBehavior.Strict);
            mockRepository.Setup(repo => repo.DoesGameExist(It.IsAny<GameId>())).Returns(true);
            mockRepository.Setup(repo => repo.HasGameStarted(It.IsAny<GameId>())).Returns(true);
            mockRepository.Setup(repo => repo.SynchLock).Returns(1);

            var gameController = new GameController(mockLogger.Object, mockRepository.Object);

            var startResult = gameController.Start("ABCD", creator.PlayerId.Value, "");

            Assert.Equal("Game ABCD is already in progress.", startResult.ErrorResult.ErrorMessage);

            mockRepository.Verify(repo => repo.DoesGameExist(It.IsAny<GameId>()));
            mockRepository.Verify(repo => repo.HasGameStarted(It.IsAny<GameId>()));
            mockRepository.Verify(repo => repo.SynchLock);
            mockRepository.VerifyNoOtherCalls();
        }
        #endregion

        #region Wait
        [Fact]
        public void TestWaitWithValidDetails_ReturnsExpectedDetails()
        {
            var rules = new GameRules(Ruleset.SmallUpwords);
            var creator = new Player(new PlayerName("Anna"), true);
            creator.AddRack("NOPQ".Select(c => c + "").ToList());
            var joiner = new Player(new PlayerName("Bob"), false);
            joiner.AddRack("RSTU".Select(c => c + "").ToList());
            var existingPlayers = new List<Player>() { creator, joiner };

            var turn = new TurnOrder(existingPlayers);

            var gameState = new GameState(new GameId("ABCD"), existingPlayers, new TileBag("ABCDEFGHIJKLM".Select(c => c + "").ToList()), turn, null, rules, GameStatus.InProgress);

            var mockLogger = new Mock<ILogger<GameController>>();

            var mockRepository = new Mock<IRepository>(MockBehavior.Strict);
            mockRepository.Setup(repo => repo.DoesGameExist(It.IsAny<GameId>())).Returns(true);
            mockRepository.Setup(repo => repo.HasGameStarted(It.IsAny<GameId>())).Returns(true);
            mockRepository.Setup(repo => repo.GetGameState(It.IsAny<GameId>())).Returns(gameState);
            mockRepository.Setup(repo => repo.SynchLock).Returns(1);

            var gameController = new GameController(mockLogger.Object, mockRepository.Object);

            var waitResult = gameController.Wait("ABCD", creator.PlayerId.Value);

            Assert.Null(waitResult.ErrorResult);
            Assert.Equal(2, waitResult.Players.Count());
            Assert.NotNull(waitResult.Players.FirstOrDefault(p => p.PlayerName == "Anna"));
            Assert.NotNull(waitResult.Players.FirstOrDefault(p => p.PlayerName == "Bob"));
            Assert.Equal(4, waitResult.Rack.Count);
            Assert.Equal(13, waitResult.TilesLeft);

            mockRepository.Verify(repo => repo.DoesGameExist(It.IsAny<GameId>()));
            mockRepository.Verify(repo => repo.HasGameStarted(It.IsAny<GameId>()));
            mockRepository.Verify(repo => repo.GetGameState(It.IsAny<GameId>()));
            mockRepository.Verify(repo => repo.SynchLock);
            mockRepository.VerifyNoOtherCalls();
        }

        [Fact]
        public void TestWaitWithUnstartedGame_ReturnsError()
        {
            var creator = new Player(new PlayerName("Anna"), true);

            var mockLogger = new Mock<ILogger<GameController>>();

            var mockRepository = new Mock<IRepository>(MockBehavior.Strict);
            mockRepository.Setup(repo => repo.DoesGameExist(It.IsAny<GameId>())).Returns(true);
            mockRepository.Setup(repo => repo.HasGameStarted(It.IsAny<GameId>())).Returns(false);
            mockRepository.Setup(repo => repo.SynchLock).Returns(1);

            var gameController = new GameController(mockLogger.Object, mockRepository.Object);

            var waitResult = gameController.Wait("ABCD", creator.PlayerId.Value);

            Assert.Equal("Game ABCD has not started.", waitResult.ErrorResult.ErrorMessage);
            Assert.Null(waitResult.Players);
            Assert.Null(waitResult.Rack);
            Assert.Equal(0, waitResult.TilesLeft);

            mockRepository.Verify(repo => repo.DoesGameExist(It.IsAny<GameId>()));
            mockRepository.Verify(repo => repo.HasGameStarted(It.IsAny<GameId>()));
            mockRepository.Verify(repo => repo.SynchLock);
            mockRepository.VerifyNoOtherCalls();
        }
        #endregion

        #region Play
        [Fact]
        public void TestPlayWithValidDetails_ReturnsExpectedDetails()
        {
            var rules = new GameRules(Ruleset.SmallUpwords);
            var board = new Board(rules);
            var creator = new Player(new PlayerName("Anna"), true);
            creator.AddRack("ABCDEFT".Select(c => c + "").ToList());
            var joiner = new Player(new PlayerName("Bob"), false);
            joiner.AddRack("ABCDEFT".Select(c => c + "").ToList());
            var existingPlayers = new List<Player>() { creator, joiner };

            var turn = new TurnOrder(existingPlayers);

            var gameState = new GameState(new GameId("ABCD"), existingPlayers, new TileBag("NOPRSUVWXYZ".Select(c => c + "").ToList()), turn, board, rules, GameStatus.InProgress);

            var mockLogger = new Mock<ILogger<GameController>>();

            var mockRepository = new Mock<IRepository>(MockBehavior.Strict);
            mockRepository.Setup(repo => repo.DoesGameExist(It.IsAny<GameId>())).Returns(true);
            mockRepository.Setup(repo => repo.HasGameStarted(It.IsAny<GameId>())).Returns(true);
            mockRepository.Setup(repo => repo.GetGameState(It.IsAny<GameId>())).Returns(gameState);
            mockRepository.Setup(repo => repo.SaveGameState(It.IsAny<GameState>(), It.IsAny<PlayerId>()));
            mockRepository.Setup(repo => repo.WordsNotInDictionary(It.IsAny<IReadOnlyList<string>>())).Returns(new List<string>());
            mockRepository.Setup(repo => repo.SynchLock).Returns(1);

            var gameController = new GameController(mockLogger.Object, mockRepository.Object);

            var playInput = new PlayInput
            {
                GameId = "ABCD",
                PlayerId = turn.CurrentPlayerId.Value,
                TilePlacements =
                new List<TilePlacementInput>
                {
                    new TilePlacementInput { Column = 4, Row = 5, Letter = "C" },
                    new TilePlacementInput { Column = 5, Row = 5, Letter = "A" },
                    new TilePlacementInput { Column = 6, Row = 5, Letter = "T" }
                }
            };

            var playResult = gameController.Play(playInput);

            Assert.Null(playResult.ErrorResult);
            Assert.Equal(2, playResult.Players.Count());
            Assert.NotNull(playResult.Players.FirstOrDefault(p => p.PlayerName == "Anna"));
            Assert.NotNull(playResult.Players.FirstOrDefault(p => p.PlayerName == "Bob"));
            Assert.Equal(7, playResult.Rack.Count);
            Assert.Equal(8, playResult.TilesLeft);

            Assert.Single(playResult.LastWords);
            Assert.Equal("CAT", playResult.LastWords[0]);
            Assert.Equal(6, playResult.LastScore);

            mockRepository.Verify(repo => repo.DoesGameExist(It.IsAny<GameId>()));
            mockRepository.Verify(repo => repo.HasGameStarted(It.IsAny<GameId>()));
            mockRepository.Verify(repo => repo.GetGameState(It.IsAny<GameId>()));
            mockRepository.Verify(repo => repo.SaveGameState(It.IsAny<GameState>(), It.IsAny<PlayerId>()));
            mockRepository.Verify(repo => repo.WordsNotInDictionary(It.IsAny<IReadOnlyList<string>>()));
            mockRepository.Verify(repo => repo.SynchLock);
            mockRepository.VerifyNoOtherCalls();
        }

        [Fact]
        public void TestPlayAtEndGameWithValidDetails_ReturnsExpectedDetails()
        {
            var rules = new GameRules(Ruleset.SmallUpwords);
            var board = new Board(rules);
            var creator = new Player(new PlayerName("Anna"), true);
            creator.AddRack("ACT".Select(c => c + "").ToList());
            var joiner = new Player(new PlayerName("Bob"), false);
            joiner.AddRack("ACT".Select(c => c + "").ToList());
            var existingPlayers = new List<Player>() { creator, joiner };

            var turn = new TurnOrder(existingPlayers);

            var gameState = new GameState(new GameId("ABCD"), existingPlayers, new TileBag(new List<string>()), turn, board, rules, GameStatus.InProgress);

            var mockLogger = new Mock<ILogger<GameController>>();

            var mockRepository = new Mock<IRepository>(MockBehavior.Strict);
            mockRepository.Setup(repo => repo.DoesGameExist(It.IsAny<GameId>())).Returns(true);
            mockRepository.Setup(repo => repo.HasGameStarted(It.IsAny<GameId>())).Returns(true);
            mockRepository.Setup(repo => repo.GetGameState(It.IsAny<GameId>())).Returns(gameState);
            mockRepository.Setup(repo => repo.SaveEndGameState(It.IsAny<GameState>()));
            mockRepository.Setup(repo => repo.WordsNotInDictionary(It.IsAny<IReadOnlyList<string>>())).Returns(new List<string>());
            mockRepository.Setup(repo => repo.SynchLock).Returns(1);

            var gameController = new GameController(mockLogger.Object, mockRepository.Object);

            var playInput = new PlayInput
            {
                GameId = "ABCD",
                PlayerId = turn.CurrentPlayerId.Value,
                TilePlacements =
                new List<TilePlacementInput>
                {
                    new TilePlacementInput { Column = 4, Row = 5, Letter = "C" },
                    new TilePlacementInput { Column = 5, Row = 5, Letter = "A" },
                    new TilePlacementInput { Column = 6, Row = 5, Letter = "T" }
                }
            };

            var playResult = gameController.Play(playInput);

            Assert.Null(playResult.ErrorResult);
            Assert.Equal(2, playResult.Players.Count());
            Assert.NotNull(playResult.Players.FirstOrDefault(p => p.PlayerName == "Anna"));
            Assert.NotNull(playResult.Players.FirstOrDefault(p => p.PlayerName == "Bob"));
            Assert.Empty(playResult.Rack);
            Assert.Equal(0, playResult.TilesLeft);

            Assert.Single(playResult.LastWords);
            Assert.Equal("CAT", playResult.LastWords[0]);
            Assert.Equal(6, playResult.LastScore);

            mockRepository.Verify(repo => repo.DoesGameExist(It.IsAny<GameId>()));
            mockRepository.Verify(repo => repo.HasGameStarted(It.IsAny<GameId>()));
            mockRepository.Verify(repo => repo.GetGameState(It.IsAny<GameId>()));
            mockRepository.Verify(repo => repo.SaveEndGameState(It.IsAny<GameState>()));
            mockRepository.Verify(repo => repo.WordsNotInDictionary(It.IsAny<IReadOnlyList<string>>()));
            mockRepository.Verify(repo => repo.SynchLock);
            mockRepository.VerifyNoOtherCalls();
        }
        #endregion

        #region Pass
        [Fact]
        public void TestPassWithValidDetails_ReturnsExpectedDetails()
        {
            var rules = new GameRules(Ruleset.SmallUpwords);
            var board = new Board(rules);
            var creator = new Player(new PlayerName("Anna"), true);
            creator.AddRack("ABCDEFT".Select(c => c + "").ToList());
            var joiner = new Player(new PlayerName("Bob"), false);
            joiner.AddRack("ABCDEFT".Select(c => c + "").ToList());
            var existingPlayers = new List<Player>() { creator, joiner };

            var turn = new TurnOrder(existingPlayers);

            var gameState = new GameState(new GameId("ABCD"), existingPlayers, new TileBag("NOPRSUVWXYZ".Select(c => c + "").ToList()), turn, board, rules, GameStatus.InProgress);

            var mockLogger = new Mock<ILogger<GameController>>();

            var mockRepository = new Mock<IRepository>(MockBehavior.Strict);
            mockRepository.Setup(repo => repo.DoesGameExist(It.IsAny<GameId>())).Returns(true);
            mockRepository.Setup(repo => repo.HasGameStarted(It.IsAny<GameId>())).Returns(true);
            mockRepository.Setup(repo => repo.GetGameState(It.IsAny<GameId>())).Returns(gameState);
            mockRepository.Setup(repo => repo.SaveGameState(It.IsAny<GameState>(), It.IsAny<PlayerId>()));
            mockRepository.Setup(repo => repo.SynchLock).Returns(1);

            var gameController = new GameController(mockLogger.Object, mockRepository.Object);

            var passResult = gameController.Pass("ABCD", turn.CurrentPlayerId.Value);

            Assert.Null(passResult.ErrorResult);
            Assert.Equal(2, passResult.Players.Count());
            Assert.NotNull(passResult.Players.FirstOrDefault(p => p.PlayerName == "Anna"));
            Assert.NotNull(passResult.Players.FirstOrDefault(p => p.PlayerName == "Bob"));
            Assert.Equal(7, passResult.Rack.Count);
            Assert.Equal(11, passResult.TilesLeft);

            mockRepository.Verify(repo => repo.DoesGameExist(It.IsAny<GameId>()));
            mockRepository.Verify(repo => repo.HasGameStarted(It.IsAny<GameId>()));
            mockRepository.Verify(repo => repo.GetGameState(It.IsAny<GameId>()));
            mockRepository.Verify(repo => repo.SaveGameState(It.IsAny<GameState>(), It.IsAny<PlayerId>()));
            mockRepository.Verify(repo => repo.SynchLock);
            mockRepository.VerifyNoOtherCalls();
        }
        #endregion

        #region Swap
        [Fact]
        public void TestSwapWithValidDetails_ReturnsExpectedDetails()
        {
            var rules = new GameRules(Ruleset.SmallUpwords);
            var board = new Board(rules);
            var creator = new Player(new PlayerName("Anna"), true);
            creator.AddRack("ABCDEFT".Select(c => c + "").ToList());
            var joiner = new Player(new PlayerName("Bob"), false);
            joiner.AddRack("ABCDEFT".Select(c => c + "").ToList());
            var existingPlayers = new List<Player>() { creator, joiner };

            var turn = new TurnOrder(existingPlayers);

            var gameState = new GameState(new GameId("ABCD"), existingPlayers, new TileBag("NOPRSUVWXYZ".Select(c => c + "").ToList()), turn, board, rules, GameStatus.InProgress);

            var mockLogger = new Mock<ILogger<GameController>>();

            var mockRepository = new Mock<IRepository>(MockBehavior.Strict);
            mockRepository.Setup(repo => repo.DoesGameExist(It.IsAny<GameId>())).Returns(true);
            mockRepository.Setup(repo => repo.HasGameStarted(It.IsAny<GameId>())).Returns(true);
            mockRepository.Setup(repo => repo.GetGameState(It.IsAny<GameId>())).Returns(gameState);
            mockRepository.Setup(repo => repo.SaveGameState(It.IsAny<GameState>(), It.IsAny<PlayerId>()));
            mockRepository.Setup(repo => repo.SynchLock).Returns(1);

            var gameController = new GameController(mockLogger.Object, mockRepository.Object);

            var swapResult = gameController.Swap("ABCD", turn.CurrentPlayerId.Value, "T");

            Assert.Null(swapResult.ErrorResult);
            Assert.Equal(2, swapResult.Players.Count());
            Assert.NotNull(swapResult.Players.FirstOrDefault(p => p.PlayerName == "Anna"));
            Assert.NotNull(swapResult.Players.FirstOrDefault(p => p.PlayerName == "Bob"));
            Assert.Equal(7, swapResult.Rack.Count);
            Assert.DoesNotContain("T", swapResult.Rack);
            Assert.Contains("C", swapResult.Rack);
            Assert.Equal(11, swapResult.TilesLeft);

            mockRepository.Verify(repo => repo.DoesGameExist(It.IsAny<GameId>()));
            mockRepository.Verify(repo => repo.HasGameStarted(It.IsAny<GameId>()));
            mockRepository.Verify(repo => repo.GetGameState(It.IsAny<GameId>()));
            mockRepository.Verify(repo => repo.SaveGameState(It.IsAny<GameState>(), It.IsAny<PlayerId>()));
            mockRepository.Verify(repo => repo.SynchLock);
            mockRepository.VerifyNoOtherCalls();
        }
        #endregion

        #region TryPlay
        [Fact]
        public void TestTryPlayWithValidDetails_ReturnsExpectedDetails()
        {
            var rules = new GameRules(Ruleset.SmallUpwords);
            var board = new Board(rules);
            var creator = new Player(new PlayerName("Anna"), true);
            creator.AddRack("ABCDEFT".Select(c => c + "").ToList());
            var joiner = new Player(new PlayerName("Bob"), false);
            joiner.AddRack("ABCDEFT".Select(c => c + "").ToList());
            var existingPlayers = new List<Player>() { creator, joiner };

            var turn = new TurnOrder(existingPlayers);

            var gameState = new GameState(new GameId("ABCD"), existingPlayers, new TileBag("NOPRSUVWXYZ".Select(c => c + "").ToList()), turn, board, rules, GameStatus.InProgress);

            var mockLogger = new Mock<ILogger<GameController>>();

            var mockRepository = new Mock<IRepository>(MockBehavior.Strict);
            mockRepository.Setup(repo => repo.DoesGameExist(It.IsAny<GameId>())).Returns(true);
            mockRepository.Setup(repo => repo.HasGameStarted(It.IsAny<GameId>())).Returns(true);
            mockRepository.Setup(repo => repo.GetGameState(It.IsAny<GameId>())).Returns(gameState);
            mockRepository.Setup(repo => repo.WordsNotInDictionary(It.IsAny<IReadOnlyList<string>>())).Returns(new List<string>());
            mockRepository.Setup(repo => repo.SynchLock).Returns(1);

            var gameController = new GameController(mockLogger.Object, mockRepository.Object);

            var playInput = new PlayInput
            {
                GameId = "ABCD",
                PlayerId = turn.CurrentPlayerId.Value,
                TilePlacements =
                new List<TilePlacementInput>
                {
                    new TilePlacementInput { Column = 4, Row = 5, Letter = "C" },
                    new TilePlacementInput { Column = 5, Row = 5, Letter = "A" },
                    new TilePlacementInput { Column = 6, Row = 5, Letter = "T" }
                }
            };

            var tryPlayResult = gameController.TryPlay(playInput);

            Assert.Null(tryPlayResult.ErrorResult);
            Assert.Single(tryPlayResult.LastWords);
            Assert.Equal("CAT", tryPlayResult.LastWords[0]);
            Assert.Equal(6, tryPlayResult.LastScore);

            mockRepository.Verify(repo => repo.DoesGameExist(It.IsAny<GameId>()));
            mockRepository.Verify(repo => repo.HasGameStarted(It.IsAny<GameId>()));
            mockRepository.Verify(repo => repo.GetGameState(It.IsAny<GameId>()));
            mockRepository.Verify(repo => repo.WordsNotInDictionary(It.IsAny<IReadOnlyList<string>>()));
            mockRepository.Verify(repo => repo.SynchLock);
            mockRepository.VerifyNoOtherCalls();
        }

        #endregion
    }
}
