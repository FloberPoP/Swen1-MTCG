using MTCG.Database;
using System.Net;
using MTCG.Controller;

namespace MTCG
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Seed.Seeding();           

            string baseUrl = "http://localhost:10001/";
            var listener = new HttpListener();
            listener.Prefixes.Add(baseUrl);
            listener.Start();

            Console.WriteLine($"Server listening at {baseUrl}");

            while (true)
            {
                var context = await listener.GetContextAsync();
                await Task.Run(() => ServerController.ProcessRequest(context));
            }
        }
    }
}