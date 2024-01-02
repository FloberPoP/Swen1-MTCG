namespace MTCG.Model
{
    internal class Package
    {
        public int PackageID { get; set; }
        public List<Card> Cards { get; set; }
        public int Price { get; set; } = 5;
    }
}
