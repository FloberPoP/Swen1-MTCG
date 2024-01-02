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
        public static int GetTotalGames(string username)
        {
            string query = "SELECT COUNT(*) as TotalGames " +
                           "FROM BattleLogs WHERE Winner = @username OR Looser = @username";

            var parameter = new NpgsqlParameter("@username", username);

            using (var reader = dataHandler.ExecuteSelectQuery(query, new NpgsqlParameter[] { parameter }))
            {
                if (reader != null && reader.Read())
                {
                    return Convert.ToInt32(reader["TotalGames"]);
                }
            }

            return 0;
        }

        public static int GetGamesWon(string username)
        {
            string query = "SELECT COUNT(*) as GamesWon " +
                           "FROM BattleLogs WHERE Winner = @username";

            var parameter = new NpgsqlParameter("@username", username);

            using (var reader = dataHandler.ExecuteSelectQuery(query, new NpgsqlParameter[] { parameter }))
            {
                if (reader != null && reader.Read())
                {
                    return Convert.ToInt32(reader["GamesWon"]);
                }
            }

            return 0;
        }

        public static int GetGamesLost(string username)
        {
            string query = "SELECT COUNT(*) as GamesLost " +
                           "FROM BattleLogs WHERE Looser = @username";

            var parameter = new NpgsqlParameter("@username", username);

            using (var reader = dataHandler.ExecuteSelectQuery(query, new NpgsqlParameter[] { parameter }))
            {
                if (reader != null && reader.Read())
                {
                    return Convert.ToInt32(reader["GamesLost"]);
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

            object result = dataHandler.ExecuteSelectQuery(query, new NpgsqlParameter[] { parameter });
            return result != null ? Convert.ToInt32(result) : 0;
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
    }
}
