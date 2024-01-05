using Npgsql;

namespace MTCG.Database
{
    public static class Seed
    {
        private static readonly DataHandler? dataHandler = new DataHandler();

        public static void Seeding()
        {
            //ClearDatabase();
            CreateTables();
            PrintTableContents();
        }
        public static void CreateTables()
        {
            string createCards = "CREATE TABLE IF NOT EXISTS Cards (CardsID serial PRIMARY KEY, Name text, Damage int, Region text, Type text)";
            ExecuteNonQuery(createCards);

            string createUsers = "CREATE TABLE IF NOT EXISTS Users (UsersID serial PRIMARY KEY, Username text, Password text, Bio text, Image text, Coins int, Elo int)";
            ExecuteNonQuery(createUsers);

            string createStacks = "CREATE TABLE IF NOT EXISTS Stacks (StacksID serial PRIMARY KEY, UserID int REFERENCES Users(UsersID), CardID int REFERENCES Cards(CardsID), Trading bool)";
            ExecuteNonQuery(createStacks);

            string createDecks = "CREATE TABLE IF NOT EXISTS Decks (DecksID serial PRIMARY KEY, UserID int REFERENCES Users(UsersID), CardID int REFERENCES Cards(CardsID))";
            ExecuteNonQuery(createDecks);

            string createBattleLogs = "CREATE TABLE IF NOT EXISTS BattleLogs (BattleID serial PRIMARY KEY, Rounds text, LoserID int REFERENCES Users(UsersID), WinnerID int REFERENCES Users(UsersID), Draw bool)";
            ExecuteNonQuery(createBattleLogs);

            string createPackages = "CREATE TABLE IF NOT EXISTS Packages (PackagesID serial PRIMARY KEY, Price int)";
            ExecuteNonQuery(createPackages);

            string createPackagesCards = "CREATE TABLE IF NOT EXISTS PackagesCards (PackageCardID serial PRIMARY KEY, PackagesID int REFERENCES Packages(PackagesID), CardsID int REFERENCES Cards(CardsID))";
            ExecuteNonQuery(createPackagesCards);

            string createPurchases = "CREATE TABLE IF NOT EXISTS Purchases (PurchasesID serial PRIMARY KEY, PackagesID int REFERENCES Packages(PackagesID), UsersID int REFERENCES Users(UsersID))";
            ExecuteNonQuery(createPurchases);

            string createTrades = "CREATE TABLE IF NOT EXISTS Trades (TradesID serial PRIMARY KEY, UsersID int REFERENCES Users(UsersID), CardID int REFERENCES Cards(CardsID), CardRegion text, CardType text, MinimumDamage int)";
            ExecuteNonQuery(createTrades);
        }
        public static void ClearDatabase()
        {
            ExecuteNonQuery("DROP TABLE IF EXISTS Stacks, Decks, Cards, Users, Packages, PackagesCards, Purchases, BattleLogs, Trades CASCADE");
        }

        private static void ExecuteNonQuery(string query)
        {
            try
            {
                dataHandler.OpenConnection();
                using (var cmd = new NpgsqlCommand(query, dataHandler.Connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                dataHandler.CloseConnection();
            }
        }

        #region Tesing

        public static void PrintTableContents()
        {
            // Print Cards table
            string selectCards = "SELECT * FROM Cards";
            PrintTable("Cards", selectCards);

            // Print Stacks table
            string selectStacks = "SELECT * FROM Stacks";
            PrintTable("Stacks", selectStacks);

            // Print Decks table
            string selectDecks = "SELECT * FROM Decks";
            PrintTable("Decks", selectDecks);

            // Print Users table
            string selectUsers = "SELECT * FROM Users";
            PrintTable("Users", selectUsers);

            // Print BattleLogs table
            string selectBattleLogs = "SELECT * FROM BattleLogs";
            PrintTable("BattleLogs", selectBattleLogs);

            // Print Packages table
            string selectPackages = "SELECT * FROM Packages";
            PrintTable("Packages", selectPackages);

            // Print PackagesCards table
            string selectPackagesCards = "SELECT * FROM PackagesCards";
            PrintTable("PackagesCards", selectPackagesCards);

            // Print Purchases table
            string selectPurchases = "SELECT * FROM Purchases";
            PrintTable("Purchases", selectPurchases);

            // Print Trades table
            string selectTrades = "SELECT * FROM Trades";
            PrintTable("Trades", selectTrades);
        }

        private static void PrintTable(string tableName, string selectQuery)
        {
            try
            {
                dataHandler.OpenConnection();

                using (var cmd = new NpgsqlCommand(selectQuery, dataHandler.Connection))
                using (var reader = cmd.ExecuteReader())
                {
                    Console.WriteLine($"Table: {tableName}");
                    Console.WriteLine("--------------------------------------------------");

                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        Console.Write($"{reader.GetName(i)}\t");
                    }

                    Console.WriteLine();

                    while (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            Console.Write($"{reader[i]}\t");
                        }
                        Console.WriteLine();
                    }

                    Console.WriteLine("--------------------------------------------------\n\n\n");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving data from {tableName}: {ex.Message}");
            }
            finally
            {
                dataHandler.CloseConnection();
            }
        }
        #endregion
    }
}
