using MTCG.Cards;
using MTCG.Database;
using MTCG.Users;
using System.Net.Sockets;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace MTCG
{
    //curl -X POST -H "Content-Type: application/json" -d "{\"username\": \"TestUser\", \"password\": \"debain123\"}" --verbose http://localhost:10001/users

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
                await Task.Run(() => ProcessRequest(context));
            }
        }

        static async Task ProcessRequest(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            string responseString = "";
            string method = request.HttpMethod.ToUpper();

            switch (request.Url.LocalPath.ToLower())
            {
                case "/users":
                    if (method == "POST")
                    {
                        using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                        {
                            string requestBody = await reader.ReadToEndAsync();
                          
                            User user = JsonConvert.DeserializeObject<User>(requestBody); 
                            if (UserRepository.GetUserByUsername(user.Username) == null)
                            {
                                UserRepository.CreateUser(user);
                                responseString = "User registered successfully.";
                            }
                            else
                            {
                                responseString = "Username already taken.";
                                response.StatusCode = (int)HttpStatusCode.Conflict;
                            }               
                        }  
                    }
                    break;

                case "/sessions":
                    if (method == "POST")
                    {
                        using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                        {
                            string requestBody = await reader.ReadToEndAsync();
                            User user = JsonConvert.DeserializeObject<User>(requestBody);
                            if (UserRepository.ValidateUserCredentials(user.Username, user.Password))
                            {
                                //TOKEN SETZTEN TO DO
                                responseString = "User logged in successfully.";
                            }
                            else
                            {
                                responseString = "Invalid username or password for login.";
                                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                            }
                        }
                    }
                    break;

                case "/packages":
                    if (method == "POST")
                    {
                        using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                        {
                            string requestBody = await reader.ReadToEndAsync();
                            // Deserialize the JSON content using JSON.NET
                            // Perform package creation logic
                        }

                        responseString = "Package created successfully.";
                    }
                    break;

                default:
                    responseString = "Invalid endpoint.";
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    break;
            }

            await Console.Out.WriteLineAsync($"Send to Client: {responseString}");
            byte[] buffer = Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;

            using (var output = response.OutputStream)
            {
                await output.WriteAsync(buffer, 0, buffer.Length);
            }

            response.Close();
        }
    }
}