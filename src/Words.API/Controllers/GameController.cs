using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Words.API.ViewModels;
using Words.API.DataModels;
using Words.API.Repository;
using Words.API.Logic;
using System;
using System.Linq;
using Words.API.Exceptions;

namespace Words.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GameController : ControllerBase
    {
        private readonly ILogger<GameController> _logger;
        private readonly IRepository _repository;

        public GameController(ILogger<GameController> logger, IRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }

        [HttpGet("create")]
        public CreateResult Create(string name)
        {
            try
            {
                lock (_repository.SyncLock)
                {
                    var game = new GameLogic(_repository);
                    var newGame = game.Create(new PlayerName(name));

                    _logger.LogDebug($"Created new game {newGame.GameId} for player {newGame.Player.PlayerName} ({newGame.Player.PlayerId})");

                    return new CreateResult(newGame);
                }
            }
            catch (ValidationException ex)
            {
                return new CreateResult(ex, ErrorType.ValidationError);
            }
            catch (Exception ex)
            {
                return new CreateResult(ex, ErrorType.SystemError);
            }
        }

        [HttpGet("join")]
        public JoinResult Join(string gameId, string name)
        {
            try
            {
                lock (_repository.SyncLock)
                {
                    var game = new GameLogic(_repository);
                    var playerId = game.Join(new GameId(gameId?.ToUpper()), new PlayerName(name));

                    _logger.LogDebug($"Joined game {gameId} for player {name} ({playerId})");

                    return new JoinResult(playerId);
                }
            }
            catch (ValidationException ex)
            {
                return new JoinResult(ex, ErrorType.ValidationError);
            }
            catch (Exception ex)
            {
                return new JoinResult(ex, ErrorType.SystemError);
            }
        }

        [HttpGet("creating")]
        public CreatingResult Creating(string gameId, string playerId)
        {
            try
            {
                var playerIdModel = new PlayerId(playerId);
                var gameIdModel = new GameId(gameId);

                lock (_repository.SyncLock)
                {
                    var game = new GameLogic(_repository);
                    var players = game.GetLoadingPlayers(gameIdModel, playerIdModel);
                    var hasStarted = game.HasGameStarted(gameIdModel);

                    return new CreatingResult(players, playerIdModel, hasStarted);
                }
            }
            catch (ValidationException ex)
            {
                return new CreatingResult(ex, ErrorType.ValidationError);
            }
            catch (Exception ex)
            {
                return new CreatingResult(ex, ErrorType.SystemError);
            }
        }

        [HttpGet("start")]
        public StartResult Start(string gameId, string playerId, string ruleset)
        {
            try
            {
                lock (_repository.SyncLock)
                {
                    var game = new GameLogic(_repository);
                    var state = game.Start(new GameId(gameId), new PlayerId(playerId), GameRules.GetRulesetOrDefault(ruleset));

                    _logger.LogDebug($"Started game {gameId} with ruleset {ruleset}");

                    return new StartResult();
                }
            }
            catch (ValidationException ex)
            {
                return new StartResult(ex, ErrorType.ValidationError);
            }
            catch (Exception ex)
            {
                return new StartResult(ex, ErrorType.SystemError);
            }
        }

        [HttpGet("wait")]
        public WaitResult Wait(string gameId, string playerId)
        {
            try
            {
                lock (_repository.SyncLock)
                {
                    var game = new GameLogic(_repository);
                    var state = game.Poll(new GameId(gameId), new PlayerId(playerId));

                    return new WaitResult(state, playerId);
                }
            }
            catch (ValidationException ex)
            {
                return new WaitResult(ex, ErrorType.ValidationError);
            }
            catch (Exception ex)
            {
                return new WaitResult(ex, ErrorType.SystemError);
            }
        }

        [HttpPost("play")]
        public PlayResult Play([FromBody]PlayInput playInput)
        {
            try
            {
                if (playInput == null) throw new ArgumentNullException(nameof(playInput));

                var gameId = new GameId(playInput.GameId);
                var playerId = new PlayerId(playInput.PlayerId);
                var placements = playInput.TilePlacements?.Select(tp => new TilePlacement(tp.Letter, tp.Row, tp.Column)).ToList();

                lock (_repository.SyncLock)
                {
                    var game = new GameLogic(_repository);
                    var state = game.Play(gameId, playerId, placements);

                    _logger.LogDebug($"Game {gameId}, Player {playerId}, played {string.Join(", ", placements)}");

                    return new PlayResult(state, playerId.Value);
                }
            }
            catch (ValidationException ex)
            {
                return new PlayResult(ex, ErrorType.ValidationError);
            }
            catch (Exception ex)
            {
                return new PlayResult(ex, ErrorType.SystemError);
            }
        }


        [HttpPost("tryplay")]
        public TryPlayResult TryPlay([FromBody] PlayInput playInput)
        {
            try
            {
                if (playInput == null) throw new ArgumentNullException(nameof(playInput));

                var gameId = new GameId(playInput.GameId);
                var playerId = new PlayerId(playInput.PlayerId);
                var placements = playInput.TilePlacements?.Select(tp => new TilePlacement(tp.Letter, tp.Row, tp.Column)).ToList();

                lock (_repository.SyncLock)
                {
                    var game = new GameLogic(_repository);
                    var turn = game.TryPlay(gameId, playerId, placements);

                    _logger.LogDebug($"Game {gameId}, Player {playerId}, tried playing {string.Join(", ", placements)}");

                    return new TryPlayResult(turn);
                }
            }
            catch (ValidationException ex)
            {
                return new TryPlayResult(ex, ErrorType.ValidationError);
            }
            catch (Exception ex)
            {
                return new TryPlayResult(ex, ErrorType.SystemError);
            }
        }

        [HttpGet("pass")]
        public PassResult Pass(string gameId, string playerId)
        {
            try
            {
                var gameIdModel = new GameId(gameId);
                var playerIdModel = new PlayerId(playerId);

                lock (_repository.SyncLock)
                {
                    var game = new GameLogic(_repository);
                    var state = game.Pass(gameIdModel, playerIdModel);

                    _logger.LogDebug($"Game {gameId}, Player {playerId}, passed");

                    return new PassResult(state, playerId);
                }
            }
            catch (ValidationException ex)
            {
                return new PassResult(ex, ErrorType.ValidationError);
            }
            catch (Exception ex)
            {
                return new PassResult(ex, ErrorType.SystemError);
            }
        }

        [HttpGet("swap")]
        public SwapResult Swap(string gameId, string playerId, string letter)
        {
            try
            {
                var gameIdModel = new GameId(gameId);
                var playerIdModel = new PlayerId(playerId);

                lock (_repository.SyncLock)
                {
                    var game = new GameLogic(_repository);
                    var state = game.Swap(gameIdModel, playerIdModel, letter);

                    _logger.LogDebug($"Game {gameId}, Player {playerId}, swapped '{letter}'");

                    return new SwapResult(state, playerId);
                }
            }
            catch (ValidationException ex)
            {
                return new SwapResult(ex, ErrorType.ValidationError);
            }
            catch (Exception ex)
            {
                return new SwapResult(ex, ErrorType.SystemError);
            }
        }

        [HttpGet("history")]
        public HistoryResult History(string gameId)
        {
            try
            {
                var gameIdModel = new GameId(gameId);

                lock (_repository.SyncLock)
                {
                    var game = new GameLogic(_repository);
                    var history = game.GetHistory(gameIdModel);
                    var players = game.GetPlayers(gameIdModel);

                    return new HistoryResult(history, players);
                }
            }
            catch (ValidationException ex)
            {
                return new HistoryResult(ex, ErrorType.ValidationError);
            }
            catch (Exception ex)
            {
                return new HistoryResult(ex, ErrorType.SystemError);
            }
        }
    }
}
