using Microsoft.IdentityModel.Tokens;
using MTCG.Cards;
using MTCG.Database;
using MTCG.Trading;
using MTCG.Users;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace MTCG.Controller
{
    public static class ServerController
    {

        public static async Task ProcessRequest(HttpListenerContext context)
        {
            var request = context.Request;
            ClientResponse clientResponse = new ClientResponse();
            clientResponse.responseString = "";
            clientResponse.response = context.Response;

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
                                clientResponse.responseString = "User registered successfully.";
                            }
                            else
                            {
                                clientResponse.responseString = "Username already taken.";
                                clientResponse.response.StatusCode = (int)HttpStatusCode.Conflict;
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
                            if (UserRepository.GetUserByUsername(user.Username) != null && ValidateUserCredentials(user.Username, user.Password))
                            {
                                // Generate and send JWT token
                                var token = GenerateToken(user.Username);
                                clientResponse.response.AddHeader("Authorization", $"Bearer {token}");
                                clientResponse.responseString = "User logged in successfully.";
                            }
                            else
                            {
                                clientResponse.responseString = "Invalid username or password for login.";
                                clientResponse.response.StatusCode = (int)HttpStatusCode.Unauthorized;
                            }
                        }
                    }
                    break;

                case "/packages":
                    if (method == "POST")
                    {
                        string authHeader = request.Headers["Authorization"];
                        string token = authHeader.Replace("Bearer ", string.Empty);

                        clientResponse = IsValidToken(request, clientResponse, token);
                        if (clientResponse.response.StatusCode == (int)HttpStatusCode.Unauthorized)
                            break;

                        using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                        {
                            string requestBody = await reader.ReadToEndAsync();
                            Package package = JsonConvert.DeserializeObject<Package>(requestBody);
                            UserRepository.CreatePackages(package);
                        }

                        clientResponse.responseString = "Package created successfully.";
                    }
                    break;

                case "/transactions/packages": //Random Package
                    if(method == "POST")
                    {
                        string authHeader = request.Headers["Authorization"];
                        string token = authHeader.Replace("Bearer ", string.Empty);

                        clientResponse = IsValidToken(request, clientResponse, token);
                        if (clientResponse.response.StatusCode == (int)HttpStatusCode.Unauthorized)
                            break;

                        using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                        {
                            string requestBody = await reader.ReadToEndAsync();
                            //string packageKey = JsonConvert.DeserializeObject<Package>(requestBody);
                            if (!UserRepository.PurchasePackage(ExtractUsernameFromToken(token), packageKey))
                                clientResponse.responseString = "User successfully purchased Package";
                            else
                            {
                                clientResponse.responseString = "Not enought Coins";
                                clientResponse.response.StatusCode = (int)HttpStatusCode.PaymentRequired; 
                            }
                        }
                    }
                    break;


                case "/cards": //Tetsing TODO
                    if (method == "POST")
                    {
                        string authHeader = request.Headers["Authorization"];
                        string token = authHeader.Replace("Bearer ", string.Empty);

                        clientResponse = IsValidToken(request, clientResponse, token);
                        if (clientResponse.response.StatusCode == (int)HttpStatusCode.Unauthorized)
                            break;

                        List<Card> stack = UserRepository.GetUserStack(ExtractUsernameFromToken(token));
                        string stackJson = JsonConvert.SerializeObject(stack);

                        clientResponse.responseString = stackJson;
                        clientResponse.response.ContentType = "application/json";
                    }
                    break;

                case "/deck":
                    if (method == "POST")
                    {
                        string authHeader = request.Headers["Authorization"];
                        string token = authHeader.Replace("Bearer ", string.Empty);

                        clientResponse = IsValidToken(request, clientResponse, token);
                        if (clientResponse.response.StatusCode == (int)HttpStatusCode.Unauthorized)
                            break;

                        //GET username from Token -> return all Cards in deck
                    }
                    break;

                case "/stats":
                    if (method == "POST")
                    {
                        string authHeader = request.Headers["Authorization"];
                        string token = authHeader.Replace("Bearer ", string.Empty);

                        clientResponse = IsValidToken(request, clientResponse, token);
                        if (clientResponse.response.StatusCode == (int)HttpStatusCode.Unauthorized)
                            break;

                        
                        //GET username from Token -> stats
                    }
                    break;

                case "/scoreboard":
                    if (method == "POST")
                    {
                        string authHeader = request.Headers["Authorization"];
                        string token = authHeader.Replace("Bearer ", string.Empty);

                        clientResponse = IsValidToken(request, clientResponse, token);
                        if (clientResponse.response.StatusCode == (int)HttpStatusCode.Unauthorized)
                            break;

                        //GET username from Token -> scoreboard
                    }
                    break;

                case "/tradings":
                    if (method == "POST")
                    {
                        string authHeader = request.Headers["Authorization"];
                        string token = authHeader.Replace("Bearer ", string.Empty);

                        clientResponse = IsValidToken(request, clientResponse, token);
                        if (clientResponse.response.StatusCode == (int)HttpStatusCode.Unauthorized)
                            break;

                        //GET username from Token -> tradings
                    }
                    break;
                default:
                    clientResponse.responseString = "Invalid endpoint.";
                    clientResponse.response.StatusCode = (int)HttpStatusCode.NotFound;
                    break;
            }

            await Console.Out.WriteLineAsync($"Send to Client: {clientResponse.responseString}");
            byte[] buffer = Encoding.UTF8.GetBytes(clientResponse.responseString);
            clientResponse.response.ContentLength64 = buffer.Length;

            using (var output = clientResponse.response.OutputStream)
            {
                await output.WriteAsync(buffer, 0, buffer.Length);
            }

            Seed.PrintTableContents();
            clientResponse.response.Close();
        }

        public static bool ValidateUserCredentials(string username, string password)
        {
            User user = UserRepository.GetUserByUsername(username);
            if (user.Username == username && password == user.Password)
            {
                return true;
            }
            return false;
        }

        static string GenerateToken(string username)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("04qYL5KCcxlUsM1zlbkQ77jg1HvvIBz14vUTgzJfDMU="));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "if22b009",
                audience: "technikumwien",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static ClientResponse IsValidToken(HttpListenerRequest request, ClientResponse cr, string token)
        {
            if (request.Headers["Authorization"] == null)
            {
                cr.responseString = "Unauthorized: Missing Authorization header.";
                cr.response.StatusCode = (int)HttpStatusCode.Unauthorized;
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("04qYL5KCcxlUsM1zlbkQ77jg1HvvIBz14vUTgzJfDMU="));
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuer = "if22b009",
                ValidAudience = "technikumwien",
                IssuerSigningKey = key
            };


            bool ValidToken;
            try
            {
                var handler = new JwtSecurityTokenHandler();
                handler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);
                ValidToken = true;
            }
            catch (SecurityTokenMalformedException)
            {
                ValidToken = false;
            }

            if (!ValidToken)
            {
                cr.responseString = "Unauthorized: Invalid token.";
                cr.response.StatusCode = (int)HttpStatusCode.Unauthorized;
            }

            return cr;
        }

        private static string ExtractUsernameFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;
            return jwtToken?.Claims?.FirstOrDefault(c => c.Type == "username")?.Value;
        }

    }
}
