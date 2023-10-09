using MTCG.Users;
using MTCG.Cards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.ComponentModel.Design;

namespace MTCG
{
    internal class Battle
    {
        public StringBuilder? BattleLog { get; protected set; }

        public void BattleStart(User playerOne, User playerTwo)
        {
            int roundNumber = 1;
            while (CheckPlayers(playerOne, playerTwo))
            {
                Console.WriteLine($"Round {roundNumber}");
                if (roundNumber % 2 != 0)
                    PlayRound(playerOne, playerTwo);
                else
                    PlayRound(playerTwo, playerOne);
                roundNumber++;
                RoundOver(roundNumber-1, playerOne, playerTwo);
            }

            // Spiel beenden und Ergebnis anzeigen
            if (playerOne.Health <= 0)
            {
                Console.WriteLine($"{playerOne.Username} hat verloren.");
            }
            else if (playerTwo.Health <= 0)
            {
                Console.WriteLine($"{playerTwo.Username} hat verloren.");
            }
            else
            {
                Console.WriteLine("Unentschieden.");
            }
        }

        private void PlayRound(User playerOne, User playerTwo)
        {
            ShowBattlefield(playerOne, playerTwo);
            switch(CheckCard(SelectRandomCard(playerOne.Deck, 'c'), playerTwo.Deck))
            {
                case 0:
                    break;

                case -1: 
                    //Ausgabe Player hat nicht genug Mana
                    break;

                case -2:
                    //ausgabe keine Monster zum angreifen Player Wird sebst angegriffen
                    break;

                //ein random champion mit er Methode SelectRandomCard(d2, 'c') die eine Card zurückgibt 
                //Champion ausgewählht => greift random Champion von Player 2 an
                //wenn nur mehr spells Übrig ausgabe keine Monster zum angreifen Player Wird sebst angegriffen
                //Sonst damage an Monster->Monster veriert Leben -> Monster Tot -> Dead Deck hinzufügen vom Main Deck entfernen
                //Spell ausgewählt => greift random Champion an
                //Pro Runde Mehr Mana wenn zu wenig Mana nächster player ist drann
            }

            //Wie Runeterra alle karten gleichzeitig Booster wenn alle karten vom gleichen region sind
            //Void -> Spells deal 20% more damage
            //Shadow-> 10% vom Damage gehealt
            //Demacia -> Strongest 30% mor Damage


            // Hier sollten Sie die Logik für den Kampf zwischen den Champions implementieren
            // Zum Beispiel: championOne.Attack(championTwo);

            // Update der Spielerleben und Manawerte
            // HealthPlayerOne -= damageOne;
            // HealthPlayerTwo -= damageTwo;
            // ManaPlayerOne += 2;
            // ManaPlayerTwo += 2;

            // Hier können Sie weitere Aktionen für den Zug implementieren
        }



        private void RoundOver(int count, User playerOne, User playerTwo)
        {
            count = (count * 2 > 8) ? 4 : count;
            playerOne.Mana += count * 2;
            playerTwo.Mana += count * 2;
        }


        private int CheckCard(Card c, Deck d)
        {
            if (PlayerOnRound)
            {
                if (c.ManaCost > ManaPlayerOne)
                {
                    return -1; // Spieler 1 hat nicht genug Mana
                }
            }
            else
            {
                if (c.ManaCost > ManaPlayerTwo)  
                {
                    return -1; // Spieler 2 hat nicht genug Mana
                }
            }

            if (!d.Cards.Any(card => card is Champion))
            {
                return -2; // Im Deck ist kein Champion vorhanden
            }

            // Alles ist in Ordnung, die Karte kann gespielt werden
            return 0;

        }

        private bool CheckPlayers(User playerOne, User playerTwo)
        {
            if (HealthPlayerOne <= 0 || (playerOne.Deck == null || playerOne.Deck.Cards == null || playerOne.Deck.Cards.Count == 0))
            {
                return false; // Spieler 1 hat verloren
            }
            else if (HealthPlayerTwo <= 0 || (playerTwo.Deck == null || playerTwo.Deck.Cards == null || playerTwo.Deck.Cards.Count == 0))
            {
                return false; // Spieler 2 hat verloren
            }
            return true;
        }

        private static Random random = new Random();

        private Card SelectRandomCard(Deck d, char c)
        {
            if (d.Cards == null || d.Cards.Count == 0)
            {
                throw new NullReferenceException("Deck ist null oder leer");
            }

            Random random = new Random();

            if (c == 'c')
            {
                // Zufälligen Champion auswählen
                List<Card> champions = d.Cards.Where(card => card is Champion).ToList();
                if (champions.Count == 0)
                {
                    throw new Exception("Keine Champions im Deck gefunden");
                }
                int randomIndex = random.Next(champions.Count);
                return champions[randomIndex];
            }
            else if (c == 's')
            {
                // Zufälligen Zauberspruch (Spell) auswählen
                List<Card> spells = d.Cards.Where(card => card is Spell).ToList();
                if (spells.Count == 0)
                {
                    throw new Exception("Keine Zaubersprüche (Spells) im Deck gefunden");
                }
                int randomIndex = random.Next(spells.Count);
                return spells[randomIndex];
            }
            else if (c == 'a')
            {
                // Einfach eine zufällige Karte auswählen
                int randomIndex = random.Next(d.Cards.Count);
                return d.Cards[randomIndex];
            }
            else
            {
                throw new ArgumentException("Ungültiges Zeichen. Es sollte 'c', 's' oder 'a' sein.");
            }
        }

        private void ShowBattlefield(User c1, User c2)
        {
            UI.ShowBattlefield(c1, c2, HealthPlayerOne, HealthPlayerTwo, ManaPlayerOne, ManaPlayerTwo);
        }

        private void ResetBattle()
        {
            //Dead Deck wird zu Player Deck wieder hinzugefügt (Leben von den Champions wieder herrstellens
        }
    }
}
