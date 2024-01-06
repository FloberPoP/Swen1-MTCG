using MTCG.Database;
using MTCG.Model;
using Npgsql;

namespace MTCG.Repositorys
{
    public static class CardRepository
    {
        public static void CreateCard(Card c)
        {

            string query = "INSERT INTO Cards (CardsID, Name, Damage, Region, Type) VALUES (@cardsid, @name, @damage, @region, @type)";

            var parameters = new NpgsqlParameter[]
            {
                new NpgsqlParameter("@cardsid", c.CardsID),
                new NpgsqlParameter("@name", c.Name),
                new NpgsqlParameter("@damage", c.Damage),
                new NpgsqlParameter("@region", c.Region.ToString()),
                new NpgsqlParameter("@type", c.Type.ToString())
            };

            using (DataHandler dbh = new DataHandler())
            {
                dbh.ExecuteNonQuery(query, parameters);
            }
        }

        public static List<Card> GetCardsByPackageId(int packageId)
        {
            string query = "SELECT c.* FROM Cards c JOIN PackagesCards pc ON c.CardsID = pc.CardsID WHERE pc.PackagesID = @packageId";

            var parameter = new NpgsqlParameter("@packageId", packageId);

            List<Card> cards = new List<Card>();

            using (DataHandler dbh = new DataHandler())
            using (var reader = dbh.ExecuteSelectQuery(query, new NpgsqlParameter[] { parameter }))
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
            string query = "UPDATE Stacks SET Trading = @tradingStatus WHERE UserID = @userId AND CardID = @cardId";

            var parameters = new NpgsqlParameter[]
            {
                new NpgsqlParameter("@userId", userId),
                new NpgsqlParameter("@cardId", cardId),
                new NpgsqlParameter("@tradingStatus", tradingStatus)
            };

            using (DataHandler dbh = new DataHandler())
            {
                dbh.ExecuteNonQuery(query, parameters);
            }
        }


        public static bool IsCardTrading(int cardId)
        {
            string checkQuery = "SELECT Trading FROM Stacks WHERE CardID = @cardId";

            var parameter = new NpgsqlParameter("@cardId", cardId);

            using (DataHandler dbh = new DataHandler())
            using (var reader = dbh.ExecuteSelectQuery(checkQuery, new NpgsqlParameter[] { parameter }))
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

            using (DataHandler dbh = new DataHandler())
            using (var reader = dbh.ExecuteSelectQuery(checkQuery, parameters))
            {
                return reader != null && reader.Read();
            }
        }
        public static Card GetCardByName(string cardName)
        {
            string query = "SELECT * FROM Cards WHERE Name = @cardName";

            var parameter = new NpgsqlParameter("@cardName", cardName);

            using (DataHandler dbh = new DataHandler())
            using (var reader = dbh.ExecuteSelectQuery(query, new NpgsqlParameter[] { parameter }))
            {
                if (reader != null && reader.Read())
                {
                    Card c = new
                    (
                         reader.GetString(reader.GetOrdinal("Name")),
                         reader.GetInt32(reader.GetOrdinal("Damage")),
                         Enum.Parse<ERegions>(reader.GetString(reader.GetOrdinal("Region"))),
                         Enum.Parse<EType>(reader.GetString(reader.GetOrdinal("Type")))
                    );
                    c.CardsID = reader.GetInt32(reader.GetOrdinal("CardsID"));
                    return c;
                }
            }

            return null;
        }

    }
}
