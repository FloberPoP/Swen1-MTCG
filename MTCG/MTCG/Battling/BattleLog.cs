using System.Text;

namespace MTCG.Battling
{
    internal class BattleLog
    {
        public int BattleID { get; set; }
        public string Rounds { get; set; }
        public int LooserID { get; set; }
        public int WinnerID { get; set; }
        public bool Draw {  get; set; }
        public void AddMessage(string str) { Rounds += $"{str}\n"; }
    }
}
