﻿namespace MTCG
{
    internal class Program
    {
        static void Main(string[] args)
        {
            GameController controller = new GameController();
            controller.StartGame();
            Console.WriteLine("Ich bin ein Pokemon Spiel");
        }
    }
}