using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Words.API.Controllers;
using Words.API.Repository;
using Words.API.ViewModels;
using Xunit;

namespace Words.Test
{
    public class IntegrationTests
    {
        // Add [Fact] here to make this run, but the Play will fail because it won't be a word most of the time.
        public void IntegrationTest()
        {
            var mockLogger = new Mock<ILogger<GameController>>();
            IRepository repository = new LiteRepository();
            var gameController = new GameController(mockLogger.Object, repository);

            // Create
            var createResult = gameController.Create("Anna");
            Assert.Null(createResult.ErrorResult);
            Assert.NotNull(createResult.GameId);
            Assert.NotNull(createResult.PlayerId);

            // Join
            var joinResult = gameController.Join(createResult.GameId, "Bert");
            Assert.Null(joinResult.ErrorResult);
            Assert.NotNull(joinResult.PlayerId);

            // Start
            var startResult = gameController.Start(createResult.GameId, createResult.PlayerId, "SmallUpwords");
            Assert.Null(startResult.ErrorResult);

            // Each player starts polling
            var annaResult = gameController.Wait(createResult.GameId, createResult.PlayerId);
            var bertResult = gameController.Wait(createResult.GameId, joinResult.PlayerId);

            Assert.Null(annaResult.ErrorResult);
            Assert.Equal(1, annaResult.TurnNumber);
            Assert.Equal(50, annaResult.TilesLeft);
            Assert.Equal(2, annaResult.Players.Count());
            Assert.Equal(7, annaResult.Rack.Count);

            var middleSquare = annaResult.Board[5][5];
            Assert.Equal(0, middleSquare.Height);
            Assert.Equal("", middleSquare.Letter);
            Assert.Equal(SquareTypeResult.Starting, middleSquare.SquareType);

            var topLeftTile = annaResult.Board[0][0];
            Assert.Equal(0, topLeftTile.Height);
            Assert.Equal("", topLeftTile.Letter);
            Assert.Equal(SquareTypeResult.Unplayable, topLeftTile.SquareType);

            var ordinarySquare = annaResult.Board[7][2];
            Assert.Equal(0, ordinarySquare.Height);
            Assert.Equal("", ordinarySquare.Letter);
            Assert.Equal(SquareTypeResult.Standard, ordinarySquare.SquareType);

            Assert.Null(bertResult.ErrorResult);
            Assert.Equal(1, bertResult.TurnNumber);
            Assert.Equal(50, bertResult.TilesLeft);
            Assert.Equal(2, bertResult.Players.Count());
            Assert.Equal(7, bertResult.Rack.Count);

            Assert.NotEqual(annaResult.IsMyTurn, bertResult.IsMyTurn);

            // Play a move
            var firstPlayerId = annaResult.IsMyTurn ? createResult.PlayerId : joinResult.PlayerId;
            var secondPlayerId = annaResult.IsMyTurn ? joinResult.PlayerId : createResult.PlayerId;
            var firstPlayer = annaResult.IsMyTurn ? annaResult : bertResult;

            var letterToCheck = firstPlayer.Rack[4];

            var play = new PlayInput
            {
                GameId = createResult.GameId,
                PlayerId = firstPlayerId,
                TilePlacements = new List<TilePlacementInput>
                {
                    new TilePlacementInput { Row = 5, Column = 5, Letter = firstPlayer.Rack[2] },
                    new TilePlacementInput { Row = 5, Column = 6, Letter = firstPlayer.Rack[4] },
                    new TilePlacementInput { Row = 5, Column = 7, Letter = firstPlayer.Rack[6] }
                }
            };

            var playResult = gameController.Play(play);

            Assert.True(playResult.ErrorResult == null, playResult.ErrorResult?.ErrorMessage);

            var squareToCheck = playResult.Board[4][5];
            Assert.Equal(1, squareToCheck.Height);
            Assert.Equal(letterToCheck, squareToCheck.Letter);
            Assert.Equal(SquareTypeResult.Standard, squareToCheck.SquareType);

            // First player polls, should be turn 2 and no longer this player's turn
            var repollResult = gameController.Wait(createResult.GameId, firstPlayerId);
            Assert.Null(repollResult.ErrorResult);
            Assert.Equal(2, repollResult.TurnNumber);
            Assert.Equal(47, repollResult.TilesLeft);
            Assert.Equal(2, repollResult.Players.Count());
            Assert.Equal(7, repollResult.Rack.Count);
            Assert.False(repollResult.IsMyTurn);

            var squareToRecheck = repollResult.Board[4][5];
            Assert.Equal(1, squareToRecheck.Height);
            Assert.Equal(letterToCheck, squareToRecheck.Letter);
            Assert.Equal(SquareTypeResult.Standard, squareToRecheck.SquareType);

            //Second player polls, should get board back, and be this player's turn
            var repollResultPlayer2 = gameController.Wait(createResult.GameId, secondPlayerId);
            Assert.Null(repollResultPlayer2.ErrorResult);
            Assert.Equal(2, repollResultPlayer2.TurnNumber);
            Assert.Equal(47, repollResultPlayer2.TilesLeft);
            Assert.Equal(2, repollResultPlayer2.Players.Count());
            Assert.Equal(7, repollResultPlayer2.Rack.Count);
            Assert.True(repollResultPlayer2.IsMyTurn);

            var squareToRecheckPlayer2 = repollResultPlayer2.Board[4][5];
            Assert.Equal(1, squareToRecheckPlayer2.Height);
            Assert.Equal(letterToCheck, squareToRecheckPlayer2.Letter);
            Assert.Equal(SquareTypeResult.Standard, squareToRecheckPlayer2.SquareType);
        }
    }
}
