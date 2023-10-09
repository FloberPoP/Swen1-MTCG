using MTCG.Cards;
using MTCG.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MTCG
{
    internal static class UI
    {
        public static void ShowMenu(User u, List<Option> options)
        {     
            int index = 0;
            WriteMenu(options, options[index], u.Username);

            ConsoleKeyInfo keyinfo;
            do
            {
                keyinfo = Console.ReadKey();

                if (keyinfo.Key == ConsoleKey.DownArrow && index + 1 < options.Count)
                {
                    index++;
                    WriteMenu(options, options[index], u.Username);
                }
                if (keyinfo.Key == ConsoleKey.UpArrow && index - 1 >= 0)
                {
                    index--;
                    WriteMenu(options, options[index], u.Username);
                }

                if (keyinfo.Key == ConsoleKey.Enter)
                {
                    options[index].Selected.Invoke();
                    index = 0;
                }
            }
            while (keyinfo.Key != ConsoleKey.X);

            Console.ReadKey();
        }

        static void WriteMenu(List<Option> options, Option selectedOption, string username)
        {
            Console.Clear();
            Console.WriteLine($"---- Welcome {username} To Legends of Runeterra ----");
            Console.WriteLine("Menu:");

            foreach (Option option in options)
            {
                if (option == selectedOption)
                {
                    Console.Write("> ");
                }
                else
                {
                    Console.Write(" ");
                }

                Console.WriteLine(option.Name);
            }
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

        public static void ShowDeck(Deck d) 
        {
           
        }

        #region Show Battle Field
        public static void ShowBattlefield(User c1, User c2)
        {
            int sideLength = Math.Max(Math.Max(Math.Max(CalculateSideLength(c1.Deck.Cards.Where(x => x is Champion).ToList()),
                                                        CalculateSideLength(c2.Deck.Cards.Where(x => x is Spell).ToList())),
                                                        CalculateSideLength(c1.Deck.Cards.Where(x => x is Champion).ToList())),
                                                        CalculateSideLength(c2.Deck.Cards.Where(x => x is Spell).ToList()));

            Console.Write("             ");
            printCharxTimes("-", sideLength+1);
            Console.WriteLine();
            printSpellinField(sideLength, c2.Deck.Cards);
            Console.WriteLine();
            Console.Write("             |");
            printCharxTimes(" ", sideLength-1);
            Console.Write("|");
            Console.WriteLine();
            printChampioninField(sideLength, c2.Deck.Cards);
            Console.Write($"Life: {c2.Health.ToString("00")}     ");
            printCharxTimes("-", sideLength+1);
            Console.Write($"     Mana: {c2.Mana.ToString("00")}");
            Console.WriteLine();
            printCharxTimes(" ", sideLength + 19);
            Console.Write($"Player {c1.Username} Attacking");
            Console.WriteLine();
            Console.Write($"Life: {c1.Health.ToString("00")}     ");
            printCharxTimes("-", sideLength + 1);
            Console.Write($"     Mana: {c1.Mana.ToString("00")}");
            Console.WriteLine();
            printChampioninField(sideLength, c1.Deck.Cards);
            Console.Write("             |");
            printCharxTimes(" ", sideLength - 1);
            Console.Write("|");
            Console.WriteLine();
            printSpellinField(sideLength, c1.Deck.Cards);
            Console.WriteLine();
            Console.Write("             ");
            printCharxTimes("-", sideLength + 1);
            Console.WriteLine();
        }

        private static void printSpellinField(int sideLength, List<Card> x)
        {
            int lenght = 0;
            Console.Write("             |   ");
            foreach (Card spell in x.Where(x => x is Spell))
            {
                Console.Write($"{spell.Name}   ");
                lenght += spell.Name.Length + 3;
            }
            printCharxTimes(" ", sideLength - lenght - 4);
            Console.Write("|");
        }

        private static void printChampioninField(int sideLength, List<Card> x)
        {
            int lenght = 0;
            Console.Write("             |   ");
            foreach (Card spell in x.Where(x => x is Champion))
            {
                Console.Write($"{spell.Name}   ");
                lenght += spell.Name.Length + 3;
            }
            printCharxTimes(" ", sideLength - lenght - 4);
            Console.Write("|");
            Console.WriteLine();
        }

        private static void printCharxTimes(string str, int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                Console.Write(str);
            }
        }
        private static int CalculateSideLength(List<Card> cards)
        {
            int length = 0;
            int count = 1;
            foreach (Card card in cards)
            {
                length += card.Name.Length;
                count ++; 
            }

            return length +3+3*count;
        }
        #endregion
    }
}