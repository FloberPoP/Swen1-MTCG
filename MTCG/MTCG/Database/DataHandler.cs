using MTCG.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Database
{
    internal class DataHandler
    {
        public static User UserLogin(string uname, string pwd)
        {
            Stack stack = new Stack();
            Deck deck = new Deck();
            User u = new(stack, deck, 20, 100, uname, pwd);
            return u;
        }
    }
}
