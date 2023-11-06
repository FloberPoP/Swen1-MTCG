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
        public NpgsqlConnection Connection { get; }   

        public DataHandler(string host, string port, string database, string username, string password)
        {
            connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password}";
            Connection = new NpgsqlConnection(connectionString);
        }

        public void OpenConnection()
        {
            if (Connection.State != System.Data.ConnectionState.Open)
            {
                Connection.Open();
            }
        }

        public void CloseConnection()
        {
            if (Connection.State != System.Data.ConnectionState.Closed)
            {
                Connection.Close();
            }
        }

        public NpgsqlDataReader ExecuteQuery(string query)
        {
            try
            {
                OpenConnection();
                using (var cmd = new NpgsqlCommand(query, Connection))
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
    }
}