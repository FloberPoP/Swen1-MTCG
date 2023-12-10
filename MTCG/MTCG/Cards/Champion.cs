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
    internal class Champion : Card
    {
        public Champion(string name, int damage, int manaCost, ERegions region, int maxHealth)
        : base(name, damage, manaCost, region)
        {
            MaxHealth = maxHealth;
            CurrentHealth = maxHealth;
            IsDead = false;
            Stuned = false;
        }

        public int MaxHealth { get; set; }
        public int CurrentHealth { get; set; }
        public bool Stuned { get; set; }
        public bool IsDead { get; set; }
        public void UpdateHealth(int enemyDamage)
        {
            CurrentHealth -= enemyDamage;

            if (CurrentHealth <= 0) 
            { 
            
            }
        }
    }
}
