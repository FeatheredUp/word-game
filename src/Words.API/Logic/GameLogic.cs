using System.Collections.Generic;
using System.Linq;
using Words.API.Exceptions;
using Words.API.DataModels;
using Words.API.Repository;

namespace Words.API.Logic
{
    internal class GameLogic
    {
        private const int Capacity = 4;
        private readonly IRepository _repository;

        public GameLogic(IRepository repository)
        {
            _repository = repository;
        }

        public CreatedGame Create(PlayerName playerName)
        {
            var newGame = new CreatedGame(playerName);
            _repository.SaveCreatedGame(newGame);

            return newGame;
        }

        public PlayerId Join(GameId gameId, PlayerName playerName)
        {
            if (!_repository.DoesGameExist(gameId)) throw new GameDoesNotExistException(gameId);
            if (_repository.HasGameStarted(gameId)) throw new GameAlreadyInProgressException(gameId);
            if (_repository.IsGameAtCapacity(gameId, Capacity)) throw new GameAtCapacityException(gameId);
            if (IsPlayerNameInGame(gameId, playerName)) throw new PlayerAlreadyInGameException(gameId, playerName);

            var player = new Player(playerName, false);
            _repository.JoinGame(gameId, player);

            return player.PlayerId;
        }

        public GameState Start(GameId gameId, PlayerId playerId, Ruleset ruleset)
        {
            if (!_repository.DoesGameExist(gameId)) throw new GameDoesNotExistException(gameId);
            if (_repository.HasGameStarted(gameId)) throw new GameAlreadyInProgressException(gameId);

            var players = _repository.GetPlayers(gameId).ToList();
            var player = players.FirstOrDefault(p => p.PlayerId.Value == playerId.Value);
            if (player == null) throw new PlayerNotInGameException(gameId, playerId);

            if (!player.IsCreator) throw new UnexpectedPlayerException(gameId, playerId, "start");

            if (players.Count <= 1) throw new NotEnoughPlayersException(gameId);

            var gameRules = new GameRules(ruleset);

            var state = new GameState(gameId, gameRules, players, GameStatus.InProgress);

            _repository.StartGame(state);

            return state;
        }

        public GameState Poll(GameId gameId, PlayerId playerId)
        {
            if (!_repository.DoesGameExist(gameId)) throw new GameDoesNotExistException(gameId);
            if (!_repository.HasGameStarted(gameId)) throw new GameNotStartedException(gameId);

            var state = _repository.GetGameState(gameId);

            var player = state.Players.FirstOrDefault(p => p.PlayerId.Value == playerId.Value);
            if (player == null) throw new PlayerNotInGameException(gameId, playerId);

            return state;
        }

        public GameState Play(GameId gameId, PlayerId playerId, List<TilePlacement> placements)
        {
            if (!_repository.DoesGameExist(gameId)) throw new GameDoesNotExistException(gameId);
            if (!_repository.HasGameStarted(gameId)) throw new GameNotStartedException(gameId);

            var state = _repository.GetGameState(gameId);

            var player = state.Players.FirstOrDefault(p => p.PlayerId.Value == playerId.Value);
            if (player == null) throw new PlayerNotInGameException(gameId, playerId);

            if (state.Turn.CurrentPlayerId.Value != playerId.Value) throw new UnexpectedPlayerException(gameId, playerId, "play");

            var checker = new PlayChecker(player, state, placements, _repository);

            state = MakePlay(player, state, placements, checker.Score, checker.Words.ToList());

            if (player.Rack.Letters.Any())
            {
                state.Turn.NextTurn();
                _repository.SaveGameState(state, playerId);
            }
            else
            {
                FinaliseScores(state, playerId);
                state.SetFinished();
                _repository.SaveEndGameState(state);
            }

            return state;
        }

        internal Turn TryPlay(GameId gameId, PlayerId playerId, List<TilePlacement> placements)
        {
            if (!_repository.DoesGameExist(gameId)) throw new GameDoesNotExistException(gameId);
            if (!_repository.HasGameStarted(gameId)) throw new GameNotStartedException(gameId);

            var state = _repository.GetGameState(gameId);

            var player = state.Players.FirstOrDefault(p => p.PlayerId.Value == playerId.Value);
            if (player == null) throw new PlayerNotInGameException(gameId, playerId);

            if (state.Turn.CurrentPlayerId.Value != playerId.Value) throw new UnexpectedPlayerException(gameId, playerId, "play");

            var checker = new PlayChecker(player, state, placements, _repository);

            var thisTurn = new Turn(playerId, TurnAction.Play, checker.Score, checker.Words.ToList());

            return thisTurn;
        }

        public GameState Pass(GameId gameId, PlayerId playerId)
        {
            if (!_repository.DoesGameExist(gameId)) throw new GameDoesNotExistException(gameId);
            if (!_repository.HasGameStarted(gameId)) throw new GameNotStartedException(gameId);

            var state = _repository.GetGameState(gameId);

            var player = state.Players.FirstOrDefault(p => p.PlayerId.Value == playerId.Value);
            if (player == null) throw new PlayerNotInGameException(gameId, playerId);

            if (state.Turn.CurrentPlayerId.Value != playerId.Value) throw new UnexpectedPlayerException(gameId, playerId, "play");

            player.Pass();

            if (!state.Players.All(p => p?.LastTurn?.Action == TurnAction.Pass))
            {
                state.Turn.NextTurn();
                _repository.SaveGameState(state, playerId);
            }
            else
            {
                FinaliseScores(state);
                state.SetFinished();
                _repository.SaveEndGameState(state);
            }

            return state;
        }

        public GameState Swap(GameId gameId, PlayerId playerId, string letter)
        {
            if (!_repository.DoesGameExist(gameId)) throw new GameDoesNotExistException(gameId);
            if (!_repository.HasGameStarted(gameId)) throw new GameNotStartedException(gameId);

            var state = _repository.GetGameState(gameId);

            var player = state.Players.FirstOrDefault(p => p.PlayerId.Value == playerId.Value);
            if (player == null) throw new PlayerNotInGameException(gameId, playerId);
            if (state.Turn.CurrentPlayerId.Value != playerId.Value) throw new UnexpectedPlayerException(gameId, playerId, "play");

            if (state.TileBag.Count == 0) throw new NoMoreLettersException();

            if (!IsLetterOnRack(player.Rack, letter)) throw new ValidationException("You cannot exchange a letter not on your rack.");

            var newLetter = SwapLetter(player, state, letter);

            player.Swap();

            state.Turn.NextTurn();

            _repository.SaveGameState(state, null);

            return state;
        }

        public History GetHistory(GameId gameId)
        {
            if (!_repository.DoesGameExist(gameId)) throw new GameDoesNotExistException(gameId);
            var history = _repository.GetHistory(gameId);

            return history;
        }

        private static bool IsLetterOnRack(Rack rack, string letter)
        {
            return rack.Letters.Contains(letter);
        }

        private static GameState MakePlay(Player player, GameState state, List<TilePlacement> placements, int score, List<string> words)
        {
            foreach (var placement in placements)
            {
                state.Board.GetSquare(placement.Row, placement.Column).AddTile(placement.Letter);

                ReplaceTileInRack(player, state, placement.Letter);
            }

            player.IncreaseScore(score, words);

            return state;
        }

        private static void FinaliseScores(GameState state, PlayerId currentPlayerId)
        {
            foreach (var player in state.Players.Where(p=> p.PlayerId.Value != currentPlayerId.Value))
            {
                var subtractedScore = player.Rack.Letters.Count * state.GameRules.DebitPerRemainingTile;
                player.DecreaseScore(subtractedScore);
            }
        }

        private static void FinaliseScores(GameState state)
        {
            foreach (var player in state.Players)
            {
                var subtractedScore = player.Rack.Letters.Count * state.GameRules.DebitPerRemainingTile;
                player.DecreaseScore(subtractedScore);
            }
        }

        private static void ReplaceTileInRack(Player player, GameState state, string letter)
        {
            player.Rack.RemoveLetter(letter);
            var nextLetter = state.TileBag.GetNext();
            if (nextLetter != null)
            {
                player.Rack.AddLetter(nextLetter);
            }
        }

        private static string SwapLetter(Player player, GameState state, string letter)
        {
            player.Rack.RemoveLetter(letter);

            var nextLetter = state.TileBag.GetNext();
            player.Rack.AddLetter(nextLetter);

            state.TileBag.AddLetter(letter);

            return nextLetter;
        }

        public IEnumerable<Player> GetLoadingPlayers(GameId gameId, PlayerId playerId)
        {
            if (!_repository.DoesGameExist(gameId)) throw new GameDoesNotExistException(gameId);

            var players = _repository.GetPlayers(gameId);
            if (!players.Any(p => p.PlayerId.Value == playerId.Value)) throw new PlayerNotInGameException(gameId, playerId);

            return players;
        }

        public IEnumerable<Player> GetPlayers(GameId gameId)
        {
            if (!_repository.DoesGameExist(gameId)) throw new GameDoesNotExistException(gameId);

            return _repository.GetPlayers(gameId);
        }

        public bool IsPlayerNameInGame(GameId gameId, PlayerName playerName)
        {
            return _repository.GetPlayers(gameId).Any(p => p.PlayerName.Value == playerName.Value);
        }

        public bool HasGameStarted(GameId gameId)
        {
            if (!_repository.DoesGameExist(gameId)) throw new GameDoesNotExistException(gameId);

            return _repository.HasGameStarted(gameId);
        }
    }
}