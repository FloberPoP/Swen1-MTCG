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

        public StringBuilder Rounds { get; set; }
        public User Looser { get; set; }
        public User Winner { get; set; }

        public BattleLog(StringBuilder rounds, User looser, User winner)
        {
            Rounds = rounds;
            Looser = looser;
            Winner = winner;
        }
        public void AddMessage(string str) { Rounds.Append($"{str}\n"); }

        public void Print()
        {
            
            Console.WriteLine("Rounds:");
            Console.WriteLine(Rounds.ToString());
            Console.WriteLine($"\n\nWinner: {Winner.Username}");
            Console.WriteLine($"Looser: {Looser.Username}");
        }
    }
}
