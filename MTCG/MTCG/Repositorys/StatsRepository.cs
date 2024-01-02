using MTCG.Battling;
using MTCG.Database;
using MTCG.Model;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Repositorys
{
    internal static class StatsRepository
    {
        private static readonly DataHandler? dataHandler = new DataHandler();
        public static int GetTotalGames(int userID)
        {
            string query = "SELECT COUNT(*) as TotalGames " +
                           "FROM BattleLogs WHERE WinnerID = @userID OR LooserID = @userID";

            var parameter = new NpgsqlParameter("@userID", userID);

            using (var reader = dataHandler.ExecuteSelectQuery(query, new NpgsqlParameter[] { parameter }))
            {
                if (reader != null && reader.Read())
                {
                    return Convert.ToInt32(reader["TotalGames"]);
                }
            }

            return 0;
        }

        public static int GetGamesWon(int userID)
        {
            string query = "SELECT COUNT(*) as GamesWon " +
                           "FROM BattleLogs WHERE WinnerID = @userID AND Draw = false";

            var parameter = new NpgsqlParameter("@userID", userID);

            using (var reader = dataHandler.ExecuteSelectQuery(query, new NpgsqlParameter[] { parameter }))
            {
                if (reader != null && reader.Read())
                {
                    return Convert.ToInt32(reader["GamesWon"]);
                }
            }

            return 0;
        }

        public static int GetGamesLost(int userID)
        {
            string query = "SELECT COUNT(*) as GamesLost " +
                           "FROM BattleLogs WHERE LooserID = @userID AND Draw = false";

            var parameter = new NpgsqlParameter("@userID", userID);

            using (var reader = dataHandler.ExecuteSelectQuery(query, new NpgsqlParameter[] { parameter }))
            {
                if (reader != null && reader.Read())
                {
                    return Convert.ToInt32(reader["GamesLost"]);
                }
            }

            return 0;
        }

        public static int GetGamesDrawn(int userID)
        {
            string query = "SELECT COUNT(*) as GamesDrawn " +
                           "FROM BattleLogs WHERE Draw = true AND (WinnerID = @userID OR LooserID = @userID)";

            var parameter = new NpgsqlParameter("@userID", userID);

            using (var reader = dataHandler.ExecuteSelectQuery(query, new NpgsqlParameter[] { parameter }))
            {
                if (reader != null && reader.Read())
                {
                    return Convert.ToInt32(reader["GamesDrawn"]);
                }
            }

            return 0;
        }


        public static int GetTotalSpentCoins(string username)
        {
            string query = "SELECT SUM(p.Price) " +
                           "FROM Purchases pur " +
                           "JOIN Packages p ON pur.PackagesID = p.PackagesID " +
                           "JOIN Users u ON pur.UsersID = u.UsersID " +
                           "WHERE u.Username = @username";

            var parameter = new NpgsqlParameter("@username", username);

            using (NpgsqlDataReader reader = (NpgsqlDataReader)dataHandler.ExecuteSelectQuery(query, new NpgsqlParameter[] { parameter }))
            {
                if (reader.Read())
                {
                    return reader.IsDBNull(0) ? 0 : Convert.ToInt32(reader[0]);
                }
            }
            return 0;
        }


        public static List<UserScoreboardEntry> GetScoreboard()
        {
            string query = "SELECT Username, Elo FROM Users ORDER BY Elo DESC";

            List<UserScoreboardEntry> scoreboard = new List<UserScoreboardEntry>();

            using (var reader = dataHandler.ExecuteSelectQuery(query, null))
            {
                while (reader != null && reader.Read())
                {
                    string username = reader.GetString(reader.GetOrdinal("Username"));
                    int elo = reader.GetInt32(reader.GetOrdinal("Elo"));

                    scoreboard.Add(new UserScoreboardEntry
                    {
                        Username = username,
                        Elo = elo
                    });
                }
            }

            return scoreboard;
        }

        public static void InsertBattleLog(BattleLog log)
        {
            string query = "INSERT INTO BattleLogs (Rounds, LooserID, WinnerID, Draw) " +
                           "VALUES (@rounds, @looserid, @winnerid, @draw)";

            var parameters = new NpgsqlParameter[]
            {
                new NpgsqlParameter("@rounds", log.Rounds),
                new NpgsqlParameter("@looserid", log.LooserID),
                new NpgsqlParameter("@winnerid", log.WinnerID),
                new NpgsqlParameter("@draw", log.Draw)
            };

            dataHandler.ExecuteNonQuery(query, parameters);
        }
    }
}
