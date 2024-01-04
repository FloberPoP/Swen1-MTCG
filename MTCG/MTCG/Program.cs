using MTCG.Database;
using MTCG.Controller;

namespace MTCG
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Seed.Seeding();

            string ipAddress = "127.0.0.1"; // Use your desired IP address
            int port = 10001; // Use your desired port

            SocketServer socketServer = new SocketServer(ipAddress, port);
            socketServer.Start();
        }
    }
}