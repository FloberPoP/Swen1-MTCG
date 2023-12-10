using MTCG.Cards;
using MTCG.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Trading
{
    public enum ETradeStatus { CLOSED, OPEN };
    internal class TradeDeal
    {
        public User Initiator { get; set; }
        public Card CardToTrade { get; set; }
        public TradeRequirement Requirement { get; set; }
        public ETradeStatus status { get; set; }

        public bool AcceptDeal()
        {
            return false;
        }
    }
}
