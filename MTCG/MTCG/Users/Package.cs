using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCG.Cards;

namespace MTCG.Users
{
    internal class Package
    {
        public List<Card>? Cards { get; set; }
        private const int Price = 5;
    }
}
