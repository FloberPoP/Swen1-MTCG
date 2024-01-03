using Npgsql;
using MTCG.Model;

namespace MTCG.Database
{
    internal class DataHandler
    {
        private string connectionString;
        public NpgsqlConnection Connection { get; }

        public DataHandler()
        {
            string host = "localhost";
            string port = "5432";
            string database = "mtcgdb";
            string username = "postgres";
            string password = "debian123";
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

        public NpgsqlDataReader ExecuteSelectQuery(string query, NpgsqlParameter[] parameters = null)
        {
            try
            {
                OpenConnection();
                using (var cmd = new NpgsqlCommand(query, Connection))
                {
                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }
                    return cmd.ExecuteReader();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return null;
            }
        }

        public int ExecuteNonQuery(string query, NpgsqlParameter[] parameters = null)
        {
            try
            {
                OpenConnection();
                using (var cmd = new NpgsqlCommand(query, Connection))
                {
                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }
                    return cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return -1;
            }
        }
    }
}
