using MTCG.Database;
using Npgsql;

namespace MTCG
{
    internal class Program
    {

       
        static void Main(string[] args)
        {
            var dbHandler = new DataHandler("172.17.0.2", "5432", "mtcgdb", "postgres", "");

            string region = "SHADOWISLES";
            List<string> shadowIslesCardNames = dbHandler.GetCardNamesByRegion(region);

            foreach (var cardName in shadowIslesCardNames)
            {
                Console.WriteLine($"Card Name: {cardName}");
            }

            //GameController controller = new GameController();
            //controller.StartGame(DataHandler.UserLogin(UI.GetUsername(), UI.GetPassword()));
        }
    }
}