using MTCG.Cards;
using MTCG.Database;
using MTCG.Users;
using Npgsql;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Sockets;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace MTCG
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Start the server on a separate thread
            Thread serverThread = new Thread(ServerThread);
            serverThread.Start();

            // Press Enter to stop the server
            Console.ReadLine();
        }

        static void ServerThread()
        {
            // Set up the server socket
            TcpListener server = new TcpListener(IPAddress.Any, 8888);
            server.Start();

            Console.WriteLine("Server started. Listening for clients...");

            try
            {
                while (true)
                {
                    // Accept a new client connection
                    TcpClient client = server.AcceptTcpClient();

                    // Start a new thread to handle the client
                    Thread clientThread = new Thread(HandleClientThread);
                    clientThread.Start(client);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                server.Stop();
            }
        }

        static void HandleClientThread(object clientObj)
        {
            using (TcpClient client = (TcpClient)clientObj)
            {
                try
                {
                    NetworkStream stream = client.GetStream();
                    byte[] data = new byte[1024];
                    int bytesRead = stream.Read(data, 0, data.Length);
                    string clientMessage = Encoding.ASCII.GetString(data, 0, bytesRead);

                    Console.WriteLine($"Received from client: {clientMessage}");

                    // Parse the client message (assuming it's in JSON format)
                    var requestData = JsonConvert.DeserializeObject<Dictionary<string, string>>(clientMessage);

                    // Check if it's a login request
                    if (requestData.ContainsKey("Username") && requestData.ContainsKey("Password"))
                    {
                        string username = requestData["Username"];
                        string password = requestData["Password"];

                        // Authenticate the user (replace this with your actual authentication logic)
                        if (AuthenticateUser(username, password))
                        {
                            // Successful login response
                            string responseMessage = "Login successful!";
                            byte[] responseBytes = Encoding.ASCII.GetBytes(responseMessage);
                            stream.Write(responseBytes, 0, responseBytes.Length);
                        }
                        else
                        {
                            // Failed login response
                            string responseMessage = "Invalid credentials!";
                            byte[] responseBytes = Encoding.ASCII.GetBytes(responseMessage);
                            stream.Write(responseBytes, 0, responseBytes.Length);
                        }
                    }
                    else
                    {
                        // Handle other types of requests
                        // ...
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error handling client: {ex.Message}");
                }
            }
        }

        static bool AuthenticateUser(string username, string password)
        {
            // In a real-world scenario, you would query your database to check credentials
            // For simplicity, this example hardcodes a user
            //var user = new User { Username = "kienboec", Password = "daniel" };

            //return user.Username == username && user.Password == password;
            return true;
        }


        public static void AddRandomCardsToDeck(List<Card> stack, List<Card> deck, int numberOfCardsToAdd)
        {
            Random random = new Random();

            for (int i = 0; i < numberOfCardsToAdd; i++)
            {
                int randomIndex = random.Next(0, stack.Count);

                // Add a copy of the randomly selected card to the deck
                deck.Add(new Card(stack[randomIndex].Name, stack[randomIndex].Damage, stack[randomIndex].Region, stack[randomIndex].Type));
            }
        }

        public void Test()
        {
            DataHandler dataHandler = new DataHandler("localhost", "5432", "mtcgdb", "postgres", "debian123");
            Seed seed = new Seed(dataHandler);
            List<Card> stack = new List<Card>();

            //TEST DB and SEED
            seed.ClearDatabase();
            seed.CreateCardsTable();
            seed.InsertCardData();

            string selectDataQuery = "SELECT * FROM Cards";
            using (var reader = dataHandler.ExecuteSelectQuery(selectDataQuery))
            {
                while (reader.Read())
                {
                    stack.Add(new Card(
                        reader["Name"].ToString(),
                        Convert.ToInt32(reader["Damage"]),
                        dataHandler.ERegionsConverter(reader["Region"].ToString()),
                        dataHandler.ETypeConverter(reader["Type"].ToString())
                    ));

                    Console.WriteLine($"Name: {reader["Name"]}, Damage: {reader["Damage"]}, Region: {reader["Region"]}, Type: {reader["Type"]}");
                }
            }


            List<Card> deck = new List<Card>();
            AddRandomCardsToDeck(stack, deck, 5);
            GameController controller = new GameController();
            controller.StartGame(new User(stack, deck, 20, 100, 0, "UserONE", "abc"));
        }

    }
}