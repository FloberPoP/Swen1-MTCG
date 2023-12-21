using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCG.Battling;
using MTCG.Cards;

namespace MTCG.Users
{
    internal class User
    {
        public User(List<Card> stack, List<Card> deck, int? coins, int? elo, int? battleCount, string username, string password)
        {
            Stack = stack;
            Deck = deck;
            Coins = coins;
            Elo = elo;
            Username = username;
            Password = password;
            BattleCount = battleCount;
        }

        public List<Card> Stack { get; set; }
        public List<Card> Deck { get; set; }
        public int? Coins { get; set; }
        public int? Elo {  get; set; }

        public int? BattleCount { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public void ManageDeck()
        {
        }

        public void BuyCards()
        {

        }

        public void Battle()
        {
            Battle b = new Battle();
            //get from DB
            List<Card> stack = new List<Card>();
            List<Card> deck = new List<Card>
            {
                new Card("WaterGoblin", 10, ERegions.WATER, EType.MONSTER),
                new Card("WaterGoblin", 10, ERegions.WATER, EType.MONSTER),
                new Card("WaterGoblin", 10, ERegions.WATER, EType.MONSTER),
                new Card("WaterGoblin", 10, ERegions.WATER, EType.MONSTER),
                new Card("WaterGoblin", 10, ERegions.WATER, EType.MONSTER)
            };
            User playerB = new User(stack, deck, 20, 100, 0, "UserTWO", "abc");
            BattleLog log = b.StartBattle(this, playerB);
            log.Print();
        }

        public void Login()
        {

        }

        public void Logout()
        {

        }

        public void ShowStats()
        {

        }

        public void RequestDeal()
        {

        }
    }
}
