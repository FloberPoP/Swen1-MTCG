using MTCG.Cards;

namespace MTCG.Trading
{
    internal class Package
    {
        public int PackageID { get; set; }
        public int PackageKey { get; set; }
        public List<Card> Cards { get; set;}
        public int Price { get; set; } = 5;
    }
}
