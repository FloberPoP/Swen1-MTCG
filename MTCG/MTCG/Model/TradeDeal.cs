namespace MTCG.Model
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
