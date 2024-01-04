using MTCG.Database;
using MTCG.Model;
using MTCG.Repositorys;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using MTCG.Battling;

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
                case var path when path.StartsWith("/users", StringComparison.OrdinalIgnoreCase):
                    await ProcessUserRequest(request, method, clientResponse);
                    break;

                case "/sessions":
                    await ProcessSessionRequest(request, method, clientResponse);
                    break;

                case "/packages":
                    await ProcessPackageRequest(request, method, clientResponse);
                    break;

                case "/transactions/packages":
                    await ProcessTransactionRequest(request, method, clientResponse);
                    break;

                case "/cards":
                    await ProcessCardsRequest(request, method, clientResponse);
                    break;

                case "/deck":
                    await ProcessDeckRequest(request, method, clientResponse);
                    break;

                case "/stats":
                    await ProcessStatsRequest(request, method, clientResponse);
                    break;

                case "/scoreboard":
                    await ProcessScoreboardRequest(request, method, clientResponse);
                    break;

                case "/battles":
                    await ProcessBattlesRequest(request, method, clientResponse);
                    break;

                case var path when path.StartsWith("/tradings", StringComparison.OrdinalIgnoreCase):
                    await ProcessTradingsRequest(request, method, clientResponse);
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
            clientResponse.response.Close();
        }

        private static async Task ProcessUserRequest(HttpListenerRequest request, string method, ClientResponse clientResponse)
        {
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
                string authHeader = request.Headers["Authorization"];
                string token = authHeader.Replace("Bearer ", string.Empty);

                clientResponse = IsValidToken(request, clientResponse, token);
                if (clientResponse.response.StatusCode == (int)HttpStatusCode.Unauthorized)
                    return;

                var segments = request.Url.Segments;

                if (segments.Length >= 3)
                {
                    string username = segments[2].Trim('/');
                    User user = UserRepository.GetUserInfoByUsername(username);                
                    if (user != null)
                    {
                        
                        if (user.Username != GetUserNameFromToken(token))
                        {
                            clientResponse.responseString = "Unauthorized User access.";
                            clientResponse.response.StatusCode = (int)HttpStatusCode.Unauthorized;
                            return;
                        }

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
                string authHeader = request.Headers["Authorization"];
                string token = authHeader.Replace("Bearer ", string.Empty);

                clientResponse = IsValidToken(request, clientResponse, token);
                if (clientResponse.response.StatusCode == (int)HttpStatusCode.Unauthorized)
                    return;

                string username = request.Url.Segments.Last();

                using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                {
                    string requestBody = await reader.ReadToEndAsync();
                    dynamic userData = JsonConvert.DeserializeObject(requestBody);

                    User user = UserRepository.GetUserByUsername(username);

                    if (user != null)
                    {
                        if (user.Username != GetUserNameFromToken(token))
                        {
                            clientResponse.responseString = "Unauthorized User access.";
                            clientResponse.response.StatusCode = (int)HttpStatusCode.Unauthorized;
                            return;
                        }

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
        }

        private static async Task ProcessSessionRequest(HttpListenerRequest request, string method, ClientResponse clientResponse)
        {
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
        }

        private static async Task ProcessPackageRequest(HttpListenerRequest request, string method, ClientResponse clientResponse)
        {
            if (method == "POST")
            {
                string authHeader = request.Headers["Authorization"];
                string token = authHeader.Replace("Bearer ", string.Empty);

                clientResponse = IsValidToken(request, clientResponse, token);
                if (clientResponse.response.StatusCode == (int)HttpStatusCode.Unauthorized)
                    return;

                using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                {
                    string requestBody = await reader.ReadToEndAsync();
                    Package package = JsonConvert.DeserializeObject<Package>(requestBody);
                    PackageRepository.CreatePackages(package);
                }

                clientResponse.responseString = "Package created successfully.";
            }
        } 

        private static async Task ProcessTransactionRequest(HttpListenerRequest request, string method, ClientResponse clientResponse)
        {
            if (method == "POST")
            {
                string authHeader = request.Headers["Authorization"];
                string token = authHeader.Replace("Bearer ", string.Empty);

                clientResponse = IsValidToken(request, clientResponse, token);
                if (clientResponse.response.StatusCode == (int)HttpStatusCode.Unauthorized)
                    return;

                string uname = GetUserNameFromToken(token);
                if (PackageRepository.PurchasePackage(uname))
                {
                    clientResponse.responseString = "User successfully purchased Package";
                }
                else
                {
                    clientResponse.responseString = "Not enought Coins";
                    clientResponse.response.StatusCode = (int)HttpStatusCode.PaymentRequired;
                }
            }
        }

        private static async Task ProcessCardsRequest(HttpListenerRequest request, string method, ClientResponse clientResponse)
        {
            if (method == "GET")
            {

                string authHeader = request.Headers["Authorization"];

                if (authHeader == null)
                {
                    clientResponse.responseString = "No authentication Header";
                    clientResponse.response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return;
                }

                string token = authHeader.Replace("Bearer ", string.Empty);

                clientResponse = IsValidToken(request, clientResponse, token);
                if (clientResponse.response.StatusCode == (int)HttpStatusCode.Unauthorized)
                    return;

                List<Card> stack = StackRepository.GetUserStack(GetUserNameFromToken(token));
                string stackJson = JsonConvert.SerializeObject(stack);

                clientResponse.responseString = stackJson;
                clientResponse.response.ContentType = "application/json";
            }
        }

        private static async Task ProcessDeckRequest(HttpListenerRequest request, string method, ClientResponse clientResponse)
        {
            string authHeader = request.Headers["Authorization"];
            string token = authHeader.Replace("Bearer ", string.Empty);

            clientResponse = IsValidToken(request, clientResponse, token);
            if (clientResponse.response.StatusCode == (int)HttpStatusCode.Unauthorized)
                return;

            if (method == "GET")
            {
                string queryString = request.Url.Query;
                var queryParameters = System.Web.HttpUtility.ParseQueryString(queryString);

                string format = queryParameters["format"];

                List<Card> deck = DeckRepository.GetUserDeck(GetUserNameFromToken(token));

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

                        if (user != null && StackRepository.AreCardsInUserStack(user.UserID, cardIds))
                        {
                            DeckRepository.ClearUserDeck(user.UserID);
                            DeckRepository.AddCardToUserDeck(user.UserID, cardIds);
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
        }

        private static async Task ProcessStatsRequest(HttpListenerRequest request, string method, ClientResponse clientResponse)
        {
            if (method == "GET")
            {
                string authHeader = request.Headers["Authorization"];
                string token = authHeader.Replace("Bearer ", string.Empty);

                clientResponse = IsValidToken(request, clientResponse, token);
                if (clientResponse.response.StatusCode == (int)HttpStatusCode.Unauthorized)
                    return;

                string username = GetUserNameFromToken(token);
                User user = UserRepository.GetUserByUsername(username);

                if (user != null)
                {
                    int totalGames = StatsRepository.GetTotalGames(user.UserID);
                    int gamesWon = StatsRepository.GetGamesWon(user.UserID);
                    int gamesLost = StatsRepository.GetGamesLost(user.UserID);
                    int gamesDraw = StatsRepository.GetGamesDrawn(user.UserID);
                    int spentCoins = StatsRepository.GetTotalSpentCoins(username);

                    double winPercentage = totalGames > 0 ? ((double)gamesWon / totalGames) * 100 : 0;
                    var responseObj = new
                    {
                        user.Elo,
                        user.Coins,
                        TotalGames = totalGames,
                        GamesWon = gamesWon,
                        GamesLost = gamesLost,
                        GamesDraw = gamesDraw,
                        WinPercentage = winPercentage,
                        SpentCoins = spentCoins
                    };
                    string jsonResponse = JsonConvert.SerializeObject(responseObj);
                    clientResponse.responseString = jsonResponse;
                    clientResponse.response.ContentType = "application/json";
                }
            }
        }

        private static async Task ProcessScoreboardRequest(HttpListenerRequest request, string method, ClientResponse clientResponse)
        {
            if (method == "GET")
            {
                string authHeader = request.Headers["Authorization"];
                string token = authHeader.Replace("Bearer ", string.Empty);

                clientResponse = IsValidToken(request, clientResponse, token);
                if (clientResponse.response.StatusCode == (int)HttpStatusCode.Unauthorized)
                    return;

                List<UserScoreboardEntry> scoreboard = StatsRepository.GetScoreboard();

                string jsonResponse = JsonConvert.SerializeObject(scoreboard);
                clientResponse.responseString = jsonResponse;
                clientResponse.response.ContentType = "application/json";
            }
        }
        public static async Task ProcessBattlesRequest(HttpListenerRequest request, string method, ClientResponse clientResponse)
        {
            if (method == "POST")
            {
                string authHeader = request.Headers["Authorization"];
                string token = authHeader.Replace("Bearer ", string.Empty);

                clientResponse = IsValidToken(request, clientResponse, token);
                if (clientResponse.response.StatusCode == (int)HttpStatusCode.Unauthorized)
                    return;

                string username = GetUserNameFromToken(token);
                User user = UserRepository.GetUserByUsername(username);
                User opponent = UserRepository.GetRandomOpponent(user);
                
                Battle battle = new Battle();
                BattleLog log = battle.StartBattle(user, opponent);
                StatsRepository.InsertBattleLog(log);
                var responseObj = new
                {
                    log.WinnerID,
                    log.LoserID,
                    log.Draw,
                    log.Rounds
                };
                string jsonResponse = JsonConvert.SerializeObject(responseObj);
                clientResponse.responseString = jsonResponse;
                clientResponse.response.ContentType = "application/json";
            }
        }

        private static async Task ProcessTradingsRequest(HttpListenerRequest request, string method, ClientResponse clientResponse)
        {
            if (method == "GET") 
            {
                string authHeader = request.Headers["Authorization"];
                string token = authHeader.Replace("Bearer ", string.Empty);

                clientResponse = IsValidToken(request, clientResponse, token);
                if (clientResponse.response.StatusCode == (int)HttpStatusCode.Unauthorized)
                    return;

                List<TradeRequirement> tradeRequirements = TradingRepository.GetAllTrades();
                string TradsJson = JsonConvert.SerializeObject(tradeRequirements);
                clientResponse.responseString = TradsJson;
                clientResponse.response.ContentType = "application/json";
            }
            else if (method == "POST")
            {

                string authHeader = request.Headers["Authorization"];
                string token = authHeader.Replace("Bearer ", string.Empty);

                clientResponse = IsValidToken(request, clientResponse, token);
                if (clientResponse.response.StatusCode == (int)HttpStatusCode.Unauthorized)
                    return;

                string username = GetUserNameFromToken(token);
                User user = UserRepository.GetUserByUsername(username);

                var segments = request.Url.Segments;
                if (segments.Length < 3)
                {
                    // Normal POST request without trade ID in the URL
                    using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                    {
                        string requestBody = await reader.ReadToEndAsync();
                        TradeRequirement t = JsonConvert.DeserializeObject<TradeRequirement>(requestBody);
                        t.UsersID = user.UserID;
                        if (DeckRepository.IsCardInUserDeck(t.UsersID, t.CardID))
                        {
                            clientResponse.responseString = "Card to Trade is Currently in Use in Deck";
                            clientResponse.response.StatusCode = (int)HttpStatusCode.Conflict;
                        }
                        else if (CardRepository.IsCardTrading(t.CardID))
                        {
                            clientResponse.responseString = "Card to Trade is already Trading";
                            clientResponse.response.StatusCode = (int)HttpStatusCode.Conflict;
                        }
                        else
                        {
                            TradingRepository.CreateTrade(t);
                            CardRepository.UpdateCardTradingStatus(t.UsersID, t.CardID, true);
                            clientResponse.responseString = "Trade created successfully.";
                        }
                    }
                }
                else
                {
                    // POST request with trade ID in the URL
                    int tradeID;
                    if (int.TryParse(segments[2].Trim('/'), out tradeID))
                    {
                        using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                        {
                            string requestBody = await reader.ReadToEndAsync();
                            int acceptingCardID;
                            if (!int.TryParse(requestBody.Trim('"'), out acceptingCardID))
                            {
                                clientResponse.responseString = "Invalid card ID format.";
                                clientResponse.response.StatusCode = (int)HttpStatusCode.BadRequest;
                                return;
                            }

                            TradeRequirement tradeOffer = TradingRepository.GetTradebyTradID(tradeID);
                            // Check if the trade exists
                            if (tradeOffer == null)
                            {
                                clientResponse.responseString = "Trade not found.";
                                clientResponse.response.StatusCode = (int)HttpStatusCode.NotFound;
                            }
                            // Check if the trader is not the same person as the trade acceptor
                            else if (tradeOffer.UsersID == user.UserID)
                            {
                                clientResponse.responseString = "You cannot accept your own trade offer.";
                                clientResponse.response.StatusCode = (int)HttpStatusCode.BadRequest;
                            }
                            // Check if user has this card
                            
                            else if (!StackRepository.AreCardsInUserStack(user.UserID, new List<int> { acceptingCardID }))
                            {
                                clientResponse.responseString = "You dont own the Card";
                                clientResponse.response.StatusCode = (int)HttpStatusCode.BadRequest;
                            }
                            else if (DeckRepository.IsCardInUserDeck(user.UserID, acceptingCardID))
                            {
                                clientResponse.responseString = "Card to Trade is Currently in Use in Deck";
                                clientResponse.response.StatusCode = (int)HttpStatusCode.Conflict;
                            }
                            else if (CardRepository.IsCardTrading(acceptingCardID))
                            {
                                clientResponse.responseString = "Card to Trade is already Trading";
                                clientResponse.response.StatusCode = (int)HttpStatusCode.Conflict;
                            }
                            // Check if the card has all the requirements
                            else if (!CardRepository.IsCardEligibleForTrade(acceptingCardID, tradeOffer.CardRegion, tradeOffer.CardType, tradeOffer.MinimumDamage))
                            {
                                clientResponse.responseString = "The provided card does not meet the trade requirements.";
                                clientResponse.response.StatusCode = (int)HttpStatusCode.BadRequest;
                            }
                            else
                            {
                                // Process the trade acceptance
                                TradingRepository.AcceptTrade(tradeID, acceptingCardID, user.UserID);
                                clientResponse.responseString = "Trade accepted successfully.";
                            }
                        }
                    }
                    else
                    {
                        clientResponse.responseString = "Invalid trade ID format.";
                        clientResponse.response.StatusCode = (int)HttpStatusCode.BadRequest;
                    }
                }
            }
            else if (method == "DELETE")
            {
                string authHeader = request.Headers["Authorization"];
                string token = authHeader.Replace("Bearer ", string.Empty);

                clientResponse = IsValidToken(request, clientResponse, token);
                if (clientResponse.response.StatusCode == (int)HttpStatusCode.Unauthorized)
                    return;

                var segments = request.Url.Segments;

                if (segments.Length >= 3)
                {
                    int tradesid = Convert.ToInt32(segments[2].Trim('/'));
                    TradeRequirement t = TradingRepository.GetTradebyTradID(tradesid);

                    if (t != null)
                    {
                        CardRepository.UpdateCardTradingStatus(t.UsersID, t.CardID, false);
                        TradingRepository.DeleteTradeById(t.TradesID);                    
                        clientResponse.responseString = $"Trade with ID: {tradesid} Deleted.";
                        clientResponse.response.StatusCode = (int)HttpStatusCode.OK;
                    }
                    else
                    {
                        clientResponse.responseString = "Trade not found.";
                        clientResponse.response.StatusCode = (int)HttpStatusCode.NotFound;
                    }
                }
                else
                {
                    clientResponse.responseString = "Invalid endpoint. Please provide a valid TradeID.";
                    clientResponse.response.StatusCode = (int)HttpStatusCode.BadRequest;
                }
            }
        }
        

        private static bool ValidateUserCredentials(string username, string password)
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
            string token = $"{username}-mtcgToken";
            return token;
        }

        private static ClientResponse IsValidToken(HttpListenerRequest request, ClientResponse cr, string token)
        {
            if (string.IsNullOrEmpty(request.Headers["Authorization"]))
            {
                cr.responseString = "Unauthorized: Missing Authorization header.";
                cr.response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return cr;
            }

            string headerToken = request.Headers["Authorization"].Replace("Bearer ", "");

            if (headerToken != $"{GetUserNameFromToken(token)}-mtcgToken")
            {
                cr.responseString = "Unauthorized: Invalid token.";
                cr.response.StatusCode = (int)HttpStatusCode.Unauthorized;
            }

            return cr;
        }

        static string GetUserNameFromToken(string tokenString)
        {
            if (tokenString.EndsWith("-mtcgToken"))
            {
                return tokenString.Substring(0, tokenString.Length - "-mtcgToken".Length);
            }
            return null;
        }
    }
}
