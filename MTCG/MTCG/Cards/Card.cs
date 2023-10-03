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

    internal abstract class Card
    {
        public string Name { get; set; }
        public int Damage { get; set; }
        public int ManaCost { get; set; }
        public ERegions Region { get; set; }
        public Card(string name, int damage, int manaCosts, ERegions regions)
        {
            Name = name;
            Damage = damage;
            ManaCost = manaCosts;
            Region = regions;
        }

        public int CalculateDamage(ERegions enemyRegion)
        {
            switch (enemyRegion)
            {
                case ERegions.VOID:
                    return (Region == ERegions.SHADOWISLES) ? Damage / 2 : (Region == ERegions.DEMACIA) ? Damage * 2 : Damage;
                case ERegions.SHADOWISLES:
                    return (Region == ERegions.DEMACIA) ? Damage / 2 : (Region == ERegions.VOID) ? Damage * 2 : Damage;
                case ERegions.DEMACIA:
                    return (Region == ERegions.VOID) ? Damage / 2 : (Region == ERegions.SHADOWISLES) ? Damage * 2 : Damage;
            }
            return int.MaxValue;

        }
    }
}