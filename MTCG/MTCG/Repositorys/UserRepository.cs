﻿using MTCG.Database;
using MTCG.Model;
using Npgsql;

namespace MTCG.Repositorys
{
    public static class UserRepository
    {
        public static void CreateUser(User user)
        {
            string query = "INSERT INTO Users (Username, Coins, Elo, Password) VALUES (@username, @coins, @elo, @password)";

            var parameters = new NpgsqlParameter[]
            {
                new NpgsqlParameter("@username", user.Username),
                new NpgsqlParameter("@coins", user.Coins),
                new NpgsqlParameter("@elo", user.Elo),
                new NpgsqlParameter("@password", user.Password)
            };

            using(DataHandler dbh = new DataHandler())
            {
                dbh.ExecuteNonQuery(query, parameters);
            }             
        }

        public static void UpdateUser(User user)
        {
            string query = "UPDATE Users SET Coins = @coins, Elo = @elo, Password = @password, Bio = @bio, Image = @image WHERE Username = @username";

            var parameters = new NpgsqlParameter[]
            {
                new NpgsqlParameter("@coins", user.Coins),
                new NpgsqlParameter("@elo", user.Elo),
                new NpgsqlParameter("@password", user.Password),
                new NpgsqlParameter("@username", user.Username),
                new NpgsqlParameter("@bio", (object)user.Bio ?? DBNull.Value),
                new NpgsqlParameter("@image", (object)user.Image ?? DBNull.Value)
            };

            using (DataHandler dbh = new DataHandler())
            {
                dbh.ExecuteNonQuery(query, parameters);
            }
        }
        public static User GetUserByUsername(string username)
        {
            string query = "SELECT * FROM Users WHERE Username = @username";

            var parameter = new NpgsqlParameter("@username", username);
            using (DataHandler dbh = new DataHandler())
            using (var reader = dbh.ExecuteSelectQuery(query, new NpgsqlParameter[] { parameter }))
            {
                if (reader != null && reader.Read())
                {
                    User tmp = new User(
                        username: reader.GetString(reader.GetOrdinal("Username")),
                        password: reader.GetString(reader.GetOrdinal("Password"))
                    );
                    tmp.UserID = reader.GetInt32(reader.GetOrdinal("UsersID"));
                    tmp.Stack = StackRepository.GetUserStack(tmp.Username);
                    tmp.Deck = DeckRepository.GetUserDeck(tmp.Username);
                    tmp.Coins = reader.GetInt32(reader.GetOrdinal("Coins"));
                    tmp.Elo = reader.GetInt32(reader.GetOrdinal("Elo"));
                    return tmp;
                }
            }

            return null;
        }

        public static User GetUserInfoByUsername(string username)
        {
            string query = "SELECT Username, Password, Bio, Image FROM Users WHERE Username = @username";

            var parameter = new NpgsqlParameter("@username", username);
            using (DataHandler dbh = new DataHandler())
            using (var reader = dbh.ExecuteSelectQuery(query, new NpgsqlParameter[] { parameter }))
            {
                if (reader != null && reader.Read())
                {
                    User tmp = new User(
                        username: reader.GetString(reader.GetOrdinal("Username")),
                        password: reader.GetString(reader.GetOrdinal("Password"))
                    );

                    int bioOrdinal = reader.GetOrdinal("Bio");
                    int imageOrdinal = reader.GetOrdinal("Image");

                    tmp.Bio = reader.IsDBNull(bioOrdinal) ? null : reader.GetString(bioOrdinal);
                    tmp.Image = reader.IsDBNull(imageOrdinal) ? null : reader.GetString(imageOrdinal);

                    return tmp;
                }
            }

            return null;
        }

        public static User GetRandomOpponent(User currentUser)
        {
            string query = "SELECT * FROM Users WHERE Username <> @username AND UsersID IN (SELECT UserID FROM Decks) ORDER BY ABS(Elo - @elo) ASC LIMIT 1";

            var parameters = new NpgsqlParameter[]
            {
                new NpgsqlParameter("@username", currentUser.Username),
                new NpgsqlParameter("@elo", currentUser.Elo)
            };

            using (DataHandler dbh = new DataHandler())
            using (var reader = dbh.ExecuteSelectQuery(query, parameters))
            {
                if (reader != null && reader.Read())
                {
                    User opponent = new User(
                        username: reader.GetString(reader.GetOrdinal("Username")),
                        password: reader.GetString(reader.GetOrdinal("Password"))
                    );

                    opponent.UserID = reader.GetInt32(reader.GetOrdinal("UsersID"));
                    opponent.Stack = StackRepository.GetUserStack(opponent.Username);
                    opponent.Deck = DeckRepository.GetUserDeck(opponent.Username);
                    opponent.Coins = reader.GetInt32(reader.GetOrdinal("Coins"));
                    opponent.Elo = reader.GetInt32(reader.GetOrdinal("Elo"));

                    return opponent;
                }
            }

            return null;
        }   
    }
}
