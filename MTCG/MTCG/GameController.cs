using MTCG.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG
{
    internal class GameController
    {
        public void StartGame(User u)
        {
            List<Option> options = new List<Option>
            {
                new Option("Search for Battle", () => u.Battle()),
                new Option("Manage Cards", () =>  u.ManageDeck()),
                new Option("ShowStats", () =>  u.ShowStats()),
                new Option("Shop", () =>  u.BuyCards()),
                new Option("LogOut", () => u.Logout()),
                new Option("Exit", () => Environment.Exit(0)),
            };

            UI.ShowMenu(u, options);
        }
    }
}
