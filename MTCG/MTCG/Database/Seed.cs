using Npgsql;

namespace MTCG.Database
{
    internal static class Seed
    {
        private static readonly DataHandler dataHandler = new("localhost", "5432", "mtcgdb", "postgres", "debian123");

        public static void Seeding()
        {
            //ClearDatabase();
            //ClearPurchases();
            //CreateTables();
            //InsertUser();
            //InsertCardData();        
        }
        public static void CreateTables()
        {
            string createCards = "CREATE TABLE IF NOT EXISTS Cards (CardsID serial PRIMARY KEY, Name text, Damage int, Region text, Type text)";
            ExecuteNonQuery(createCards);

            string createUsers = "CREATE TABLE IF NOT EXISTS Users " +
                "(UsersID serial PRIMARY KEY, " +
                "Username text, " +
                "Password text, " +
                "StackID int, " +
                "DeckID int, " +
                "Coins int, " +
                "Elo int, " +
                "BattleCount int)";
            ExecuteNonQuery(createUsers);


            string createStacks = "CREATE TABLE IF NOT EXISTS Stacks " +
                "(StacksID serial PRIMARY KEY, UserID int REFERENCES Users(UsersID), CardID int REFERENCES Cards(CardsID), Trading bool)";
            ExecuteNonQuery(createStacks);

            string createDecks = "CREATE TABLE IF NOT EXISTS Decks " +
                "(DecksID serial PRIMARY KEY, UserID int REFERENCES Users(UsersID), CardID int REFERENCES Cards(CardsID))";
            ExecuteNonQuery(createDecks);

            string alterUsers = "ALTER TABLE Users " +
                "ADD CONSTRAINT FK_Users_Stacks FOREIGN KEY (StackID) REFERENCES Stacks(StacksID) ON DELETE SET NULL, " +
                "ADD CONSTRAINT FK_Users_Decks FOREIGN KEY (DeckID) REFERENCES Decks(DecksID) ON DELETE SET NULL";
            ExecuteNonQuery(alterUsers);

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

            ExecuteNonQuery("INSERT INTO Users (Username, StackID, DeckID, Coins, Elo, BattleCount, Password) VALUES ('SeedUser', null, null, 20, 100, 0, 'debian123')");
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
            ExecuteNonQuery("INSERT INTO Users (Username, StackID, DeckID, Coins, Elo, BattleCount, Password) VALUES ('kienboec', null, null, 20, 100, 0, 'daniel')");
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
