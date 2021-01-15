using System.Collections.Generic;
using Words.API.DataModels;

namespace Words.API.Repository
{
    public interface IRepository
    {
        void SaveCreatedGame(CreatedGame createdGame);
        void JoinGame(GameId gameId, Player player);
        bool DoesGameExist(GameId gameId);
        IEnumerable<Player> GetPlayers(GameId gameId);
        void StartGame(GameState state);
        void SaveGameState(GameState state, PlayerId playerId);
        void SaveEndGameState(GameState state);
        GameState GetGameState(GameId gameId);
        bool HasGameStarted(GameId gameId);
        List<string> WordsNotInDictionary(IReadOnlyList<string> words);
        History GetHistory(GameId gameId);
        bool IsGameAtCapacity(GameId gameId, int capacity);
        object SyncLock { get; }
    }
}
