using Npgsql;

namespace MTCG.Database
{
    internal static class Seed
    {
        private static readonly DataHandler? dataHandler = new DataHandler();

        public static void Seeding()
        {
            ClearDatabase();
            //ClearPurchases();
            CreateTables();
            InsertCardData();
            InsertUser();     
        }
        public static void CreateTables()
        {
            string createCards = "CREATE TABLE IF NOT EXISTS Cards (CardsID serial PRIMARY KEY, Name text, Damage int, Region text, Type text)";
            ExecuteNonQuery(createCards);

            string createUsers = "CREATE TABLE IF NOT EXISTS Users " +
                "(UsersID serial PRIMARY KEY, " +
                "Username text, " +
                "Password text, " +
                "Bio text,"+
                "Image text,"+
                "Coins int, " +
                "Elo int)";
            ExecuteNonQuery(createUsers);


            string createStacks = "CREATE TABLE IF NOT EXISTS Stacks " +
                "(StacksID serial PRIMARY KEY, UserID int REFERENCES Users(UsersID), CardID int REFERENCES Cards(CardsID), Trading bool)";
            ExecuteNonQuery(createStacks);

            string createDecks = "CREATE TABLE IF NOT EXISTS Decks " +
                "(DecksID serial PRIMARY KEY, UserID int REFERENCES Users(UsersID), CardID int REFERENCES Cards(CardsID))";
            ExecuteNonQuery(createDecks);

            string createBattleLogs = "CREATE TABLE IF NOT EXISTS BattleLogs " +
                "(BattleID serial PRIMARY KEY, " +
                "Rounds text, " +
                "Looser text, " +
                "Winner text)";
            ExecuteNonQuery(createBattleLogs);

            string createPackages = "CREATE TABLE IF NOT EXISTS Packages " +
                 "(PackagesID serial PRIMARY KEY, Price int)";
            ExecuteNonQuery(createPackages);

            string createPackagesCards = "CREATE TABLE IF NOT EXISTS PackagesCards " +
                 "(PackageCardID serial PRIMARY KEY, PackagesID int REFERENCES Packages(PackagesID), CardsID int REFERENCES Cards(CardsID))";
            ExecuteNonQuery(createPackagesCards);

            string createPurchases = "CREATE TABLE IF NOT EXISTS Purchases " +
                "(PurchasesID serial PRIMARY KEY, PackagesID int REFERENCES Packages(PackagesID), UsersID int REFERENCES Users(UsersID))";
            ExecuteNonQuery(createPurchases);
        }

        public static void InsertCardData()
        {
            string insertDataQuery = "INSERT INTO Cards (Name, Damage, Region, Type) VALUES " +
                // Water
                "('WaterGoblin', 10, 'WATER', 'MONSTER'), " +
                "('WaterSpell', 20, 'WATER', 'SPELL'), " +
                "('AnotherWaterSpell', 15, 'WATER', 'SPELL'), " +
                "('WaterSerpent', 30, 'WATER', 'MONSTER'), " +
                // Fire
                "('Dragon', 50, 'FIRE', 'MONSTER'), " +
                "('FireSpell', 25, 'FIRE', 'SPELL'), " +
                "('AnotherFireSpell', 30, 'FIRE', 'SPELL'), " +
                "('FirePhoenix', 55, 'FIRE', 'MONSTER'), " +
                // Normal
                "('Ork', 45, 'NORMAL', 'MONSTER'), " +
                "('Golem', 40, 'NORMAL', 'MONSTER'), " +
                "('NormalSpell', 25, 'NORMAL', 'SPELL'), " +
                "('AnotherNormalSpell', 18, 'NORMAL', 'SPELL')";

            ExecuteNonQuery(insertDataQuery);
        }


        public static void ClearDatabase()
        {
            ExecuteNonQuery("DROP TABLE IF EXISTS Stacks, Decks, Cards, Users, Packages, PackagesCards, Purchases CASCADE");
        }

        public static void ClearPurchases()
        {
            ExecuteNonQuery("DROP TABLE IF EXISTS Purchases, Users CASCADE");
        }
        private static void InsertUser()
        {
            ExecuteNonQuery("INSERT INTO Users (Username, Coins, Elo, Password) VALUES ('kienboec', 20, 100, 'daniel')");
            ExecuteNonQuery("INSERT INTO Users (Username, Coins, Elo, Password) VALUES ('altenhof', 20, 100, 'markus')");

            // Get the UserIDs for the inserted users
            int kienboecUserID = 1;
            int altenhofUserID = 2;

            // Insert decks for each user with 4 cards each
            ExecuteNonQuery($"INSERT INTO Decks (UserID, CardID) VALUES ({kienboecUserID}, (SELECT CardsID FROM Cards WHERE Name = 'WaterGoblin')), " +
                                                                           $"({kienboecUserID}, (SELECT CardsID FROM Cards WHERE Name = 'WaterSpell')), " +
                                                                           $"({kienboecUserID}, (SELECT CardsID FROM Cards WHERE Name = 'AnotherWaterSpell')), " +
                                                                           $"({kienboecUserID}, (SELECT CardsID FROM Cards WHERE Name = 'WaterSerpent'))");

            ExecuteNonQuery($"INSERT INTO Decks (UserID, CardID) VALUES ({altenhofUserID}, (SELECT CardsID FROM Cards WHERE Name = 'Dragon')), " +
                                                                           $"({altenhofUserID}, (SELECT CardsID FROM Cards WHERE Name = 'FireSpell')), " +
                                                                           $"({altenhofUserID}, (SELECT CardsID FROM Cards WHERE Name = 'AnotherFireSpell')), " +
                                                                           $"({altenhofUserID}, (SELECT CardsID FROM Cards WHERE Name = 'FirePhoenix'))");
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
                Console.WriteLine("Error: " + ex.Message);
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
                        Console.Write(reader.GetName(i) + "\t");
                    }

                    Console.WriteLine();

                    while (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            Console.Write(reader[i] + "\t");
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
