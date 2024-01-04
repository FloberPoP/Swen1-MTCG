using MTCG.Battling;
using MTCG.Database;
using MTCG.Model;
using MTCG.Repositorys;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MTCG.Controller
{
    public class SocketServer
    {
        private Socket serverSocket;

        public SocketServer(string ipAddress, int port)
        {
            IPAddress ip = IPAddress.Parse(ipAddress);
            IPEndPoint localEndPoint = new IPEndPoint(ip, port);

            serverSocket = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(localEndPoint);
            serverSocket.Listen(10);
        }

        public void Start()
        {
            Console.WriteLine($"Server listening on {serverSocket.LocalEndPoint}");

            while (true)
            {
                Socket clientSocket = serverSocket.Accept();
                HandleClient(clientSocket);
            }
        }

        private void HandleClient(Socket clientSocket)
        {
            using (NetworkStream networkStream = new NetworkStream(clientSocket))
            {
                byte[] buffer = new byte[4096];

                int bytesRead;
                while (clientSocket.Connected && networkStream.CanRead && (bytesRead = networkStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    ClientResponse clientResponse = new ClientResponse();
                    ProcessRequest(request, clientResponse);
                    SendResponse(clientResponse, networkStream);
                }
            }

            clientSocket.Close();
        }



        private void SendResponse(ClientResponse clientResponse, NetworkStream networkStream)
        {
            try
            {
                if(clientResponse.StatusCode == 0)
                    clientResponse.StatusCode = (int)HttpStatusCode.OK;

                string response = $"HTTP/1.1 {clientResponse.StatusCode}\r\nContent-Type: text/plain\r\n\r\n\"{clientResponse.ResponseString}\"\r\n";
                Console.WriteLine($"SEND to Client:\n{response}");

                byte[] responseBuffer = Encoding.UTF8.GetBytes(response);
                networkStream.Write(responseBuffer, 0, responseBuffer.Length);
                networkStream.Flush();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending response: {ex.Message}");
            }
            finally
            {
                networkStream.Close();
                Seed.PrintTableContents();
            }
        }


        private void ProcessRequest(string request, ClientResponse clientResponse)
        {
            // Parse the request to extract method and path
            string[] requestLines = request.Split('\n');
            string[] requestTokens = requestLines[0].Split(' ');

            if (requestTokens.Length < 3)
            {
                // Invalid request format
                clientResponse.ResponseString = "Invalid request format.";
                clientResponse.NetworkStream.Close(); // Close the connection
                return;
            }

            string method = requestTokens[0];
            string path = requestTokens[1];

            switch (path.ToLower())
            {
                case var p when p.StartsWith("/users", StringComparison.OrdinalIgnoreCase):
                    ProcessUserRequest(request, method, clientResponse);
                    break;

                case "/sessions":
                    ProcessSessionRequest(request, method, clientResponse);
                    break;

                case "/packages":
                    ProcessPackageRequest(request, method, clientResponse);
                    break;

                case "/transactions/packages":
                    ProcessTransactionRequest(request, method, clientResponse);
                    break;

                case "/cards":
                    ProcessCardsRequest(request, method, clientResponse);
                    break;

                case var p when p.StartsWith("/deck", StringComparison.OrdinalIgnoreCase):
                    ProcessDeckRequest(request, method, clientResponse);
                    break;

                case "/stats":
                    ProcessStatsRequest(request, method, clientResponse);
                    break;

                case "/scoreboard":
                    ProcessScoreboardRequest(request, method, clientResponse);
                    break;

                case "/battles":
                    ProcessBattlesRequest(request, method, clientResponse);
                    break;

                case var p when p.StartsWith("/tradings", StringComparison.OrdinalIgnoreCase):
                    ProcessTradingsRequest(request, method, clientResponse);
                    break;

                default:
                    clientResponse.ResponseString = "Invalid endpoint.";
                    return;
            }            
        }

        public void ProcessUserRequest(string request, string method, ClientResponse clientResponse)
        {
            if (method == "POST")
            {
                User user = JsonConvert.DeserializeObject<User>(ServerController.GetJasonPart(request));
                if (UserRepository.GetUserByUsername(user.Username) == null)
                {
                    UserRepository.CreateUser(user);
                    clientResponse.ResponseString = "User registered successfully.";
                }
                else
                {
                    clientResponse.ResponseString = "Username already taken.";
                    clientResponse.StatusCode = (int)HttpStatusCode.Conflict;
                }
            }
            else if (method == "GET")
            {
                string authHeader = ServerController.GetAuthorizationHeader(request);
                string token = authHeader.Replace("Bearer ", string.Empty);

                clientResponse = IsValidToken(request, clientResponse, token);
                if (clientResponse.StatusCode == (int)HttpStatusCode.Unauthorized)
                    return;

                var segments = ServerController.GetUrlSegments(request);

                if (segments.Length >= 3)
                {
                    string username = segments[2].Trim('/');
                    User user = UserRepository.GetUserInfoByUsername(username);

                    if (user != null)
                    {
                        if (user.Username != ServerController.GetUserNameFromToken(token))
                        {
                            clientResponse.ResponseString = "Unauthorized User access.";
                            clientResponse.StatusCode = (int)HttpStatusCode.Unauthorized;
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
                        clientResponse.ResponseString = userInfoJson;
                        clientResponse.ResponseHeaders.Add("Content-Type", "application/json");
                    }
                    else
                    {
                        clientResponse.ResponseString = "User not found.";
                        clientResponse.StatusCode = (int)HttpStatusCode.NotFound;
                    }
                }
                else
                {
                    clientResponse.ResponseString = "Invalid endpoint. Please provide a valid username.";
                    clientResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                }
            }
            else if (method == "PUT")
            {
                string authHeader = ServerController.GetAuthorizationHeader(request);
                string token = authHeader.Replace("Bearer ", string.Empty);

                clientResponse = IsValidToken(request, clientResponse, token);
                if (clientResponse.StatusCode == (int)HttpStatusCode.Unauthorized)
                    return;

                string username = ServerController.GetUrlSegments(request).Last();
                JObject requestBodyJson = JsonConvert.DeserializeObject<JObject>(ServerController.GetJasonPart(request));

                User user = UserRepository.GetUserByUsername(username);

                if (user != null)
                {
                    if (user.Username != ServerController.GetUserNameFromToken(token))
                    {
                        clientResponse.ResponseString = "Unauthorized User access.";
                        clientResponse.StatusCode = (int)HttpStatusCode.Unauthorized;
                        return;
                    }

                    if (requestBodyJson.Value<string>("Username") != null)
                    {
                        user.Username = requestBodyJson.Value<string>("Username");
                    }

                    if (requestBodyJson.Value<string>("Password") != null)
                    {
                        user.Password = requestBodyJson.Value<string>("Password");
                    }

                    if (requestBodyJson.Value<string>("Bio") != null)
                    {
                        user.Bio = requestBodyJson.Value<string>("Bio");
                    }

                    if (requestBodyJson.Value<string>("Image") != null)
                    {
                        user.Image = requestBodyJson.Value<string>("Image");
                    }

                    UserRepository.UpdateUser(user);
                    var JWTtoken = ServerController.GenerateToken(user.Username);
                    clientResponse.ResponseString = "User updated successfully.";
                    clientResponse.ResponseHeaders.Add("Authorization", $"Bearer {JWTtoken}");
                }
                else
                {
                    clientResponse.ResponseString = "User not found.";
                    clientResponse.StatusCode = (int)HttpStatusCode.NotFound;
                }
            }
        }

        public void ProcessSessionRequest(string request, string method, ClientResponse clientResponse)
        {
            if (method == "POST")
            {
                // Assuming that the request body contains the user credentials in JSON format              
                JObject requestBodyJson = JsonConvert.DeserializeObject<JObject>(ServerController.GetJasonPart(request));

                string username = requestBodyJson.Value<string>("Username");
                string password = requestBodyJson.Value<string>("Password");

                bool isValidUser = ServerController.ValidateUserCredentials(username, password);

                if (UserRepository.GetUserByUsername(username) != null && isValidUser)
                {
                    var token = ServerController.GenerateToken(username);
                    clientResponse.ResponseString = "User logged in successfully.";
                    clientResponse.ResponseHeaders.Add("Authorization", $"Bearer {token}");
                }
                else
                {
                    clientResponse.ResponseString = "Invalid username or password for login.";
                    clientResponse.StatusCode = (int)HttpStatusCode.Unauthorized;
                }
            }
            else
            {
                clientResponse.ResponseString = "Invalid HTTP method for this endpoint.";
                clientResponse.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
            }
        }

        public void ProcessPackageRequest(string request, string method, ClientResponse clientResponse)
        {
            if (method == "POST")
            {
                string authHeader = ServerController.GetAuthorizationHeader(request);
                string token = authHeader.Replace("Bearer ", string.Empty);

                clientResponse = IsValidToken(request, clientResponse, token);
                if (clientResponse.StatusCode == (int)HttpStatusCode.Unauthorized)
                    return;

                Console.WriteLine(request);
                JObject requestBodyJson = JsonConvert.DeserializeObject<JObject>(ServerController.GetJasonPart(request));

                JArray cardsArray = requestBodyJson.Value<JArray>("Cards");
                List<Card> cards = cardsArray?.ToObject<List<Card>>() ?? new List<Card>();

                Package package = new Package
                {
                    PackageID = requestBodyJson.Value<int>("PackageID"),
                    Cards = cards,
                    Price = requestBodyJson.Value<int>("Price")
                };

                PackageRepository.CreatePackages(package);
                clientResponse.ResponseString = "Package created successfully.";
            }
            else
            {
                clientResponse.ResponseString = "Invalid HTTP method for this endpoint.";
                clientResponse.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
            }
        }

        public void ProcessTransactionRequest(string request, string method, ClientResponse clientResponse)
        {
            if (method == "POST")
            {
                string authHeader = ServerController.GetAuthorizationHeader(request);
                string token = authHeader.Replace("Bearer ", string.Empty);

                clientResponse = IsValidToken(request, clientResponse, token);
                if (clientResponse.StatusCode == (int)HttpStatusCode.Unauthorized)
                    return;

                string username = ServerController.GetUserNameFromToken(token);

                if (PackageRepository.PurchasePackage(username))
                {
                    clientResponse.ResponseString = "User successfully purchased Package";
                }
                else
                {
                    clientResponse.ResponseString = "Not enough Coins";
                    clientResponse.StatusCode = (int)HttpStatusCode.PaymentRequired;
                }
            }
            else
            {
                clientResponse.ResponseString = "Invalid HTTP method for this endpoint.";
                clientResponse.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
            }
        }

        public void ProcessCardsRequest(string request, string method, ClientResponse clientResponse)
        {
            if (method == "GET")
            {
                string authHeader = ServerController.GetAuthorizationHeader(request);

                if (authHeader == null)
                {
                    clientResponse.ResponseString = "No authentication Header";
                    clientResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                    return;
                }

                string token = authHeader.Replace("Bearer ", string.Empty);

                clientResponse = IsValidToken(request, clientResponse, token);
                if (clientResponse.StatusCode == (int)HttpStatusCode.Unauthorized)
                    return;

                List<Card> stack = StackRepository.GetUserStack(ServerController.GetUserNameFromToken(token));
                string stackJson = JsonConvert.SerializeObject(stack);

                clientResponse.ResponseString = stackJson;
                clientResponse.ResponseHeaders.Add("Content-Type", "application/json");
            }
            else
            {
                clientResponse.ResponseString = "Invalid HTTP method for this endpoint.";
                clientResponse.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
            }
        }

        public void ProcessDeckRequest(string request, string method, ClientResponse clientResponse)
        {
            string authHeader = ServerController.GetAuthorizationHeader(request);
            string token = authHeader.Replace("Bearer ", string.Empty);

            clientResponse = IsValidToken(request, clientResponse, token);
            if (clientResponse.StatusCode == (int)HttpStatusCode.Unauthorized)
                return;

            if (method == "GET")
            {
                if (request.Contains("?format=plain"))
                {
                    List<Card> deck = DeckRepository.GetUserDeck(ServerController.GetUserNameFromToken(token));
                    string plainTextRepresentation = ServerController.GeneratePlainTextRepresentation(deck);

                    clientResponse.ResponseString = plainTextRepresentation;
                    clientResponse.ResponseHeaders.Add("Content-Type", "text/plain");
                }
                else
                {
                    List<Card> deck = DeckRepository.GetUserDeck(ServerController.GetUserNameFromToken(token));
                    string deckJson = JsonConvert.SerializeObject(deck);

                    clientResponse.ResponseString = deckJson;
                    clientResponse.ResponseHeaders.Add("Content-Type", "application/json");
                }
            }

            else if (method == "PUT")
            {
                JObject requestBodyJson = JsonConvert.DeserializeObject<JObject>(ServerController.GetJasonPart(request));

                JArray cardIdsArray = requestBodyJson.Value<JArray>("CardIds");
                List<int> cardIds = cardIdsArray?.ToObject<List<int>>() ?? new List<int>();

                if (cardIds.Count == 4)
                {
                    string username = ServerController.GetUserNameFromToken(token);
                    User user = UserRepository.GetUserByUsername(username);

                    if (user != null && StackRepository.AreCardsInUserStack(user.UserID, cardIds))
                    {
                        DeckRepository.ClearUserDeck(user.UserID);
                        DeckRepository.AddCardToUserDeck(user.UserID, cardIds);
                        clientResponse.ResponseString = "Deck configured successfully.";
                    }
                    else
                    {
                        clientResponse.ResponseString = "Invalid card IDs. Only use cards that are in your stack and are currently not in Trading.";
                        clientResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                    }
                }
                else
                {
                    clientResponse.ResponseString = "Invalid number of cards. Please provide exactly 4 cards.";
                    clientResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                }
            }
            else
            {
                clientResponse.ResponseString = "Invalid HTTP method for this endpoint.";
                clientResponse.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
            }
        }

        public void ProcessStatsRequest(string request, string method, ClientResponse clientResponse)
        {
            if (method == "GET")
            {
                string authHeader = ServerController.GetAuthorizationHeader(request);
                string token = authHeader.Replace("Bearer ", string.Empty);

                clientResponse = IsValidToken(request, clientResponse, token);
                if (clientResponse.StatusCode == (int)HttpStatusCode.Unauthorized)
                    return;

                string username = ServerController.GetUserNameFromToken(token);
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
                    clientResponse.ResponseString = jsonResponse;
                    clientResponse.ResponseHeaders.Add("Content-Type", "application/json");
                }
                else
                {
                    clientResponse.ResponseString = "User not found.";
                    clientResponse.StatusCode = (int)HttpStatusCode.NotFound;
                }
            }
            else
            {
                clientResponse.ResponseString = "Invalid HTTP method for this endpoint.";
                clientResponse.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
            }
        }

        public void ProcessScoreboardRequest(string request, string method, ClientResponse clientResponse)
        {
            if (method == "GET")
            {
                string authHeader = ServerController.GetAuthorizationHeader(request);
                string token = authHeader.Replace("Bearer ", string.Empty);

                clientResponse = IsValidToken(request, clientResponse, token);
                if (clientResponse.StatusCode == (int)HttpStatusCode.Unauthorized)
                    return;

                List<UserScoreboardEntry> scoreboard = StatsRepository.GetScoreboard();

                string jsonResponse = JsonConvert.SerializeObject(scoreboard);
                clientResponse.ResponseString = jsonResponse;
                clientResponse.ResponseHeaders.Add("Content-Type", "application/json");
            }
            else
            {
                clientResponse.ResponseString = "Invalid HTTP method for this endpoint.";
                clientResponse.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
            }
        }

        public void ProcessBattlesRequest(string request, string method, ClientResponse clientResponse)
        {
            if (method == "POST")
            {
                string authHeader = ServerController.GetAuthorizationHeader(request);
                string token = authHeader.Replace("Bearer ", string.Empty);

                clientResponse = IsValidToken(request, clientResponse, token);
                if (clientResponse.StatusCode == (int)HttpStatusCode.Unauthorized)
                    return;

                string username = ServerController.GetUserNameFromToken(token);
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
                clientResponse.ResponseString = jsonResponse;
                clientResponse.ResponseHeaders.Add("Content-Type", "application/json");
            }
            else
            {
                clientResponse.ResponseString = "Invalid HTTP method for this endpoint.";
                clientResponse.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
            }
        }

        public void ProcessTradingsRequest(string request, string method, ClientResponse clientResponse)
        {
            string authHeader = ServerController.GetAuthorizationHeader(request);
            string token = authHeader.Replace("Bearer ", string.Empty);

            clientResponse = IsValidToken(request, clientResponse, token);
            if (clientResponse.StatusCode == (int)HttpStatusCode.Unauthorized)
                return;

            if (method == "GET")
            {               
                List<TradeRequirement> tradeRequirements = TradingRepository.GetAllTrades();
                string tradesJson = JsonConvert.SerializeObject(tradeRequirements);
                clientResponse.ResponseString = tradesJson;
                clientResponse.ResponseHeaders.Add("Content-Type", "application/json");
            }
            else if (method == "POST")
            {               
                string username = ServerController.GetUserNameFromToken(token);
                User user = UserRepository.GetUserByUsername(username);

                var segments = ServerController.GetUrlSegments(request);
                if (segments.Length < 3)
                {
                    // Normal POST request without trade ID in the URL
                    TradeRequirement tradeOffer = JsonConvert.DeserializeObject<TradeRequirement>(ServerController.GetJasonPart(request));
                    tradeOffer.UsersID = user.UserID;

                    if (DeckRepository.IsCardInUserDeck(tradeOffer.UsersID, tradeOffer.CardID))
                    {
                        clientResponse.ResponseString = "Card to Trade is Currently in Use in Deck";
                        clientResponse.StatusCode = (int)HttpStatusCode.Conflict;
                    }
                    else if (CardRepository.IsCardTrading(tradeOffer.CardID))
                    {
                        clientResponse.ResponseString = "Card to Trade is already Trading";
                        clientResponse.StatusCode = (int)HttpStatusCode.Conflict;
                    }
                    else
                    {
                        TradingRepository.CreateTrade(tradeOffer);
                        CardRepository.UpdateCardTradingStatus(tradeOffer.UsersID, tradeOffer.CardID, true);
                        clientResponse.ResponseString = "Trade created successfully.";
                    }
                }
                else
                {
                    // POST request with trade ID in the URL
                    int tradeID;
                    if (int.TryParse(segments[2].Trim('/'), out tradeID))
                    {
                        int acceptingCardID;
                        if (!int.TryParse(request.Trim('"'), out acceptingCardID))
                        {
                            clientResponse.ResponseString = "Invalid card ID format.";
                            clientResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                            return;
                        }

                        TradeRequirement tradeOffer = TradingRepository.GetTradebyTradID(tradeID);

                        // Check if the trade exists
                        if (tradeOffer == null)
                        {
                            clientResponse.ResponseString = "Trade not found.";
                            clientResponse.StatusCode = (int)HttpStatusCode.NotFound;
                        }
                        // Check if the trader is not the same person as the trade acceptor
                        else if (tradeOffer.UsersID == user.UserID)
                        {
                            clientResponse.ResponseString = "You cannot accept your own trade offer.";
                            clientResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                        }
                        // Check if user has this card
                        else if (!StackRepository.AreCardsInUserStack(user.UserID, new List<int> { acceptingCardID }))
                        {
                            clientResponse.ResponseString = "You don't own the Card";
                            clientResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                        }
                        else if (DeckRepository.IsCardInUserDeck(user.UserID, acceptingCardID))
                        {
                            clientResponse.ResponseString = "Card to Trade is Currently in Use in Deck";
                            clientResponse.StatusCode = (int)HttpStatusCode.Conflict;
                        }
                        else if (CardRepository.IsCardTrading(acceptingCardID))
                        {
                            clientResponse.ResponseString = "Card to Trade is already Trading";
                            clientResponse.StatusCode = (int)HttpStatusCode.Conflict;
                        }
                        // Check if the card has all the requirements
                        else if (!CardRepository.IsCardEligibleForTrade(acceptingCardID, tradeOffer.CardRegion, tradeOffer.CardType, tradeOffer.MinimumDamage))
                        {
                            clientResponse.ResponseString = "The provided card does not meet the trade requirements.";
                            clientResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                        }
                        else
                        {
                            // Process the trade acceptance
                            TradingRepository.AcceptTrade(tradeID, acceptingCardID, user.UserID);
                            clientResponse.ResponseString = "Trade accepted successfully.";
                        }
                    }
                    else
                    {
                        clientResponse.ResponseString = "Invalid trade ID format.";
                        clientResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                    }
                }
            }
            else if (method == "DELETE")
            {
                var segments = ServerController.GetUrlSegments(request);

                if (segments.Length >= 3)
                {
                    int tradesid = Convert.ToInt32(segments[2].Trim('/'));
                    TradeRequirement t = TradingRepository.GetTradebyTradID(tradesid);

                    if (t != null)
                    {
                        CardRepository.UpdateCardTradingStatus(t.UsersID, t.CardID, false);
                        TradingRepository.DeleteTradeById(t.TradesID);
                        clientResponse.ResponseString = $"Trade with ID: {tradesid} Deleted.";
                        clientResponse.StatusCode = (int)HttpStatusCode.OK;
                    }
                    else
                    {
                        clientResponse.ResponseString = "Trade not found.";
                        clientResponse.StatusCode = (int)HttpStatusCode.NotFound;
                    }
                }
                else
                {
                    clientResponse.ResponseString = "Invalid endpoint. Please provide a valid TradeID.";
                    clientResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                }
            }
            else
            {
                clientResponse.ResponseString = "Invalid HTTP method for this endpoint.";
                clientResponse.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
            }
        }

        public static ClientResponse IsValidToken(string request, ClientResponse cr, string token)
        {
            string authHeader = ServerController.GetAuthorizationHeader(request);

            if (string.IsNullOrEmpty(authHeader))
            {
                cr.ResponseString = "Unauthorized: Missing Authorization header.";
                cr.StatusCode = (int)HttpStatusCode.Unauthorized;
                return cr;
            }

            string headerToken = authHeader.Replace("Bearer ", "");

            if (headerToken != $"{ServerController.GetUserNameFromToken(token)}-mtcgToken")
            {
                cr.ResponseString = "Unauthorized: Invalid token.";
                cr.StatusCode = (int)HttpStatusCode.Unauthorized;
            }

            return cr;
        }
    }
}