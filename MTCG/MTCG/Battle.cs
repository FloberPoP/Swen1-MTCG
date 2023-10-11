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
        User PlayerOne {  get; set; }
        User PlayerTwo { get; set; }
        public StringBuilder BattleLog { get; protected set; }

        public Battle(User playerOne, User playerTwo)
        {
            PlayerOne = playerOne;
            PlayerTwo = playerTwo;
        }

        public void BattleStart()
        {
            int roundNumber = 1;
            while (CheckPlayers())
            {
                Console.WriteLine($"Round {roundNumber}");
                if (roundNumber % 2 != 0)
                    PlayRound(PlayerOne, PlayerTwo);
                else
                    PlayRound(PlayerTwo, PlayerOne);
                roundNumber++;
                RoundOver(roundNumber-1);
            }

            if (PlayerOne.Health <= 0)
            {
                BattleLog.Append($"{PlayerOne.Username} Lost.");
                BattleLog.Append($"{PlayerTwo.Username} Won.");
            }
            else if (PlayerTwo.Health <= 0)
            {
                BattleLog.Append($"{PlayerTwo.Username} Lost.");
                BattleLog.Append($"{PlayerOne.Username} Won.");
            }
        }

        private void PlayRound(User playerOne,User playerTwo)
        {
            Card attackingCard = SelectRandomCard(playerOne.Deck, 'c');
            ShowBattlefield();
            switch(CheckCard(attackingCard, playerTwo.Deck, playerOne))
            {
                case 0:
                    Champion a = (Champion)SelectRandomCard(playerTwo.Deck, 'c');
                    a.UpdateHealth(attackingCard.CalculateDamage(a.Region));
                    BattleLog.Append($"{a.Name} got attecked by {attackingCard.Name} for {attackingCard.CalculateDamage(a.Region)}");
                    if (a.IsDead)
                    {
                        BattleLog.Append($"{a.Name} Died");
                        playerTwo.Deck.RemoveCards(a);
                        playerTwo.DeadDeck.AddCards(a);
                    }

                    playerOne.Mana -= attackingCard.ManaCost;
                        
                    break;

                case -1:
                    BattleLog.Append($"{PlayerOne.Username} doesn't have enough mana.");
                    break;

                case -2:
                    BattleLog.Append($"{PlayerTwo.Username} has no more champions left");
                    BattleLog.Append($"{PlayerTwo.Username} got attacked by {attackingCard.Name} for {attackingCard.Damage}");
                    break;
            }
        }

        private void RoundOver(int count)
        {
            count = (count * 2 > 8) ? 4 : count;
            PlayerOne.Mana += count * 2;
            PlayerTwo.Mana += count * 2;
        }

        private int CheckCard(Card c, Deck d, User player)
        {
            if (c.ManaCost > player.Mana)  
            {
                return -1;
            }

            if (!d.Cards.Any(card => card is Champion))
            {
                return -2;
            }

            return 0;

        }

        private bool CheckPlayers()
        {
            if (PlayerOne.Health <= 0 || (PlayerOne.Deck == null || PlayerOne.Deck.Cards == null || PlayerOne.Deck.Cards.Count == 0))
            {
                return false;
            }
            else if (PlayerTwo.Health <= 0 || (PlayerTwo.Deck == null || PlayerTwo.Deck.Cards == null || PlayerTwo.Deck.Cards.Count == 0))
            {
                return false;
            }
            return true;
        }

        private Card SelectRandomCard(Deck d, char c)
        {
            if (d.Cards == null || d.Cards.Count == 0)
            {
                throw new NullReferenceException("Deck ist null oder leer");
            }

            Random random = new Random();

            if (c == 'c')
            {
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
                int randomIndex = random.Next(d.Cards.Count);
                return d.Cards[randomIndex];
            }
            else
            {
                throw new ArgumentException("Ungültiges Zeichen. Es sollte 'c', 's' oder 'a' sein.");
            }
        }

        private void ShowBattlefield()
        {
            UI.ShowBattlefield(PlayerOne, PlayerTwo);
        }

        private void ResetBattle()
        {
            foreach(Card c in PlayerOne.DeadDeck.Cards)
            {
                if(c is Champion)
                {
                    c.IsDead = false;
                    //c.CurrentHealth = c.MaxHealth;
                }
            }

            foreach (Card c in PlayerTwo.DeadDeck.Cards)
            {
                if (c is Champion)
                {
                    c.IsDead = false;
                    //c.CurrentHealth = c.MaxHealth;
                }
            }

            PlayerOne.Deck = PlayerOne.DeadDeck;
            PlayerTwo.Deck = PlayerTwo.DeadDeck;
            PlayerOne.DeadDeck.Cards.Clear();
            PlayerTwo.DeadDeck.Cards.Clear();
        }
    }
}
