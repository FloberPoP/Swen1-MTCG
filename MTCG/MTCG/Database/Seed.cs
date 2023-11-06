using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                "(CardsID serial PRIMARY KEY, Name text, Damage int, ManaCost int, Region text, IsDead boolean, SpellType text, MaxHealth int, CurrentHealth int, Stuned boolean)";
            ExecuteNonQuery(createTableQuery);
        }

        public void InsertCardData()
        {
            string insertDataQuery = "INSERT INTO Cards (Name, Healthpoints, IsDead) VALUES " +
                "('Viego', 5, 3, 'SHADOWISLES', false, NULL, 8, 8, false), " +
                "('Garen', 6, 2, 'DEMACIA', false, NULL, 10, 10, false), " +
                "('Cho`Gath', 7, 4, 'VOID', false, NULL, 15, 15, false), " +
                "('Smite', 5, 3, 'DEMACIA', NULL, 'DAMAGE', NULL, NULL, NULL), " +
                "('Cage', 0, 5, 'SHADOWISLES', NULL, 'STUN', NULL, NULL, NULL)";
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
