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

    internal abstract class Card
    {
        public Card(string name, int damage, ERegions region)
        {
            Name = name;
            Damage = damage;
            Region = region;
        }

        public required string Name { get; set; }
        public required int Damage { get; set; }
        public required ERegions Region { get; set; }

        public int CalculateDamage(ERegions enemyRegion)
        {
            switch (enemyRegion)
            {
                case ERegions.VOID:
                    if (Region == ERegions.DEMACIA)
                        return Damage * 2;
                    else if (Region == ERegions.SHADOWISLES)
                        return Damage / 2;
                    break;
                case ERegions.SHADOWISLES:
                    if (Region == ERegions.VOID)
                        return Damage * 2;
                    else if (Region == ERegions.DEMACIA)
                        return Damage / 2;
                    break;
                case ERegions.DEMACIA:
                    if (Region == ERegions.SHADOWISLES)
                        return Damage * 2;
                    else if (Region == ERegions.VOID)
                        return Damage / 2;
                    break;
            }

            return Damage;
        }
    }
}
