using System.Collections.Generic;
using System.Linq;
using Words.API.DataModels;
using Words.API.ViewModels;
using Xunit;

namespace Words.Test
{
    public class ViewModelTests
    {
        #region CreatingResult
        [Fact]
        public void TestCreatingResultWithAlreadyStarted_ReturnsStarted()
        {
            var existingPlayers = new List<Player>() {
                new Player(new PlayerName("Susan"), true) ,
                new Player(new PlayerName("Bob"), false) };

            var creatorPlayer = existingPlayers.FirstOrDefault(p => p.IsCreator)?.PlayerId;

            var state = new CreatingResult(existingPlayers, creatorPlayer, true);

            Assert.NotNull(state);
            Assert.True(state.HasStarted);
            Assert.False(state.CanStart);
            Assert.NotNull(state.Players);

            var players = state.Players.ToList();
            Assert.Equal(2, state.Players.Count());
            Assert.NotNull(state.Players.FirstOrDefault(p => p.PlayerName == "Susan"));
            Assert.NotNull(state.Players.FirstOrDefault(p => p.PlayerName == "Bob"));
        }

        [Fact]
        public void TestCreatingResultWithReadyToStart_ReturnsReadyToStart()
        {
            var existingPlayers = new List<Player>() {
                new Player(new PlayerName("Susan"), true) ,
                new Player(new PlayerName("Bob"), false) };

            var creatorPlayer = existingPlayers.FirstOrDefault(p => p.IsCreator)?.PlayerId;

            var state = new CreatingResult(existingPlayers, creatorPlayer, false);

            Assert.NotNull(state);
            Assert.False(state.HasStarted);
            Assert.True(state.CanStart);
            Assert.NotNull(state.Players);

            var players = state.Players.ToList();
            Assert.Equal(2, state.Players.Count());
            Assert.NotNull(state.Players.FirstOrDefault(p => p.PlayerName == "Susan"));
            Assert.NotNull(state.Players.FirstOrDefault(p => p.PlayerName == "Bob"));
        }

        [Fact]
        public void TestCreatingResultWithOnePlayer_ReturnsNotReadyToStart()
        {
            var existingPlayers = new List<Player>() {
                new Player(new PlayerName("Susan"), true) };

            var creatorPlayer = existingPlayers.FirstOrDefault(p => p.IsCreator)?.PlayerId;

            var state = new CreatingResult(existingPlayers, creatorPlayer, false);

            Assert.NotNull(state);
            Assert.False(state.HasStarted);
            Assert.False(state.CanStart);
            Assert.NotNull(state.Players);

            var players = state.Players.ToList();
            Assert.Single(state.Players);
            Assert.NotNull(state.Players.FirstOrDefault(p => p.PlayerName == "Susan"));
        }

        [Fact]
        public void TestCreatingResultWithNonCreator_ReturnsNotReadyToStart()
        {
            var existingPlayers = new List<Player>() {
                new Player(new PlayerName("Susan"), true) ,
                new Player(new PlayerName("Bob"), false) };

            var nonCreatorPlayer = existingPlayers.FirstOrDefault(p => !p.IsCreator)?.PlayerId;

            var state = new CreatingResult(existingPlayers, nonCreatorPlayer, false);

            Assert.NotNull(state);
            Assert.False(state.HasStarted);
            Assert.False(state.CanStart);
            Assert.NotNull(state.Players);

            var players = state.Players.ToList();
            Assert.Equal(2, state.Players.Count());
            Assert.NotNull(state.Players.FirstOrDefault(p => p.PlayerName == "Susan"));
            Assert.NotNull(state.Players.FirstOrDefault(p => p.PlayerName == "Bob"));
        }
        #endregion

        #region WaitResult
        [Fact]
        public void TestWaitResultWhenValid_ReturnsValidState()
        {
            var rules = new GameRules(Ruleset.SmallUpwords);
            var anna = new Player(new PlayerName("Anna"), true);
            var bob = new Player(new PlayerName("Bob"), false);

            var players = new List<Player> { anna, bob };
            var gameState = new GameState(new GameId("ABCD"), rules, players, GameStatus.InProgress);

            // I.e. Anna's view of the game
            var waitResult = new WaitResult(gameState, anna.PlayerId.Value);

            var topEdge = waitResult.Board[6][0];
            Assert.Equal("", topEdge.Letter);
            Assert.Equal(0, topEdge.Height);
            Assert.Equal(SquareTypeResult.Unplayable, topEdge.SquareType);

            var middle = waitResult.Board[5][5];
            Assert.Equal("", middle.Letter);
            Assert.Equal(0, middle.Height);
            Assert.Equal(SquareTypeResult.Starting, middle.SquareType);

            var ordinarySquare = waitResult.Board[8][3];
            Assert.Equal("", ordinarySquare.Letter);
            Assert.Equal(0, ordinarySquare.Height);
            Assert.Equal(SquareTypeResult.Standard, ordinarySquare.SquareType);

            var annasRack = gameState.Players.FirstOrDefault(p => p.PlayerName.Value == "Anna").Rack;

            Assert.Equal(annasRack.Letters, waitResult.Rack);

            var annaResult = waitResult.Players.FirstOrDefault(players => players.PlayerName == "Anna");
            Assert.NotNull(annaResult);
            Assert.Equal(anna.PlayerName.Value, annaResult.PlayerName);
            Assert.True(annaResult.IsCurrentPlayer);
            Assert.Equal(0, annaResult.Score);

            var bobResult = waitResult.Players.FirstOrDefault(players => players.PlayerName == "Bob");
            Assert.NotNull(bobResult);
            Assert.Equal(bob.PlayerName.Value, bobResult.PlayerName);
            Assert.False(bobResult.IsCurrentPlayer);
            Assert.Equal(0, bobResult.Score);

            // One will be 0, the other 1, but random which is which
            Assert.Equal(1, annaResult.TurnsToWait + bobResult.TurnsToWait);

            Assert.Equal(50, waitResult.TilesLeft);

            // Two ways of expressing the fact 'is it Anna's turn?'
            Assert.Equal(annaResult.TurnsToWait == 0, waitResult.IsMyTurn);
        }
        #endregion
    }
}