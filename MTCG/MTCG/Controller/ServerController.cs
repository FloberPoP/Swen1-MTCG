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

            string authHeader = "";
            string token = "";

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

                            bool isValidUser = ValidateUserCredentials(user.Username, user.Password);

                            if (UserRepository.GetUserByUsername(user.Username) != null && isValidUser)
                            {
                                var JWTtoken = GenerateToken(user.Username);
                                clientResponse.response.AddHeader("Authorization", $"Bearer {JWTtoken}");
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
                        authHeader = request.Headers["Authorization"];
                        token = authHeader.Replace("Bearer ", string.Empty);

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

                case "/transactions/packages":
                    if(method == "POST")
                    {
                        authHeader = request.Headers["Authorization"];
                        token = authHeader.Replace("Bearer ", string.Empty);

                        clientResponse = IsValidToken(request, clientResponse, token);
                        if (clientResponse.response.StatusCode == (int)HttpStatusCode.Unauthorized)
                            break;

                        string uname = GetUserNameFromToken(token);
                        if (UserRepository.PurchasePackage(uname))
                        {
                            clientResponse.responseString = "User successfully purchased Package";
                        }
                        else
                        {
                            clientResponse.responseString = "Not enought Coins";
                            clientResponse.response.StatusCode = (int)HttpStatusCode.PaymentRequired; 
                        }
                    }
                    break;


                case "/cards":
                    if (method == "GET")
                    {
                        
                        authHeader = request.Headers["Authorization"];

                        if (authHeader == null)
                        {
                            clientResponse.responseString = "No authentication Header";
                            clientResponse.response.StatusCode = (int)HttpStatusCode.BadRequest;
                            break;
                        }
                  
                        token = authHeader.Replace("Bearer ", string.Empty);

                        clientResponse = IsValidToken(request, clientResponse, token);
                        if (clientResponse.response.StatusCode == (int)HttpStatusCode.Unauthorized)
                            break;

                        List<Card> stack = UserRepository.GetUserStack(GetUserNameFromToken(token));
                        string stackJson = JsonConvert.SerializeObject(stack);

                        clientResponse.responseString = stackJson;
                        clientResponse.response.ContentType = "application/json";
                    }
                    break;

                case "/deck": //Testing TODO
                    authHeader = request.Headers["Authorization"];
                    token = authHeader.Replace("Bearer ", string.Empty);

                    clientResponse = IsValidToken(request, clientResponse, token);
                    if (clientResponse.response.StatusCode == (int)HttpStatusCode.Unauthorized)
                        break;

                    if (method == "GET")
                    {
                        List<Card> deck = UserRepository.GetUserDeck(GetUserNameFromToken(token));
                        string deckJson = JsonConvert.SerializeObject(deck);

                        clientResponse.responseString = deckJson;
                        clientResponse.response.ContentType = "application/json";
                    }
                    else if (method == "PUT")
                    {
                        using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                        {
                            string requestBody = reader.ReadToEnd();
                            List<int> cardIds = JsonConvert.DeserializeObject<List<int>>(requestBody);

                            if (cardIds.Count == 4)
                            {
                                string username = GetUserNameFromToken(token);
                                User user = UserRepository.GetUserByUsername(username);
                                //Check if Card ids are in Stack
                                if (user != null)
                                {
                                    UserRepository.ClearUserDeck(user.UserID);
                                    UserRepository.AddCardToUserDeck(user.UserID, cardIds);
                                    clientResponse.responseString = "Deck configured successfully.";
                                }
                                else
                                {
                                    clientResponse.responseString = "User not found.";
                                    clientResponse.response.StatusCode = (int)HttpStatusCode.NotFound;
                                }
                            }
                            else
                            {
                                clientResponse.responseString = "Invalid number of cards. Please provide exactly 4 cards.";
                                clientResponse.response.StatusCode = (int)HttpStatusCode.BadRequest;
                            }
                        }
                    }
                    break;


                case "/stats":
                    if (method == "POST")
                    {
                        authHeader = request.Headers["Authorization"];
                        token = authHeader.Replace("Bearer ", string.Empty);

                        clientResponse = IsValidToken(request, clientResponse, token);
                        if (clientResponse.response.StatusCode == (int)HttpStatusCode.Unauthorized)
                            break;
                        
                        //GET username from Token -> stats
                    }
                    break;

                case "/scoreboard":
                    if (method == "POST")
                    {
                        authHeader = request.Headers["Authorization"];
                        token = authHeader.Replace("Bearer ", string.Empty);

                        clientResponse = IsValidToken(request, clientResponse, token);
                        if (clientResponse.response.StatusCode == (int)HttpStatusCode.Unauthorized)
                            break;

                        //GET username from Token -> scoreboard
                    }
                    break;

                case "/tradings":
                    if (method == "POST")
                    {
                        authHeader = request.Headers["Authorization"];
                        token = authHeader.Replace("Bearer ", string.Empty);

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
            if (user != null && user.Username == username && password == user.Password)
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

        static string GetUserNameFromToken(string tokenString)
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(tokenString) as JwtSecurityToken;

            if (jsonToken != null)
            {
                return jsonToken?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            }
            return null;
        }
    }
}
