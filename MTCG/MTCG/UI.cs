using MTCG.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG
{
    internal static class UI
    {
        public static void ShoeMenu()
        {
            Console.WriteLine("Welcome to the Game");
        }

        public static string GetStringInput()
        {
            string input;
            do {
                input = Console.ReadLine();
            } while (!string.IsNullOrEmpty(input) && !string.IsNullOrWhiteSpace(input));
            return input;
        }
         
        public static int GerIntInput(int max, int min)
        {
            int input;
            do {
                input = (int)Convert.ToInt64(Console.ReadLine());
            } while (input < max && input > min);
            return input;
        }

        public static string GetUsername()
        {
            Console.Write("\nUsername: ");
            return Console.ReadLine();
        }

        public static string GetPassword()
        {
            Console.Write("\nPasswor: ");
            return Console.ReadLine();
        }

        public static string ShowDeck(Deck d) 
        {
            
            return "";
        }
    }
}
