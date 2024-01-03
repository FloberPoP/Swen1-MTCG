namespace MTCG.Model
{
    internal class TradeRequirement
    {
        public int TradesID { get; set; }
        public int UsersID { get; set; }
        public int CardID { get; set; }
        public EType CardType { get; set; }
        public int MinimumDamage { get; set; }
        public ERegions CardRegion { get; set; }
    }
}
