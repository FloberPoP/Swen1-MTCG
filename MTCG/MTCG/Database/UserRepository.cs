using System.Collections.Generic;
using MTCG.Cards;
using MTCG.Database;
using Newtonsoft.Json;
using Npgsql;
using NpgsqlTypes;

namespace MTCG.Users
{
    internal static class UserRepository
    {
        private static readonly DataHandler dataHandler = new("localhost", "5432", "mtcgdb", "postgres", "debian123");

        public static void CreateUser(User user)
        {
            string query = "INSERT INTO Users (Username, StackID, DeckID, Coins, Elo, BattleCount, Password) " +
                           "VALUES (@username, @stackID, @deckID, @coins, @elo, @battleCount, @password)";

            var parameters = new NpgsqlParameter[]
            {
                new NpgsqlParameter("@username", user.Username),
                new NpgsqlParameter("@stackID", DBNull.Value), 
                new NpgsqlParameter("@deckID",  DBNull.Value), 
                new NpgsqlParameter("@coins", user.Coins),
                new NpgsqlParameter("@elo", user.Elo),
                new NpgsqlParameter("@battleCount", user.BattleCount),
                new NpgsqlParameter("@password", user.Password)
            };

            dataHandler.ExecuteNonQuery(query, parameters);
        }

        public static void UpdateUser(User user)
        {
            string query = "UPDATE Users SET Stack = @stack, Deck = @deck, Coins = @coins, Elo = @elo, BattleCount = @battleCount, Password = @password WHERE Username = @username";

            var parameters = new NpgsqlParameter[]
            {
                new NpgsqlParameter("@stack", NpgsqlDbType.Jsonb) { Value = JsonConvert.SerializeObject(user.Stack) },
                new NpgsqlParameter("@deck",  NpgsqlDbType.Jsonb) { Value = JsonConvert.SerializeObject(user.Deck) },
                new NpgsqlParameter("@coins", user.Coins),
                new NpgsqlParameter("@elo", user.Elo),
                new NpgsqlParameter("@battleCount", user.BattleCount),
                new NpgsqlParameter("@password", user.Password),
                new NpgsqlParameter("@username", user.Username)
            };

            dataHandler.ExecuteNonQuery(query, parameters);
        }

        public static User GetUserByUsername(string username)
        {
            string query = "SELECT * FROM Users WHERE Username = @username";

            var parameter = new NpgsqlParameter("@username", username);

            using (var reader = dataHandler.ExecuteSelectQuery(query, new NpgsqlParameter[] { parameter }))
            {
                if (reader != null && reader.Read())
                {
                    User tmp = new User(
                        username: reader.GetString(reader.GetOrdinal("Username")),
                        password: reader.GetString(reader.GetOrdinal("Password"))
                        );

                    tmp.Stack = null;// TODO !!!!
                    tmp.Deck = null; // TODO !!!!
                    tmp.Coins = reader.IsDBNull(reader.GetOrdinal("Coins")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("Coins"));
                    tmp.BattleCount = reader.IsDBNull(reader.GetOrdinal("BattleCount")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("BattleCount"));
                    tmp.Elo = reader.IsDBNull(reader.GetOrdinal("Elo")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("Elo"));                
                    return tmp;
                }
            }

            return null; // User not found
        }


        public static bool ValidateUserCredentials(string username, string password)
        {
            User user = GetUserByUsername(username);    
            if (user.Username == username && password == user.Password)
            {
                return true;
            }
            return false;
        }
    }
}
