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

        public static string[] GetUrlSegments(string request)
        {
            string[] requestLines = request.Split('\n');
            string[] requestTokens = requestLines[0].Split(' ');

            if (requestTokens.Length < 2)
            {
                return Array.Empty<string>();
            }

            string[] pathSegments = requestTokens[1].Split('/');
            return pathSegments
                .Where(segment => !string.IsNullOrWhiteSpace(segment))
                .Select(segment => segment.Trim())
                .ToArray();
        }

        public static string GetJasonPart(string request)
        {
            int jsonStartIndex = request.IndexOf('{');
            int jsonLength = request.LastIndexOf('}') - jsonStartIndex + 1;
            string jsonPart = request.Substring(jsonStartIndex, jsonLength);
            return jsonPart;
        }
    }
}
