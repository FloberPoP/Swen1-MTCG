using MTCG.Database;
using MTCG.Model;
using Npgsql;

namespace MTCG.Repositorys
{
    public static class DeckRepository
    {
        public static List<Card> GetUserDeck(string username)
        {
            string query = "SELECT c.* FROM Cards c JOIN Decks d ON c.CardsID = d.CardID JOIN Users u ON d.UserID = u.UsersID WHERE u.Username = @username";

            var parameter = new NpgsqlParameter("@username", username);

            List<Card> userDeck = new List<Card>();

            using (DataHandler dbh = new DataHandler())
            using (var reader = dbh.ExecuteSelectQuery(query, new NpgsqlParameter[] { parameter }))
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
                    userDeck.Add(card);
                }
            }

            return userDeck;
        }
        public static void ClearUserDeck(int userId)
        {
            string query = "DELETE FROM Decks WHERE UserID = @userId";

            var parameter = new NpgsqlParameter("@userId", userId);

            using (DataHandler dbh = new DataHandler())
            {
                dbh.ExecuteNonQuery(query, new NpgsqlParameter[] { parameter });
            }           
        }
        public static void AddCardToUserDeck(int userId, List<int> cardIDs)
        {
            string query = "INSERT INTO Decks (UserID, CardID) VALUES (@userId, @cardId)";

            foreach (int cardId in cardIDs)
            {
                var parameters = new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@userId", userId),
                    new NpgsqlParameter("@cardId", cardId)
                };

                using (DataHandler dbh = new DataHandler())
                {
                    dbh.ExecuteNonQuery(query, parameters);
                }
            }
        }

        public static bool IsCardInUserDeck(int userId, int cardId)
        {
            string query = "SELECT 1 FROM Decks WHERE UserID = @userId AND CardID = @cardId";

            var parameters = new NpgsqlParameter[]
            {
                new NpgsqlParameter("@userId", userId),
                new NpgsqlParameter("@cardId", cardId)
            };

            using (DataHandler dbh = new DataHandler())
            using (var reader = dbh.ExecuteSelectQuery(query, parameters))
            {
                return reader != null && reader.HasRows;
            }
        }
    }
}
