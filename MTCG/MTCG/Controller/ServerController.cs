using Microsoft.IdentityModel.Tokens;
using MTCG.Database;
using MTCG.Model;
using MTCG.Repos;
using Newtonsoft.Json;
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
                case var path when path.StartsWith("/users", StringComparison.OrdinalIgnoreCase):
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
                    else if (method == "GET")
                    {
                        var segments = request.Url.Segments;

                        if (segments.Length >= 3)
                        {
                            string username = segments[2].Trim('/');
                            User user = UserRepository.GetUserInfoByUsername(username);

                            if (user != null)
                            {
                                var userInfo = new
                                {
                                    user.Username,
                                    user.Password,
                                    user.Bio,
                                    user.Image
                                };

                                string userInfoJson = JsonConvert.SerializeObject(userInfo);
                                clientResponse.responseString = userInfoJson;
                                clientResponse.response.ContentType = "application/json";
                            }
                            else
                            {
                                clientResponse.responseString = "User not found.";
                                clientResponse.response.StatusCode = (int)HttpStatusCode.NotFound;
                            }
                        }
                        else
                        {
                            clientResponse.responseString = "Invalid endpoint. Please provide a valid username.";
                            clientResponse.response.StatusCode = (int)HttpStatusCode.BadRequest;
                        }
                    }

                    else if (method == "PUT")
                    {
                        authHeader = request.Headers["Authorization"];
                        token = authHeader.Replace("Bearer ", string.Empty);

                        clientResponse = IsValidToken(request, clientResponse, token);
                        if (clientResponse.response.StatusCode == (int)HttpStatusCode.Unauthorized)
                            break;

                        string username = request.Url.Segments.Last();

                        using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                        {
                            string requestBody = await reader.ReadToEndAsync();
                            dynamic userData = JsonConvert.DeserializeObject(requestBody);

                            User user = UserRepository.GetUserByUsername(username);

                            if (user != null)
                            {
                                if (userData.Username != null)
                                {
                                    user.Username = userData.Username;
                                }

                                if (userData.Password != null)
                                {
                                    user.Password = userData.Password;
                                }

                                if (userData.Bio != null)
                                {
                                    user.Bio = userData.Bio;
                                }

                                if (userData.Image != null)
                                {
                                    user.Image = userData.Image;
                                }

                                UserRepository.UpdateUser(user);
                                var JWTtoken = GenerateToken(user.Username);
                                clientResponse.responseString = "User updated successfully.";                           
                                clientResponse.response.AddHeader("Authorization", $"Bearer {JWTtoken}");
                            }
                            else
                            {
                                clientResponse.responseString = "User not found.";
                                clientResponse.response.StatusCode = (int)HttpStatusCode.NotFound;
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

                case "/deck":
                    authHeader = request.Headers["Authorization"];
                    token = authHeader.Replace("Bearer ", string.Empty);

                    clientResponse = IsValidToken(request, clientResponse, token);
                    if (clientResponse.response.StatusCode == (int)HttpStatusCode.Unauthorized)
                        break;

                    if (method == "GET")
                    {
                        string queryString = request.Url.Query;
                        var queryParameters = System.Web.HttpUtility.ParseQueryString(queryString);

                        string format = queryParameters["format"];

                        List<Card> deck = UserRepository.GetUserDeck(GetUserNameFromToken(token));

                        if (format == "plain")
                        {
                            string plainTextRepresentation = GeneratePlainTextRepresentation(deck);
                            clientResponse.responseString = plainTextRepresentation;
                            clientResponse.response.ContentType = "text/plain";
                        }
                        else
                        {
                            string deckJson = JsonConvert.SerializeObject(deck);
                            clientResponse.responseString = deckJson;
                            clientResponse.response.ContentType = "application/json";
                        }
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

                                if (user != null && UserRepository.AreCardsInUserStack(user.UserID, cardIds))
                                {
                                    UserRepository.ClearUserDeck(user.UserID);
                                    UserRepository.AddCardToUserDeck(user.UserID, cardIds);
                                    clientResponse.responseString = "Deck configured successfully.";
                                }
                                else
                                {
                                    clientResponse.responseString = "Invalid card IDs. Only use Card which are in your stack and are currently not in Trading.";
                                    clientResponse.response.StatusCode = (int)HttpStatusCode.BadRequest;
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
                    if (method == "GET")
                    {
                        authHeader = request.Headers["Authorization"];
                        token = authHeader.Replace("Bearer ", string.Empty);

                        clientResponse = IsValidToken(request, clientResponse, token);
                        if (clientResponse.response.StatusCode == (int)HttpStatusCode.Unauthorized)
                            break;

                        string username = GetUserNameFromToken(token);
                        User user = UserRepository.GetUserByUsername(username);

                        if (user != null)
                        {
                            int totalGames = UserRepository.GetTotalGames(username);
                            int gamesWon = UserRepository.GetGamesWon(username);
                            int gamesLost = UserRepository.GetGamesLost(username);
                            int spentCoins = UserRepository.GetTotalSpentCoins(username);

                            double winPercentage = totalGames > 0 ? ((double)gamesWon / totalGames) * 100 : 0;
                            var responseObj = new
                            {                             
                                user.Elo,
                                user.Coins,
                                TotalGames = totalGames,
                                GamesWon = gamesWon,
                                GamesLost = gamesLost,
                                WinPercentage = winPercentage,
                                SpentCoins = spentCoins
                            };
                            string jsonResponse = JsonConvert.SerializeObject(responseObj);
                            clientResponse.responseString = jsonResponse;
                            clientResponse.response.ContentType = "application/json";
                        }
                    }
                    break;


                case "/scoreboard":
                    if (method == "GET")
                    {
                        authHeader = request.Headers["Authorization"];
                        token = authHeader.Replace("Bearer ", string.Empty);

                        clientResponse = IsValidToken(request, clientResponse, token);
                        if (clientResponse.response.StatusCode == (int)HttpStatusCode.Unauthorized)
                            break;

                        List<UserScoreboardEntry> scoreboard = UserRepository.GetScoreboard();

                        string jsonResponse = JsonConvert.SerializeObject(scoreboard);
                        clientResponse.responseString = jsonResponse;
                        clientResponse.response.ContentType = "application/json";
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

                        string username = GetUserNameFromToken(token);
                    }
                    break;

                case "/battles":
                    if (method == "POST")
                    {
                        authHeader = request.Headers["Authorization"];
                        token = authHeader.Replace("Bearer ", string.Empty);

                        clientResponse = IsValidToken(request, clientResponse, token);
                        if (clientResponse.response.StatusCode == (int)HttpStatusCode.Unauthorized)
                            break;

                        string username = GetUserNameFromToken(token);
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
        private static string GeneratePlainTextRepresentation(List<Card> deck)
        {
            StringBuilder plainText = new StringBuilder();

            plainText.AppendLine("Deck:");

            foreach (Card card in deck)
            {
                plainText.AppendLine($"\t{card.Name}, Damage: {card.Damage}, Region: {card.Region}, Type: {card.Type}");
            }

            return plainText.ToString();
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
