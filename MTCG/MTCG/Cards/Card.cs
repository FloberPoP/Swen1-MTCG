using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Cards
{
    //Void --> Shadow
    //Shadow --> Demacia
    //Demacia --> Void
    public enum ERegions { VOID, SHADOWISLES, DEMACIA };

    internal class Card
    {
        public required string Name { get; set; }
        public required int Damage { get; set; }
        public required int ManaCosts { get; set; }
        public required ERegions Regions { get; set; }
    }
}
