using MTCG.Database;
using Npgsql;

namespace MTCG
{
    internal class Program
    {

       
        static void Main(string[] args)
        {
            var dbHandler = new DataHandler("localhost", 5432, "mtcgdb", "if22b009", "debian123");

            // Example: Execute a SQL statement (e.g., INSERT, UPDATE, DELETE)
            //string insertSql = "INSERT INTO your_table (column1, column2) VALUES ('value1', 'value2');";
            //dbHandler.ExecuteNonQuery(insertSql);

            // Example: Execute a SELECT statement and read the results
            string selectSql = "SELECT * FROM Cards;";
            NpgsqlDataReader reader = dbHandler.ExecuteQuery(selectSql);

            while (reader.Read())
            {
                Console.WriteLine($"Column1: {reader["column1"]}, Column2: {reader["column2"]}");
            }

            // Close the reader and release resources
            dbHandler.CloseConnection(reader);

            //GameController controller = new GameController();
            //controller.StartGame(DataHandler.UserLogin(UI.GetUsername(), UI.GetPassword()));
        }
    }
}