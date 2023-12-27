using MTCG.Cards;

namespace MTCG.Trading
{
    internal class Package
    {
        public Package(int packageKey, List<Card> cards, int price)
        {
            PackageKey = packageKey;
            Cards = cards;
            Price = price;
        }

        public int PackageID { get; set; }
        public int PackageKey { get; set; }
        public List<Card> Cards { get; set;}
        public int Price { get; set; } = 5;
    }
}
