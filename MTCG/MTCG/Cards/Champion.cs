using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Cards
{
    internal class Champion : Card, IHealth
    {
        public int MaxHealth { get; set; }
        public int CurrentHealth { get; set; }

        public void UpdateHealth()
        {

        }
    }
}
