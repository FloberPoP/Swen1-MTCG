using MTCG.Cards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Users
{
    internal class Deck : ICardList
    {
        public List<Card>? Cards { get; }

        public void AddCards(Card c)
        {
            if(Cards.Count < 4)
                Cards.Add(c);
        }

        public void RemoveCards(Card c) => Cards.Remove(c);
    }
}
