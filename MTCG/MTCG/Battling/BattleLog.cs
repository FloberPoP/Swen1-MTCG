using MTCG.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Battling
{
    internal class BattleLog
    {
        public int BattleID { get; set; }
        public User Participants { get; set; }
        public StringBuilder Rounds { get; set; }

        public User Winner { get; set; }

        public void AddMessage(string str) { Rounds.Append(str); }
    }
}
