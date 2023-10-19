using MTCG.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace MTCG.Database
{
    internal class DataHandler
    {
        private string ConnectionString { get; set; }

        public DataHandler(string host, int port, string database, string username, string password)
        {
            // Construct the connection string
            //ConnectionString = $"Host={host};Port={port};Database=mtcgdb;Username=if22b009;Password=debian123";
            ConnectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password}";
        }

        public void ExecuteNonQuery(string sql)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand(sql, connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public NpgsqlDataReader ExecuteQuery(string sql)
        {
            NpgsqlDataReader reader = null;
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand(sql, connection))
                {
                    reader = cmd.ExecuteReader();
                }
            }
            return reader;
        }

        public void CloseConnection(NpgsqlDataReader reader)
        {
            if (reader != null && !reader.IsClosed)
            {
                reader.Close();
            }
        }
        public static User UserLogin(string uname, string pwd)
        {
            Stack stack = new Stack();
            Deck deck = new Deck();
            User u = new(stack, deck, 20, 100, uname, pwd);
            return u;
        }
    }
}

//Eine klasse fürs parsing(Json, csv, ...)
//