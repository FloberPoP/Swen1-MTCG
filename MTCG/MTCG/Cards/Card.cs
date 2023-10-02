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
<<<<<<< HEAD
        public Card(string name, int damage, ERegions region)
        {
            Name = name;
            Damage = damage;
            Region = region;
=======
        public Card(string name, int damage, int manaCosts, ERegions regions)
        {
            Name = name;
            Damage = damage;
            ManaCosts = manaCosts;
            Regions = regions;
>>>>>>> CardsImplementation
        }

        public required string Name { get; set; }
        public required int Damage { get; set; }
<<<<<<< HEAD
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
=======
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
            return int.MaxValue;
>>>>>>> CardsImplementation
        }
    }
}