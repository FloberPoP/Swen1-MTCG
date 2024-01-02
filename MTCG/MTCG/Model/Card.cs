namespace MTCG.Model
{
    //water -> fire
    //fire -> normal
    //normal -> water
    public enum ERegions { WATER, FIRE, NORMAL };
    public enum EType { SPELL, MONSTER }
    public class Card
    {
        public int CardsID { get; set; }
        public string Name { get; set; }
        public int Damage { get; set; }
        public ERegions Region { get; set; }
        public EType Type { get; set; }

        public Card(string name, int damage, ERegions regions, EType type)
        {
            Name = name;
            Damage = damage;
            Region = regions;
            Type = type;
        }

        public int CalculateDamage(Card enemy)
        {
            if (!(enemy.Type == EType.MONSTER && Type == EType.MONSTER))
            {
                switch (enemy.Region)
                {
                    case ERegions.WATER:
                        return Region == ERegions.FIRE ? Damage / 2 : Region == ERegions.NORMAL ? Damage * 2 : Damage;
                    case ERegions.FIRE:
                        return Region == ERegions.NORMAL ? Damage / 2 : Region == ERegions.WATER ? Damage * 2 : Damage;
                    case ERegions.NORMAL:
                        return Region == ERegions.WATER ? Damage / 2 : Region == ERegions.FIRE ? Damage * 2 : Damage;
                }
            }

            return Damage;
        }
    }
}