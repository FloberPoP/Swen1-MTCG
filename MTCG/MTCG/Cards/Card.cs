using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
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

        public int DamageCalculation(ERegions eRegions)
        {
            switch(eRegions)
            {
                case ERegions.VOID:
                    return (Regions == ERegions.SHADOWISLES) ? Damage / 2 : (Regions == ERegions.DEMACIA) ? Damage * 2 : Damage;
                case ERegions .SHADOWISLES:
                    return (Regions == ERegions.DEMACIA) ? Damage / 2 : (Regions == ERegions.VOID) ? Damage * 2 : Damage;
                case ERegions .DEMACIA:
                    return (Regions == ERegions.VOID) ? Damage / 2 : (Regions == ERegions.SHADOWISLES) ? Damage * 2 : Damage;
            }
            return -1;
        }
    }
}