using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace MTCG.Database
{
    internal class Seed
    {
        private readonly DataHandler dataHandler;

        public Seed(DataHandler dataHandler)
        {
            this.dataHandler = dataHandler;
        }

        public void CreateCardsTable()
        {
            string createTableQuery = "CREATE TABLE IF NOT EXISTS Cards " +
                "(CardsID serial PRIMARY KEY, Name text, Damage int, Region text, Type text)";
            ExecuteNonQuery(createTableQuery);
        }

        public void InsertCardData()
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


        public void ClearDatabase()
        {
            string clearDatabaseQuery = "DROP TABLE IF EXISTS Cards";
            ExecuteNonQuery(clearDatabaseQuery);
        }

        private void ExecuteNonQuery(string query)
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
    }
}
