namespace MTCG.Model
{
    public class User
    {
        public User(string username, string password)
        {
            Stack = new List<Card>();
            Deck = new List<Card>();
            Coins = 20;
            Elo = 100;
            Username = username;
            Password = password;
        }

        public int UserID { get; set; }
        public int StackID { get; set; }
        public List<Card> Stack { get; set; }
        public int DeckID { get; set; }
        public List<Card> Deck { get; set; }
        public int? Coins { get; set; }
        public int? Elo { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Bio { get; set;}
        public string Image { get; set; }
    }
}
