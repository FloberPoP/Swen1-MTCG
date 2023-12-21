using MTCG.Cards;
using MTCG.Database;
using MTCG.Trading;
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

        public static void CreatePackages(Package p)
        {
            string query = "INSERT INTO Packages (PackagesKey, CardsID, Price) " +
                           "VALUES (@packageskey, @cardsid, @price)";

            List<Card> cards = p.Cards;

            foreach (Card card in cards)
            {
                CreateCard(card);
                var parameters = new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@packageskey", p.PackageKey),
                    new NpgsqlParameter("@cardsid", card.CardsID),
                    new NpgsqlParameter("@price", p.Price)
                };

                dataHandler.ExecuteNonQuery(query, parameters);
            }
            
        }

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
    }
}
