using MTCG.Database;
using MTCG.Model;
using Npgsql;

namespace MTCG.Repositorys
{
    public static class TradingRepository
    {
        public static void CreateTrade(TradeRequirement t)
        {

            string query = "INSERT INTO Trades (TradesID, UsersID, CardID, CardRegion, CardType, MinimumDamage) VALUES (@tradesid, @usersid, @cardid, @cardregion, @cardtype, @minimumdamage)";

            var parameters = new NpgsqlParameter[]
            {
                new NpgsqlParameter("@tradesid", t.TradesID),
                new NpgsqlParameter("@usersid", t.UsersID),
                new NpgsqlParameter("@cardid", t.CardID),
                new NpgsqlParameter("@cardregion", t.CardRegion.ToString()),
                new NpgsqlParameter("@cardtype", t.CardType.ToString()),
                new NpgsqlParameter("@minimumdamage", t.MinimumDamage)
            };

            using (DataHandler dbh = new DataHandler())
            {
                dbh.ExecuteNonQuery(query, parameters);
            }
        }

        public static TradeRequirement GetTradebyTradID(int  tradesid)
        {
            string query = "SELECT * FROM Trades WHERE TradesID = @tradesid";

            var parameter = new NpgsqlParameter("@tradesid", tradesid);

            using (DataHandler dbh = new DataHandler())
            using (var reader = dbh.ExecuteSelectQuery(query, new NpgsqlParameter[] { parameter }))
            {
                if (reader != null && reader.Read())
                {
                    TradeRequirement tmp = new TradeRequirement();
                    tmp.CardID = reader.GetInt32(reader.GetOrdinal("CardID"));
                    tmp.UsersID = reader.GetInt32(reader.GetOrdinal("UsersID"));
                    tmp.TradesID = reader.GetInt32(reader.GetOrdinal("TradesID"));
                    tmp.MinimumDamage = reader.GetInt32(reader.GetOrdinal("MinimumDamage"));
                    tmp.CardType = Enum.Parse<EType>(reader.GetString(reader.GetOrdinal("CardType")));
                    tmp.CardRegion = Enum.Parse<ERegions>(reader.GetString(reader.GetOrdinal("CardRegion")));
                    return tmp;
                }
            }
            return null;
        }

        public static List<TradeRequirement> GetAllTrades()
        {
            List<TradeRequirement> trades = new List<TradeRequirement>();

            string query = "SELECT * FROM Trades";

            using (DataHandler dbh = new DataHandler())
            using (var reader = dbh.ExecuteSelectQuery(query, null))
            {
                while (reader != null && reader.Read())
                {
                    TradeRequirement tmp = new TradeRequirement();

                    tmp.TradesID = reader.GetInt32(reader.GetOrdinal("TradesID"));
                    tmp.UsersID = reader.GetInt32(reader.GetOrdinal("UsersID"));
                    tmp.CardID = reader.GetInt32(reader.GetOrdinal("CardID"));
                    tmp.MinimumDamage = reader.GetInt32(reader.GetOrdinal("MinimumDamage"));
                    tmp.CardType = Enum.Parse<EType>(reader.GetString(reader.GetOrdinal("CardType")));
                    tmp.CardRegion = Enum.Parse<ERegions>(reader.GetString(reader.GetOrdinal("CardRegion")));

                    trades.Add(tmp);
                }
            }

            return trades;
        }

        public static void DeleteTradeById(int tradesId)
        {
            string query = "DELETE FROM Trades WHERE TradesID = @tradesid";

            var parameter = new NpgsqlParameter("@tradesid", tradesId);

            using (DataHandler dbh = new DataHandler())
            {
                dbh.ExecuteNonQuery(query, new NpgsqlParameter[] { parameter });
            }
        }

        public static void AcceptTrade(int tradeID, int acceptingCardID, int acceptingUserID)
        {        
            TradeRequirement tradeOffer = GetTradebyTradID(tradeID);         

            DeleteTradeById(tradeID);

            CardRepository.UpdateCardTradingStatus(tradeOffer.UsersID,tradeOffer.CardID, false);

            StackRepository.AddCardToUserStack(tradeOffer.UsersID, acceptingCardID);
            StackRepository.AddCardToUserStack(acceptingUserID, tradeOffer.CardID);

            StackRepository.DeleteCardFromUserStack(tradeOffer.UsersID, tradeOffer.CardID);
            StackRepository.DeleteCardFromUserStack(acceptingUserID, acceptingCardID); 
        }
    }
}
