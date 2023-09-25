using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Users
{
    internal class User
    {
        public Stack? Stack { get; set; }
        public Deck? Deck { get; set; }

        public int? Coins { get; set; } = 20;
        public int? Elo {  get; set; }

        public required string Username { get; set; }
        public required string Password { get; set; }

        public void ManageDeck()
        {
            if(Deck != null && Deck.Cards != null) {
                //Deck.Cards.Clear();
                //Deck.Cards.Count() == 4

                
            }
        }

        public void BuyCards()
        {

        }

        public void Battle()
        {

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
