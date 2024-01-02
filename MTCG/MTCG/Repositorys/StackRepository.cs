using MTCG.Database;
using MTCG.Model;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Repositorys
{
    internal static class StackRepository
    {
        private static readonly DataHandler? dataHandler = new DataHandler();
        public static void AddCardToUserStack(int userId, int cardId)
        {
            string query = "INSERT INTO Stacks (UserID, CardID, Trading) VALUES (@userId, @cardId, false)";

            var parameters = new NpgsqlParameter[]
            {
                new NpgsqlParameter("@userId", userId),
                new NpgsqlParameter("@cardId", cardId)
            };

            dataHandler.ExecuteNonQuery(query, parameters);
        }

        public static List<Card> GetUserStack(string username)
        {
            string query = "SELECT c.* FROM Cards c " +
                           "JOIN Stacks s ON c.CardsID = s.CardID " +
                           "JOIN Users u ON s.UserID = u.UsersID " +
                           "WHERE u.Username = @username AND s.Trading = false";

            var parameter = new NpgsqlParameter("@username", username);

            List<Card> userStack = new List<Card>();

            using (var reader = dataHandler.ExecuteSelectQuery(query, new NpgsqlParameter[] { parameter }))
            {
                while (reader != null && reader.Read())
                {
                    Card card = new Card(
                        reader.GetString(reader.GetOrdinal("Name")),
                        reader.GetInt32(reader.GetOrdinal("Damage")),
                        Enum.Parse<ERegions>(reader.GetString(reader.GetOrdinal("Region"))),
                        Enum.Parse<EType>(reader.GetString(reader.GetOrdinal("Type")))
                        );
                    card.CardsID = reader.GetInt32(reader.GetOrdinal("CardsID"));

                    userStack.Add(card);
                }
            }

            return userStack;
        }
        public static bool AreCardsInUserStack(int userId, List<int> cardIds)
        {
            string query = "SELECT COUNT(*) FROM Stacks " +
                           "WHERE UserID = @userId " +
                           "AND CardID = ANY(@cardIds) " +
                           "AND Trading = false";

            var parameters = new NpgsqlParameter[]
            {
                new NpgsqlParameter("@userId", userId),
                new NpgsqlParameter("@cardIds", NpgsqlDbType.Array | NpgsqlDbType.Integer)
                {
                    Value = cardIds.ToArray()
                }
            };

            int count = Convert.ToInt32(dataHandler.ExecuteSelectQuery(query, parameters));

            return count == cardIds.Count;
        }
    }
}
