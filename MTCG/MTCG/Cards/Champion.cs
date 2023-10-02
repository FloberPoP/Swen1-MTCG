using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.Xml.Linq;
using System.Linq.Expressions;

namespace MTCG.Cards
{
    internal class Champion : Card, IHealth
    {
        public Champion(string name, int damage, ERegions region, int maxHealth, int currentHealth)
        : base(name, damage, region)
        {
            MaxHealth = maxHealth;
            CurrentHealth = currentHealth;
            IsDead = false;
        }

        public int MaxHealth { get; set; }
        public int CurrentHealth { get; set; }
<<<<<<< HEAD
        public bool IsDead { get; private set; }

        public void UpdateHealth(int enemyDamage)
        {
            CurrentHealth += enemyDamage;
            if(CurrentHealth <= 0)
                IsDead = true;
=======
        public bool Deaed { get; set; }
        public bool Stuned { get; set; }

        public void UpdateHealth(int value)
        {
            CurrentHealth += value;
>>>>>>> CardsImplementation
        }
    }
}
