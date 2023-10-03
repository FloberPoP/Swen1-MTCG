namespace MTCG
{
    internal class Program
    {

       
        static void Main(string[] args)
        {
            GameController controller = new GameController();
            controller.StartGame(DataHandler.UserLogin(UI.GetUsername(), UI.GetPassword()));
        }
    }
}