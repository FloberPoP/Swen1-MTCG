using MTCG.Database;
using MTCG.Model;
using Npgsql;

namespace MTCG.Repositorys
{
    internal static class PackageRepository
    {
        private static readonly DataHandler? dataHandler = new DataHandler();

        public static bool PurchasePackage(string username)
        {
            User user = UserRepository.GetUserByUsername(username);
            if (user != null)
            {
                Package package = GetRandomPackage(user.UserID);
                package.Cards = CardRepository.GetCardsByPackageId(package.PackageID);

                if (package != null && user.Coins >= package.Price)
                {
                    user.Coins -= package.Price;
                    UserRepository.UpdateUser(user);
                    AddPurchaseRecord(user.UserID, package.PackageID);

                    foreach (Card card in package.Cards)
                    {
                        Console.WriteLine($"cID: {card.CardsID}");
                        StackRepository.AddCardToUserStack(user.UserID, card.CardsID);
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
        public static void CreatePackages(Package p)
        {
            string packageQuery = "INSERT INTO Packages (PackagesID, Price) " +
                                  "VALUES (@packagesID, @price)";

            var packageParameters = new NpgsqlParameter[]
            {
                new NpgsqlParameter("@packagesID", p.PackageID),
                new NpgsqlParameter("@price", p.Price)
            };

            dataHandler.ExecuteNonQuery(packageQuery, packageParameters);
            foreach (Card card in p.Cards)
            {
                CardRepository.CreateCard(card);

                string cardsQuery = "INSERT INTO PackagesCards (PackagesID, CardsID) " +
                                    "VALUES (@packagesID, @cardsid)";

                var cardsParameters = new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@packagesID", p.PackageID),
                    new NpgsqlParameter("@cardsid", card.CardsID)
                };

                dataHandler.ExecuteNonQuery(cardsQuery, cardsParameters);
            }
        }
    }
}
