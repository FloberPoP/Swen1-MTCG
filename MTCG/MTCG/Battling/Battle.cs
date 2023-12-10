using MTCG.Users;
using MTCG.Cards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.ComponentModel.Design;

namespace MTCG.Battling
{
    internal class Battle
    {
        User Attacker { get; set; }
        User Defender { get; set; }
        public BattleLog Log { get; protected set; }

        public Battle(User playerOne, User playerTwo)
        {
            Attacker = playerOne;
            Defender = playerTwo;
            Attacker.Mana = 0; 
            Defender.Mana = 0;
            Attacker.Health = 10;
            Defender.Health = 10;
        }

        public void BattleStart()
        {
            int roundNumber = 1;
            while (CheckPlayers())
            {
                Console.WriteLine($"Round {roundNumber}");
                Log.AddMessage($"Round {roundNumber}");
                if (roundNumber % 2 != 0)
                    PlayRound(Attacker, Defender);
                else
                    PlayRound(Defender, Attacker);
                roundNumber++;
                RoundOver(roundNumber - 1);
            }

            if (Attacker.Health <= 0)
            {
                Log.AddMessage($"{Attacker.Username} Lost.");
                Log.AddMessage($"{Defender.Username} Won.");
                Console.WriteLine($"{Attacker.Username} Lost.");
                Console.WriteLine($"{Defender.Username} Won.");
            }
            else if (Defender.Health <= 0)
            {
                Log.AddMessage($"{Defender.Username} Lost.");
                Log.AddMessage($"{Attacker.Username} Won.");
                Console.WriteLine($"{Defender.Username} Lost.");
                Console.WriteLine($"{Attacker.Username} Won.");
            }
        }

        private void PlayRound(User attacker, User defender)
        {
            Card attackingCard = SelectRandomCard(attacker.Deck, 'a');
            Card defendingCard = SelectRandomCard(defender.Deck, 'a');
            ShowBattlefield();

            switch (CheckCard(attackingCard, defender.Deck, attacker))
            {
                case 0:
                    // Champion VS Champion
                    if (attackingCard is Champion attackingChampion && defendingCard is Champion defenderChampion)
                    {
                        SomethingVSChampion(attackingChampion, defenderChampion);
                        SomethingVSChampion(defenderChampion, attackingChampion);
                    }
                    // Spell vs Champion
                    else if (attackingCard is Spell attackerSpell && defendingCard is Champion defenderChampion) 
                    {
                        if (attackerSpell.SpellType == ESpellType.DAMAGE)
                            SomethingVSChampion(attackerSpell, defenderChampion);
                        else(attackerSpell.SpellType == ESpellType.HEAL)
                        {
                            Champion tmpChampion = (Champion)SelectRandomCard(attacker.Deck, 'c');
                            tmpChampion.UpdateHealth(attackerSpell.CastAbility());
                        }
                               

                    }
                    // If the defender has a spell
                    else if (defendingCard is Spell defenderSpell)
                    {
                        // Redirect the damage against the player
                        defender.Health -= attackingCard.CalculateDamage(defenderSpell.Region);
                        Log.AddMessage($"{attackingCard.Name} attacked {defenderSpell.Name}. Damage redirected to {defender.Username} for {attackingCard.CalculateDamage(defenderSpell.ElementType)}");

                        if (defender.Health <= 0)
                        {
                            Log.AddMessage($"{defender.Username} Lost.");
                            Log.AddMessage($"{attacker.Username} Won.");
                        }
                    }

                    // Deduct mana cost from attacker's mana
                    attacker.Mana -= attackingCard.ManaCost;
                    break;

                case -1:
                    Log.AddMessage($"{attacker.Username} doesn't have enough mana.");
                    break;

                case -2:
                    Log.AddMessage($"{defender.Username} has no more champions left");
                    Log.AddMessage($"{defender.Username} got attacked by {attackingCard.Name} for {attackingCard.Damage}");
                    break;
            }
        }

        private void SomethingVSChampion(Card attacker, Champion defender)
        {
            defender.UpdateHealth(attacker.CalculateDamage(defender.Region));
            Log.AddMessage($"{defender.Name} got attacked by {attacker.Name} for {attacker.CalculateDamage(defender.Region)}");

            if (defender.IsDead)
            {
                Log.AddMessage($"{defender.Name} Died");
                defender.IsDead = true;
            }
        }
        private void RoundOver(int count)
        {
            count = count * 2 > 8 ? 4 : count;
            Attacker_Mana += count * 2;
            Defender_Mana += count * 2;
        }

        private int CheckCard(Card c, Deck d, User player)
        {
            if (c.ManaCost > player.Mana) // change to Defender_Mana or Attacker_Mana
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
            if (Attacker_Health <= 0 || Attacker.Deck == null || Attacker.Deck.Cards == null || Attacker.Deck.Cards.Count == 0)
            {
                return false;
            }
            else if (Attacker_Health <= 0 || Defender.Deck == null || Defender.Deck.Cards == null || Defender.Deck.Cards.Count == 0)
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
            UI.ShowBattlefield(Attacker, Defender);
        }
    }
}
