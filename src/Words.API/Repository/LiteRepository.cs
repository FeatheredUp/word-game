using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using Words.API.DataModels;

namespace Words.API.Repository 
{
    public class LiteRepository : IRepository
    {
        private static string ConnectionString => new SQLiteConnectionStringBuilder(){ DataSource = new Uri(Path.Join(AppContext.BaseDirectory, "words.db")).LocalPath }.ConnectionString;
        private static readonly object _locker = new object();

        public bool DoesGameExist(GameId gameId)
        {
            if (gameId == null) throw new ArgumentNullException(nameof(gameId));

            using var con = new SQLiteConnection(ConnectionString);
            con.Open();

            const string sql = "SELECT COUNT(*) FROM game WHERE gameId = @gameId";
            using var cmd = new SQLiteCommand(sql, con);
            cmd.Parameters.AddWithValue("@gameId", gameId);
            var result = (long)cmd.ExecuteScalar();

            return result > 0;
        }

        public bool IsGameAtCapacity(GameId gameId, int capacity)
        {
            if (gameId == null) throw new ArgumentNullException(nameof(gameId));

            using var con = new SQLiteConnection(ConnectionString);
            con.Open();

            const string sql = "SELECT COUNT(*) FROM player WHERE gameId = @gameId";
            using var cmd = new SQLiteCommand(sql, con);
            cmd.Parameters.AddWithValue("@gameId", gameId);
            var result = (long)cmd.ExecuteScalar();

            return result >= capacity;
        }

        public GameState GetGameState(GameId gameId)
        {
            if (gameId == null) throw new ArgumentNullException(nameof(gameId));

            var players = GetPlayers(gameId).ToList();
            var playOrder = GetPlayerOrder(gameId);
            var rules = GetGameRules(gameId);
            var board = GetBoard(gameId, rules);

            using var con = new SQLiteConnection(ConnectionString);
            con.Open();

            const string sql = "SELECT bag, turnNumber, gameStatus FROM game WHERE gameId = @gameId";
            using var cmd = new SQLiteCommand(sql, con);
            cmd.Parameters.AddWithValue("@gameId", gameId);

            using SQLiteDataReader rdr = cmd.ExecuteReader();

            if (rdr.Read())
            {
                var bag = new TileBag(rdr.GetString(0));
                var turnNumber = rdr.GetInt16(1);
                var status = (GameStatus)rdr.GetInt16(2);
                var turn = new TurnOrder(playOrder, turnNumber);

                return new GameState(gameId, players, bag, turn, board, rules, status);
            }

            return null;
        }

        public IEnumerable<Player> GetPlayers(GameId gameId)
        {
            var players = GetBasicPlayers(gameId);

            using var con = new SQLiteConnection(ConnectionString);
            con.Open();

            const string sql = "SELECT turnAction, turnScore, words FROM turn WHERE gameId = @gameId AND playerId = @playerId ORDER BY turnNumber DESC LIMIT 1";

            foreach (var player in players)
            {
                using var cmd = new SQLiteCommand(sql, con);
                cmd.Parameters.AddWithValue("@gameId", gameId);
                cmd.Parameters.AddWithValue("@playerId", player.PlayerId);

                using SQLiteDataReader rdr = cmd.ExecuteReader();

                if (rdr.Read())
                {
                    var turnAction = (TurnAction)rdr.GetInt16(0);
                    var turnScore = rdr.GetInt16(1);
                    var words = rdr.GetString(2);

                    player.AddTurn(new Turn(player.PlayerId, turnAction, turnScore, words));
                }

                yield return player;
            }
        }

        public static IEnumerable<string> GetPlayerOrder(GameId gameId)
        {
            if (gameId == null) throw new ArgumentNullException(nameof(gameId));

            using var con = new SQLiteConnection(ConnectionString);
            con.Open();

            const string sql = "SELECT playerId FROM player WHERE gameId = @gameId ORDER BY playOrder";
            using var cmd = new SQLiteCommand(sql, con);
            cmd.Parameters.AddWithValue("@gameId", gameId);

            using SQLiteDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                yield return rdr.GetString(0);
            }
        }

        public bool HasGameStarted(GameId gameId)
        {
            if (gameId == null) throw new ArgumentNullException(nameof(gameId));

            using var con = new SQLiteConnection(ConnectionString);
            con.Open();

            const string sql = "SELECT gameStatus FROM game WHERE gameId = @gameId";
            using var cmd = new SQLiteCommand(sql, con);
            cmd.Parameters.AddWithValue("@gameId", gameId);

            var result = (GameStatus)cmd.ExecuteScalar();

            return result == GameStatus.InProgress || result == GameStatus.Completed;
        }

        public void SaveCreatedGame(CreatedGame createdGame)
        {
            if (createdGame == null) throw new ArgumentNullException(nameof(createdGame));

            RunInTransaction((con, trn) =>
            {
                CreateGame(createdGame, con, trn);
                CreatePlayer(createdGame.GameId, createdGame.Player, con, trn);
            });
        }

        public void JoinGame(GameId gameId, Player player)
        {
            if (gameId == null) throw new ArgumentNullException(nameof(gameId));
            if (player == null) throw new ArgumentNullException(nameof(player));

            const string sql = "INSERT INTO player(playerId, gameId, playerName, isCreator, score, rack, playOrder) " +
                "VALUES (@playerId, @gameId, @playerName, 0, 0, '', 0)";

            RunInTransaction((con, trn) =>
            {
                using var cmd = new SQLiteCommand(sql, con, trn);
                cmd.Parameters.AddWithValue("@gameId", gameId);
                cmd.Parameters.AddWithValue("@playerId", player.PlayerId);
                cmd.Parameters.AddWithValue("@playerName", player.PlayerName);

                cmd.ExecuteNonQuery();

            });
        }

        public void StartGame(GameState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));

            RunInTransaction((con, trn) =>
            {
                // Save each player
                foreach (var player in state.Players)
                {
                    var playOrder = state.Turn.TurnsToWait(player.PlayerId);
                    UpdateInitialPlayer(player, playOrder, con, trn);
                }

                SaveGameRules(state.GameId, state.GameRules, con, trn);
                UpdateGame(state.GameId, state.TileBag, state.Turn.TurnNumber, GameStatus.InProgress, con, trn);
            });
        }

        public void SaveGameState(GameState state, PlayerId playerId)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (playerId == null) throw new ArgumentNullException(nameof(playerId));

            var players = state.Players.Where(p => p.PlayerId.Value == playerId.Value).ToList();

            RunInTransaction((con, trn) =>
            {
                foreach (var player in players)
                {
                    UpdatePlayer(player, con, trn);
                    CreateTurn(state.GameId, player, state.Turn.TurnNumber - 1, con, trn);
                }
                UpdateGame(state.GameId, state.TileBag, state.Turn.TurnNumber, state.GameStatus, con, trn);
                UpdateBoard(state.GameId, state.Board, con, trn);
            });
        }

        public void SaveEndGameState(GameState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));

            RunInTransaction((con, trn) =>
            {
                foreach (var player in state.Players)
                {
                    UpdatePlayer(player, con, trn);
                    CreateTurn(state.GameId, player, state.Turn.TurnNumber, con, trn);
                }
                UpdateGame(state.GameId, state.TileBag, state.Turn.TurnNumber, state.GameStatus, con, trn);
                UpdateBoard(state.GameId, state.Board, con, trn);
            });
        }

        public List<string> WordsNotInDictionary(IReadOnlyList<string> words)
        {
            if (words == null) throw new ArgumentNullException(nameof(words));

            var result = new List<string>();
            var dictionary = File.ReadAllLines(Path.Join(AppContext.BaseDirectory, "dictionary.txt"));

            foreach (var word in words)
            {
                if (!dictionary.Contains(word.ToLower())) result.Add(word);
            }

            return result;
        }

        public History GetHistory(GameId gameId)
        {
            if (gameId == null) throw new ArgumentNullException(nameof(gameId));

            var turns = GetTurns(gameId);

            using var con = new SQLiteConnection(ConnectionString);
            con.Open();

            const string sql = "SELECT gameStatus, bag FROM game WHERE gameId = @gameId";
            using var cmd = new SQLiteCommand(sql, con);
            cmd.Parameters.AddWithValue("@gameId", gameId);
            SQLiteDataReader rdr = cmd.ExecuteReader();
            if (rdr.Read())
            {
                var status = (GameStatus)rdr.GetInt16(0);
                var bag = new TileBag(rdr.GetString(1));

                return new History(status, turns, bag.Count);
            }

            return null;
        }

        public object SyncLock
        {
            get
            {
                return _locker;
            }
        }

        private static List<Turn> GetTurns(GameId gameId)
        {
            var turns = new List<Turn>();
            using var con = new SQLiteConnection(ConnectionString);
            con.Open();

            const string sql = "SELECT playerId, turnAction, turnScore, words FROM turn WHERE gameId = @gameId ORDER BY turnNumber";
            using var cmd = new SQLiteCommand(sql, con);
      
            cmd.Parameters.AddWithValue("@gameId", gameId);
            SQLiteDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                var playerId = rdr.GetString(0);
                var turnAction = (TurnAction)rdr.GetInt16(1);
                var turnScore = rdr.GetInt16(2);
                var words = rdr.GetString(3);

                turns.Add(new Turn(new PlayerId(playerId), turnAction, turnScore, words));
            }

            return turns;
        }

        private static IEnumerable<Player> GetBasicPlayers(GameId gameId)
        {
            if (gameId == null) throw new ArgumentNullException(nameof(gameId));

            using var con = new SQLiteConnection(ConnectionString);
            con.Open();

            const string sql = "SELECT playerId, playerName, isCreator, score, rack FROM player WHERE gameId = @gameId";
            using var cmd = new SQLiteCommand(sql, con);
            cmd.Parameters.AddWithValue("@gameId", gameId);

            using SQLiteDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                var playerId = rdr.GetString(0);
                var playerName = rdr.GetString(1);
                var isCreator = rdr.GetBoolean(2);
                var score = rdr.GetInt32(3);
                var rack = rdr.GetString(4);

                yield return new Player(playerId, playerName, isCreator, score, rack);
            }
        }

        private static Board GetBoard(GameId gameId, GameRules gameRules)
        {
            using var con = new SQLiteConnection(ConnectionString);
            con.Open();

            const string sql = "SELECT row, column, letter, height, squareType " +
                "FROM boardSquare WHERE gameId = @gameId";
            using var cmd = new SQLiteCommand(sql, con);
            cmd.Parameters.AddWithValue("@gameId", gameId);

            using SQLiteDataReader rdr = cmd.ExecuteReader();

            var board = new Board(gameRules);

            while (rdr.Read())
            {
                var row = rdr.GetInt16(0);
                var column = rdr.GetInt16(1);
                var letter = rdr.GetString(2);
                var height = rdr.GetInt16(3);
                var squareType = (SquareType)rdr.GetInt16(4);

                board.GetSquare(row, column).SetTile(letter, height, squareType);
            }

            return board;
        }

        private static GameRules GetGameRules(GameId gameId)
        {
            using var con = new SQLiteConnection(ConnectionString);
            con.Open();

            const string sql = "SELECT ruleset " +
                "FROM game WHERE gameId = @gameId";
            using var cmd = new SQLiteCommand(sql, con);
            cmd.Parameters.AddWithValue("@gameId", gameId);

            using SQLiteDataReader rdr = cmd.ExecuteReader();

            if (rdr.Read())
            {
                var rulesetName = rdr.GetString(0);
                return new GameRules(rulesetName);
            }

            return null;
        }

        private static void CreateGame(CreatedGame createdGame, SQLiteConnection con, SQLiteTransaction trn)
        {
            const string sql = "INSERT INTO game(gameId, gameStatus, date, turnNumber, bag) " +
                "VALUES (@gameId, @gameStatus, datetime('now'), 0, '')";
            using var cmd = new SQLiteCommand(sql, con, trn);
            cmd.Parameters.AddWithValue("@gameId", createdGame.GameId);
            cmd.Parameters.AddWithValue("@gameStatus", GameStatus.BeingFormed);

            cmd.ExecuteNonQuery();
        }

        private static void CreatePlayer(GameId gameId, Player player, SQLiteConnection con, SQLiteTransaction trn)
        {
            const string sql = "INSERT INTO player(playerId, gameId, playerName, IsCreator, score, rack, playOrder) " +
                "VALUES (@playerId, @gameId, @playerName, @isCreator, 0, '', 0)";
            using var cmd = new SQLiteCommand(sql, con, trn);
            cmd.Parameters.AddWithValue("@playerId", player.PlayerId);
            cmd.Parameters.AddWithValue("@gameId", gameId);
            cmd.Parameters.AddWithValue("@playerName", player.PlayerName);
            cmd.Parameters.AddWithValue("@isCreator", player.IsCreator ? 1 : 0);

            cmd.ExecuteNonQuery();
        }

        private static void UpdateInitialPlayer(Player player, int playOrder, SQLiteConnection con, SQLiteTransaction trn)
        {
            const string sql = "UPDATE player set score = 0, rack = @rack, playOrder = @playOrder " +
                "WHERE playerId = @playerId";
            using var cmd = new SQLiteCommand(sql, con, trn);
            cmd.Parameters.AddWithValue("@playerId", player.PlayerId);
            cmd.Parameters.AddWithValue("@rack", string.Join(',', player.Rack.Letters));
            cmd.Parameters.AddWithValue("@playOrder", playOrder);

            cmd.ExecuteNonQuery();
        }

        private static void UpdatePlayer(Player player, SQLiteConnection con, SQLiteTransaction trn)
        {
            const string sql = "UPDATE player set score = @score, rack = @rack " +
                "WHERE playerId = @playerId";
            using var cmd = new SQLiteCommand(sql, con, trn);
            cmd.Parameters.AddWithValue("@playerId", player.PlayerId);
            cmd.Parameters.AddWithValue("@score", player.Score);
            cmd.Parameters.AddWithValue("@rack", string.Join(',', player.Rack.Letters));

            cmd.ExecuteNonQuery();
        }

        private static void CreateTurn(GameId gameId, Player player, int turnNumber, SQLiteConnection con, SQLiteTransaction trn)
        {
            const string sql = "REPLACE INTO turn (gameId, turnNumber, playerId, turnAction, turnScore, words) " +
                  "VALUES(@gameId, @turnNumber, @playerId, @turnAction, @turnScore, @words)";

            using var cmd = new SQLiteCommand(sql, con, trn);
            cmd.Parameters.AddWithValue("@gameId", gameId.Value);
            cmd.Parameters.AddWithValue("@turnNumber", turnNumber);
            cmd.Parameters.AddWithValue("@playerId", player.PlayerId.Value);
            cmd.Parameters.AddWithValue("@turnAction", player.LastTurn.Action);
            cmd.Parameters.AddWithValue("@turnScore", player.LastTurn.Score);
            if (player.LastTurn.Words == null)
            {
                cmd.Parameters.AddWithValue("@words", "");
            }
            else
            {
                cmd.Parameters.AddWithValue("@words", string.Join(",", player.LastTurn.Words));
            }

            cmd.ExecuteNonQuery();
        }

        private static void SaveGameRules(GameId gameId, GameRules gameRules, SQLiteConnection con, SQLiteTransaction trn)
        {
            const string sql = "UPDATE game set ruleset = @ruleset WHERE gameId = @gameId";
            using var cmd = new SQLiteCommand(sql, con, trn);
            cmd.Parameters.AddWithValue("@gameId", gameId);
            cmd.Parameters.AddWithValue("@ruleset", gameRules.RuleSet.ToString());

            cmd.ExecuteNonQuery();
        }

        private static void UpdateGame(GameId gameId, TileBag tileBag, int turnNumber, GameStatus status, SQLiteConnection con, SQLiteTransaction trn)
        {
            const string sql = "UPDATE game set bag = @bag, turnNumber = @turnNumber, gameStatus = @gameStatus "
                + "WHERE gameId = @gameId";
            using var cmd = new SQLiteCommand(sql, con, trn);
            cmd.Parameters.AddWithValue("@gameId", gameId);
            cmd.Parameters.AddWithValue("@bag", string.Join(',', tileBag.Letters));
            cmd.Parameters.AddWithValue("@turnNumber", turnNumber);
            cmd.Parameters.AddWithValue("@gameStatus", status);

            cmd.ExecuteNonQuery();
        }

        private static void UpdateBoard(GameId gameId, Board board, SQLiteConnection con, SQLiteTransaction trn)
        {
            const string sql = "REPLACE INTO boardSquare (gameId, row, column, letter, height, squareType) " +
                "VALUES(@gameId, @row, @column, @letter, @height, @squareType)";

            for (int row = 1; row <= GameRules.MaxRows; row++)
            {
                for (int column = 1; column <= GameRules.MaxColumns; column++)
                {
                    var square = board.GetSquare(row, column);
                    using var cmd = new SQLiteCommand(sql, con, trn);
                    cmd.Parameters.AddWithValue("@gameId", gameId);
                    cmd.Parameters.AddWithValue("@row", row);
                    cmd.Parameters.AddWithValue("@column", column);
                    cmd.Parameters.AddWithValue("@letter", square.Letter);
                    cmd.Parameters.AddWithValue("@height", square.Height);
                    cmd.Parameters.AddWithValue("@squareType", square.SquareType);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static void RunInTransaction(Action<SQLiteConnection, SQLiteTransaction> doInTransaction)
        {
            SQLiteConnection connection = null;
            SQLiteTransaction transaction = null;

            try
            {
                connection = new SQLiteConnection(ConnectionString);
                connection.Open();
                transaction = connection.BeginTransaction();
                doInTransaction(connection, transaction);
                transaction.Commit();
            }
            catch
            {
                if (transaction != null)
                {
                    transaction.Rollback();
                }
                throw;
            }
            finally
            {
                if (transaction != null)
                {
                    transaction.Dispose();
                }
                if (connection != null)
                {
                    connection.Dispose();
                }
            }
        }
    }
}
