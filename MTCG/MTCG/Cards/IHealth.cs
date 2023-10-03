using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Cards
{
    internal interface IHealth
    {
        int MaxHealth { get; set; }
        int CurrentHealth { get; set; }
        public void UpdateHealth(int value);
    }
}
