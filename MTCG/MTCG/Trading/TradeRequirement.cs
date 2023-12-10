using MTCG.Cards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Trading
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
