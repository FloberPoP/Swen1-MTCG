using MTCG.Database;
using Npgsql;

namespace MTCG
{
    internal class Program
    {

       
        static void Main(string[] args)
        {
            DataHandler dataHandler = new DataHandler("localhost", "5432", "mtcgdb", "postgres", "debian123");
            Seed seed = new Seed(dataHandler);

            // Erstelle die Cards-Tabelle
            seed.CreateCardsTable();

            // Füge Daten in die Tabelle ein
            seed.InsertCardData();

            // Lösche alle Tabellen und Daten in der Datenbank
            seed.ClearDatabase();


            //GameController controller = new GameController();
            //controller.StartGame(DataHandler.UserLogin(UI.GetUsername(), UI.GetPassword()));
        }
    }
}