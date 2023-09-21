using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCG.Cards;

namespace MTCG.Users
{
    interface ICardList
    {
        public void AddCards(Card c);

        public void RemoveCards(Card c);
    }
}
