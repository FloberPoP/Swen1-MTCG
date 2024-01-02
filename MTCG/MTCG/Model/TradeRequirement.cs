namespace MTCG.Model
{
    public enum ERequirementType { Monster, Spell };
    internal class TradeRequirement
    {
        public ERequirementType Type { get; set; }
        public int MinDamage { get; set; }
        public int MinHealing { get; set; }
        public bool Stunning { get; set; }
        public ERegions RegionType { get; set; }
    }
}
