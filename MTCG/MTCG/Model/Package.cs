namespace MTCG.Model
{
    public class Package
    {
        public int PackageID { get; set; }
        public List<Card> Cards { get; set; }
        public int Price { get; set; } = 5;
    }
}
