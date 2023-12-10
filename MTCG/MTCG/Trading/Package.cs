using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCG.Cards;

namespace MTCG.Trading
{
    internal class Package
    {
        public int PackageID { get; set; }
        public List<Card> Cards { get; set;}
        public int Price { get; set; } = 5;


        public List<Card> Open()
        {
            return Cards;
        }
    }
}
