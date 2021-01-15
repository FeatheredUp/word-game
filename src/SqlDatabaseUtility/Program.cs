using System;
using System.Data.SQLite;
using System.IO;

namespace SqlDatabaseUtility
{
    class Program
    {
        static void Main(string[] args)
        {
            var fileName = Path.Join(AppContext.BaseDirectory, "words.db");
            var connectionString = $"URI={fileName}";

            using var con = new SQLiteConnection(connectionString);
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.CommandText = "DROP TABLE IF EXISTS game";
            cmd.ExecuteNonQuery();

            cmd.CommandText = "CREATE TABLE game(id TEXT PRIMARY KEY, state INT)";
            cmd.ExecuteNonQuery();

            cmd.CommandText = "INSERT INTO game(id, state) VALUES ('ABCD', 0)";
            cmd.ExecuteNonQuery();

            cmd.CommandText = "INSERT INTO game(id, state) VALUES ('PEGH', 1)";
            cmd.ExecuteNonQuery();


            string stm = "SELECT * FROM game LIMIT 5";

            using var cmdRead = new SQLiteCommand(stm, con);
            using SQLiteDataReader rdr = cmdRead.ExecuteReader();

            while (rdr.Read())
            {
                Console.WriteLine($"{rdr.GetString(0)} {rdr.GetInt32(1)}");
            }
        }
    }
}
