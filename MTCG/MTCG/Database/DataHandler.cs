using MTCG.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using MTCG.Cards;

namespace MTCG.Database
{
    internal class DataHandler
    {
        private string connectionString;
        private NpgsqlConnection connection;

        public DataHandler(string host, string port, string database, string username, string password)
        {
            connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password}";
            connection = new NpgsqlConnection(connectionString);
        }

        public void OpenConnection()
        {
            if (connection.State != System.Data.ConnectionState.Open)
            {
                connection.Open();
            }
        }

        public void CloseConnection()
        {
            if (connection.State != System.Data.ConnectionState.Closed)
            {
                connection.Close();
            }
        }

        public NpgsqlDataReader ExecuteQuery(string query)
        {
            try
            {
                OpenConnection();
                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    return cmd.ExecuteReader();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return null;
            }
        }

        public static User UserLogin(string uname, string pwd)
        {
            Stack stack = new Stack();
            Deck deck = new Deck();
            User u = new(stack, deck, 20, 100, uname, pwd);
            return u;
        }

        public List<string> GetCardNamesByRegion(string region)
        {
            List<string> cardNames = new List<string>();

            try
            {
                OpenConnection();

                string query = "SELECT Name FROM Cards WHERE Region = @Region";
                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("Region", region);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string cardName = reader.GetString(reader.GetOrdinal("Name"));
                            cardNames.Add(cardName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            finally
            {
                CloseConnection();
            }

            return cardNames;
        }
    }
}

//Eine klasse fürs parsing(Json, csv, ...)
//