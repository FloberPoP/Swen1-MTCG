using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Cards
{
    public enum ESpellType { DAMAGE, HEAL, STUN };

    internal class Spell : Card
    {

        public ESpellType SpellType {  get; set; }

        public int CastAbility()
        {
            switch(SpellType)
            {
                case ESpellType.DAMAGE:
                    return Damage;
                case ESpellType.HEAL:
                    return Damage * -1;
                case ESpellType.STUN:
                    return 0;
            }
            return int.MaxValue;
        }
    }
}