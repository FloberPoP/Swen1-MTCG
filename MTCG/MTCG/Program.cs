using MTCG.Database;
using System.Net;
using MTCG.Controller;

namespace MTCG
{
    /*
    curl -X POST -H "Content-Type: application/json" -d "{\"username\": \"TestUser\", \"password\": \"debain123\"}" --verbose http://localhost:10001/users
      
    curl -X POST -H "Content-Type: application/json" -d "{\"username\": \"TestUser\", \"password\": \"debain123\"}" --verbose http://localhost:10001/sessions
    curl -i -X POST http://localhost:10001/packages --header "Content-Type: application/json" --header "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiVGVzdFVzZXIiLCJleHAiOjE3MDMxNzg1MDIsImlzcyI6ImlmMjJiMDA5IiwiYXVkIjoidGVjaG5pa3Vtd2llbiJ9.-01HAhidCK5ifbeXdqgB65IiEh2yNYlwadqHvrWprjE" -d "[{\"Id\":\"845f0dc7-37d0-426e-994e-43fc3ac83c08\", \"Name\":\"WaterGoblin\", \"Damage\": 10.0}, {\"Id\":\"99f8f8dc-e25e-4a95-aa2c-782823f36e2a\", \"Name\":\"Dragon\", \"Damage\": 50.0}, {\"Id\":\"e85e3976-7c86-4d06-9a80-641c2019a79f\", \"Name\":\"WaterSpell\", \"Damage\": 20.0}, {\"Id\":\"1cb6ab86-bdb2-47e5-b6e4-68c5ab389334\", \"Name\":\"Ork\", \"Damage\": 45.0}, {\"Id\":\"dfdd758f-649c-40f9-ba3a-8657f4b3439f\", \"Name\":\"FireSpell\",    \"Damage\": 25.0}]"
    
    curl -X POST http://localhost:10001/packages -H "Content-Type: application/json" -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiVGVzdFVzZXIiLCJleHAiOjE3MDMxODE0NzgsImlzcyI6ImlmMjJiMDA5IiwiYXVkIjoidGVjaG5pa3Vtd2llbiJ9.54B3-S4SWWE1IvD4v8sUC4fDkhMg1ONvfXOyr-w-3vA" -d "{\"PackageKey\": 1, \"Cards\": [{\"CardsID\": 20, \"Name\": \"WaterGoblin\", \"Damage\": 10, \"Region\": \"WATER\", \"Type\": \"MONSTER\"}, {\"CardsID\": 21, \"Name\": \"WaterSpell\", \"Damage\": 20, \"Region\": \"WATER\", \"Type\": \"SPELL\"}, {\"CardsID\": 22, \"Name\": \"AnotherWaterSpell\", \"Damage\": 15, \"Region\": \"WATER\", \"Type\": \"SPELL\"}, {\"CardsID\": 23, \"Name\": \"WaterSerpent\", \"Damage\": 30, \"Region\": \"WATER\", \"Type\": \"MONSTER\"}, {\"CardsID\": 24, \"Name\": \"Dragon\", \"Damage\": 50, \"Region\": \"FIRE\", \"Type\": \"MONSTER\"}], \"Price\": 5}"
    */
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Seed.Seeding();
            Seed.PrintTableContents();

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