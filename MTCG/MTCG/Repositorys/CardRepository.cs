using MTCG.Database;
using MTCG.Model;
using Npgsql;

namespace MTCG.Repositorys
{
    internal static class CardRepository
    {
        public static readonly DataHandler? dataHandler = new DataHandler();
        public static void CreateCard(Card c)
        {

            string query = "INSERT INTO Cards (CardsID, Name, Damage, Region, Type)" +
                           "VALUES (@cardsid, @name, @damage, @region, @type)";

            var parameters = new NpgsqlParameter[]
            {
                new NpgsqlParameter("@cardsid", c.CardsID),
                new NpgsqlParameter("@name", c.Name),
                new NpgsqlParameter("@damage", c.Damage),
                new NpgsqlParameter("@region", c.Region.ToString()),
                new NpgsqlParameter("@type", c.Type.ToString())
            };

            dataHandler.ExecuteNonQuery(query, parameters);
        }

        public static List<Card> GetCardsByPackageId(int packageId)
        {
            string query = "SELECT c.* FROM Cards c " +
                           "JOIN PackagesCards pc ON c.CardsID = pc.CardsID " +
                           "WHERE pc.PackagesID = @packageId";

            var parameter = new NpgsqlParameter("@packageId", packageId);

            List<Card> cards = new List<Card>();

            using (var reader = dataHandler.ExecuteSelectQuery(query, new NpgsqlParameter[] { parameter }))
            {
                while (reader != null && reader.Read())
                {
                    Card card = new Card
                    (
                        reader.GetString(reader.GetOrdinal("Name")),
                        reader.GetInt32(reader.GetOrdinal("Damage")),
                        Enum.Parse<ERegions>(reader.GetString(reader.GetOrdinal("Region"))),
                        Enum.Parse<EType>(reader.GetString(reader.GetOrdinal("Type")))
                    );

                    card.CardsID = reader.GetInt32(reader.GetOrdinal("CardsID"));
                    cards.Add(card);
                }
            }

            return cards;
        }
    }
}
