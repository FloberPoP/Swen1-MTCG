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

        public static void UpdateCardTradingStatus(int userId, int cardId, bool tradingStatus)
        {
            string updateQuery = "UPDATE Stacks SET Trading = @tradingStatus WHERE UserID = @userId AND CardID = @cardId";

            var parameters = new NpgsqlParameter[]
            {
                new NpgsqlParameter("@userId", userId),
                new NpgsqlParameter("@cardId", cardId),
                new NpgsqlParameter("@tradingStatus", tradingStatus)
            };

            dataHandler.ExecuteNonQuery(updateQuery, parameters);
        }


        public static bool IsCardTrading(int cardId)
        {
            string checkQuery = "SELECT Trading FROM Stacks WHERE CardID = @cardId";

            var parameter = new NpgsqlParameter("@cardId", cardId);

            using (var reader = dataHandler.ExecuteSelectQuery(checkQuery, new NpgsqlParameter[] { parameter }))
            {
                if (reader != null && reader.Read())
                {
                    return reader.GetBoolean(reader.GetOrdinal("Trading"));
                }
            }
            return false;
        }

        public static bool IsCardEligibleForTrade(int acceptingCardID, ERegions tradeOfferCardRegion, EType tradeOfferCardType, int tradeOfferMinimumDamage)
        {
            string checkQuery = "SELECT * FROM Cards WHERE CardsID = @acceptingCardID AND Region = @cardRegion AND Type = @cardType AND Damage >= @minimumDamage";
            var parameters = new NpgsqlParameter[]
            {
                new NpgsqlParameter("@acceptingCardID", acceptingCardID),
                new NpgsqlParameter("@cardRegion", tradeOfferCardRegion.ToString()),
                new NpgsqlParameter("@cardType", tradeOfferCardType.ToString()),
                new NpgsqlParameter("@minimumDamage", tradeOfferMinimumDamage)
            };

            using (var reader = dataHandler.ExecuteSelectQuery(checkQuery, parameters))
            {
                return reader != null && reader.Read();
            }
        }

    }
}
