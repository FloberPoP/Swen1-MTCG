using System.Collections.Generic;
using MTCG.Cards;
using MTCG.Database;
using Npgsql;

namespace MTCG.Users
{
    internal class UserRepository
    {
        private readonly DataHandler dataHandler;

        public UserRepository(DataHandler dataHandler)
        {
            this.dataHandler = dataHandler;
        }

        public void UpdateUser(User user)
        {
            string query = "UPDATE Users SET Stack = @stack, Deck = @deck, Coins = @coins, Elo = @elo, BattleCount = @battleCount, Password = @password WHERE Username = @username";

            var parameters = new NpgsqlParameter[]
            {
                new NpgsqlParameter("@stack", user.Stack),
                new NpgsqlParameter("@deck", user.Deck),
                new NpgsqlParameter("@coins", user.Coins),
                new NpgsqlParameter("@elo", user.Elo),
                new NpgsqlParameter("@battleCount", user.BattleCount),
                new NpgsqlParameter("@password", user.Password),
                new NpgsqlParameter("@username", user.Username)
            };

            dataHandler.ExecuteNonQuery(query, parameters);
        }

        public User SelectUserByUsername(string username)
        {
            string query = "SELECT * FROM Users WHERE Username = @username";

            var parameter = new NpgsqlParameter("@username", username);

            using (var reader = dataHandler.ExecuteSelectQuery(query, new NpgsqlParameter[] { parameter }))
            {
                if (reader != null && reader.Read())
                {
                    return new User(
                        stack: null, // TODO !!!!
                        deck: null, // TODO !!!!
                        coins: reader.IsDBNull(reader.GetOrdinal("Coins")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("Coins")),
                        elo: reader.IsDBNull(reader.GetOrdinal("Elo")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("Elo")),
                        battleCount: reader.IsDBNull(reader.GetOrdinal("BattleCount")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("BattleCount")),
                        username: reader.GetString(reader.GetOrdinal("Username")),
                        password: reader.GetString(reader.GetOrdinal("Password"))
                    );
                }
            }

            return null; // User not found
        }
    }
}
