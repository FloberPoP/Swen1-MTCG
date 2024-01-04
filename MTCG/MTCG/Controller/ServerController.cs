using MTCG.Model;
using MTCG.Repositorys;
using System.Text;

namespace MTCG.Controller
{
    public static class ServerController
    {                              
        public static bool ValidateUserCredentials(string username, string password)
        {
            User user = UserRepository.GetUserByUsername(username);
            if (user != null && user.Username == username && password == user.Password)
            {
                return true;
            }
            return false;
        }

        public static string GeneratePlainTextRepresentation(List<Card> deck)
        {
            StringBuilder plainText = new StringBuilder();

            plainText.AppendLine("Deck:");

            foreach (Card card in deck)
            {
                plainText.AppendLine($"\t{card.Name}, Damage: {card.Damage}, Region: {card.Region}, Type: {card.Type}");
            }

            return plainText.ToString();
        }

        public static string GenerateToken(string username)
        {
            string token = $"{username}-mtcgToken";
            return token;
        }     

        public static string GetUserNameFromToken(string tokenString)
        {
            if (tokenString.EndsWith("-mtcgToken"))
            {
                return tokenString.Substring(0, tokenString.Length - "-mtcgToken".Length);
            }
            return null;
        }

        public static string GetAuthorizationHeader(string request)
        {
            string authorizationHeader = null;
            string[] requestLines = request.Split('\n');

            foreach (var line in requestLines)
            {
                if (line.StartsWith("Authorization:", StringComparison.OrdinalIgnoreCase))
                {
                    authorizationHeader = line.Trim().Substring("Authorization:".Length).Trim();
                    break;
                }
            }

            return authorizationHeader;
        }

        public static string GetUsernameFromPath(string request)
        {
            string[] requestLines = request.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            string[] requestTokens = requestLines[0].Split(' ');

            if (requestTokens.Length < 2)
            {
                return string.Empty;
            }

            string[] pathSegments = requestTokens[1].Split('/');

            int usersIndex = Array.IndexOf(pathSegments, "users");
            if (usersIndex >= 0 && usersIndex < pathSegments.Length - 1)
            {
                return pathSegments[usersIndex + 1];
            }

            return string.Empty;
        }


        public static int GetTradeIDFromPath(string request)
        {
            string[] requestLines = request.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            string[] requestTokens = requestLines[0].Split(' ');

            if (requestTokens.Length < 2)
            {
                return -1;
            }

            string[] pathSegments = requestTokens[1].Split('/');

            int tradingsIndex = Array.IndexOf(pathSegments, "tradings");

            if (tradingsIndex >= 0 && tradingsIndex < pathSegments.Length - 1)
            {
                if (int.TryParse(pathSegments[tradingsIndex + 1], out int tradeID))
                {
                    return tradeID;
                }
            }

            return -1;
        }


        public static string GetJasonPart(string request)
        {
            int jsonStartIndex = request.IndexOf('{');
            int jsonLength = request.LastIndexOf('}') - jsonStartIndex + 1;

            if(jsonStartIndex < 0)
            {
                jsonStartIndex = request.IndexOf('[');
                jsonLength = request.LastIndexOf(']') - jsonStartIndex + 1;
            }
            if (jsonStartIndex < 0)
            {
                jsonStartIndex = request.IndexOf('"');
                jsonLength = request.LastIndexOf('"') - jsonStartIndex + 1;
            }
            string jsonPart = request.Substring(jsonStartIndex, jsonLength);
            return jsonPart;
        }
    }
}
