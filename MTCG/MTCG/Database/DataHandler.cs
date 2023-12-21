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

    /*
    DOCKER Befehl:
    docker run --name MTCG_Container -p 5432:5432 -e POSTGRES_PASSWORD=debian123 -e POSTGRES_DB=mtcgdb -d postgres
    */
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

        public EType ETypeConverter(string type)
        {
            if (Enum.TryParse<EType>(type, out var result))
            {
                return result;
            }
            else
            {
                throw new ArgumentException($"Invalid type: {type}");
            }
        }

        public ERegions ERegionsConverter(string region)
        {
            if (Enum.TryParse<ERegions>(region, out var result))
            {
                return result;
            }
            else
            {
                throw new ArgumentException($"Invalid region: {region}");
            }
        }

        // Example usage for SELECT:
        // var reader = dataHandler.ExecuteSelectQuery("SELECT * FROM TableName WHERE ColumnName = @param1", new NpgsqlParameter[] { new NpgsqlParameter("@param1", "someValue") });

        // Example usage for INSERT/UPDATE:
        // var affectedRows = dataHandler.ExecuteNonQuery("INSERT INTO TableName (ColumnName) VALUES (@param1)", new NpgsqlParameter[] { new NpgsqlParameter("@param1", "someValue") });
    }
}