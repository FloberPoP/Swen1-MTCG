using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.VisualBasic;
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
            string query = "UPDATE Users SET Coins = @coins, Elo = @elo, BattleCount = @battleCount, Password = @password WHERE Username = @username";

            var parameters = new NpgsqlParameter[]
            {
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
                    tmp.UserID = reader.GetInt32(reader.GetOrdinal("UsersID"));
                    tmp.Stack = GetUserStack(tmp.Username);
                    tmp.Deck = GetUserDeck(tmp.Username);
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
            string packageQuery = "INSERT INTO Packages (PackagesID, Price) " +
                                  "VALUES (@packagesID, @price)";

            var packageParameters = new NpgsqlParameter[]
            {
                new NpgsqlParameter("@packagesID", p.PackageID),
                new NpgsqlParameter("@price", p.Price)
            };

            // Insert the package entry
            dataHandler.ExecuteNonQuery(packageQuery, packageParameters);

            // Loop through the cards and associate them with the package
            foreach (Card card in p.Cards)
            {
                CreateCard(card);

                string cardsQuery = "INSERT INTO PackagesCards (PackagesID, CardsID) " +
                                    "VALUES (@packagesID, @cardsid)";

                var cardsParameters = new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@packagesID", p.PackageID),
                    new NpgsqlParameter("@cardsid", card.CardsID)
                };

                // Insert the association between the package and the card
                dataHandler.ExecuteNonQuery(cardsQuery, cardsParameters);
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

        public static List<Card> GetUserDeck(string username)
        {
            string query = "SELECT c.* FROM Cards c " +
                           "JOIN Decks d ON c.CardsID = d.CardID " +
                           "JOIN Users u ON d.UserID = u.UsersID " +
                           "WHERE u.Username = @username";

            var parameter = new NpgsqlParameter("@username", username);

            List<Card> userDeck = new List<Card>();

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

                    userDeck.Add(card);
                }
            }

            return userDeck;
        }


        public static bool PurchasePackage(string username)
        {
            User user = GetUserByUsername(username);
            if (user != null)
            {
                Package package = GetRandomPackage(user.UserID);
                package.Cards = GetCardsByPackageId(package.PackageID);

                if (package != null && user.Coins >= package.Price)
                {
                    user.Coins -= package.Price;
                    UpdateUser(user);
                    AddPurchaseRecord(user.UserID, package.PackageID);

                    foreach (Card card in package.Cards)
                    {
                        Console.WriteLine($"cID: {card.CardsID}");
                        AddCardToUserStack(user.UserID, card.CardsID);
                    }
                    return true;
                }
            }

            return false;
        }

        private static void AddPurchaseRecord(int userId, int packageId)
        {
            string query = "INSERT INTO Purchases (UsersID, PackagesID) VALUES (@userId, @packageId)";

            var parameters = new NpgsqlParameter[]
            {
                new NpgsqlParameter("@userId", userId),
                new NpgsqlParameter("@packageId", packageId)
            };

            dataHandler.ExecuteNonQuery(query, parameters);
        }


        private static void AddCardToUserStack(int userId, int cardId)
        {
            string query = "INSERT INTO Stacks (UserID, CardID, Trading) VALUES (@userId, @cardId, false)";

            var parameters = new NpgsqlParameter[]
            {
                new NpgsqlParameter("@userId", userId),
                new NpgsqlParameter("@cardId", cardId)
            };

            dataHandler.ExecuteNonQuery(query, parameters);
        }

        private static Package GetRandomPackage(int userId)
        {
            string query = "SELECT * FROM Packages " +
                           "WHERE PackagesID NOT IN (SELECT PackagesID FROM Purchases WHERE UsersID = @userId) " +
                           "ORDER BY RANDOM() LIMIT 1";

            var parameter = new NpgsqlParameter("@userId", userId);

            using (var reader = dataHandler.ExecuteSelectQuery(query, new NpgsqlParameter[] { parameter }))
            {
                if (reader != null && reader.Read())
                {
                    int packageId = reader.GetInt32(reader.GetOrdinal("PackagesID"));
                    int price = reader.GetInt32(reader.GetOrdinal("Price"));
                    Package package = new Package();
                    package.Price = price;
                    package.PackageID = packageId;
                    return package;
                }
            }
            return null;
        }


        private static List<Card> GetCardsByPackageId(int packageId)
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
                        Enum.Parse < EType >(reader.GetString(reader.GetOrdinal("Type")))
                    );

                    card.CardsID = reader.GetInt32(reader.GetOrdinal("CardsID"));
                    cards.Add(card);
                }
            }

            return cards;
        }

        public static void ClearUserDeck(int userId)
        {
            string query = "DELETE FROM Decks WHERE UserID = @userId";

            var parameter = new NpgsqlParameter("@userId", userId);

            dataHandler.ExecuteNonQuery(query, new NpgsqlParameter[] { parameter });
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

                dataHandler.ExecuteNonQuery(query, parameters);
            }
        }
    }
}
