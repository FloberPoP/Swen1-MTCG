﻿using MTCG.Users;
using MTCG.Cards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG
{
    internal class Battle
    {
        /*Wie Runeterra alle karten gleichzeitig Booster wenn alle karten vom gleichen region sind
            //Void -> Spells deal 20% more damage
            //Shadow-> 10% vom Damage gehealt
            //Demacia -> Strongest 30% mor Damage
        */

        public StringBuilder? BattleLog { get; protected set; }

        public void BattleStart(User c1, User c2)
        {

        }

        private void ShowBattlefield(User c1, User c2)
        {
            //UI.ShowBattlefield(c1, c2);
        }
    }
}
