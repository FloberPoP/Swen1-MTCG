using System;
using System.Text;
using MTCG.Cards;
using MTCG.Users;

namespace MTCG.Battling
{
    internal class Battle
    {
        private StringBuilder rounds;
        private User winner;
        private User looser;

        public Battle()
        {
            rounds = new StringBuilder();
        }

        public BattleLog StartBattle(User playerA, User playerB)
        {
            List<Card> deckA = playerA.Deck;
            List<Card> deckB = playerB.Deck;

            // Battle for up to 100 rounds
            for (int round = 1; round <= 100; round++)
            {
                // Draw cards from the decks for the current round
                Card cardA = DrawCard(deckA);
                Card cardB = DrawCard(deckB);

                // Calculate damage for the round
                int damageA = cardA.CalculateDamage(cardB);
                int damageB = cardB.CalculateDamage(cardA);

                // Determine the winner of the round
                if (damageA > damageB)
                {
                    deckB.Remove(cardB);
                    deckA.Add(cardB);
                    rounds.AppendLine($"{playerB.Username}: {cardB.Name} ({damageB} Damage) defeated {playerA.Username}: {cardA.Name} ({damageA} Damage)");
                }
                else if (damageB > damageA)
                {
                    deckA.Remove(cardA);
                    deckB.Add(cardA);
                    rounds.AppendLine($"{playerA.Username}: {cardA.Name} ({damageA} Damage) defeated {playerB.Username}: {cardB.Name} ({damageB} Damage)");
                }
                else
                {
                    rounds.AppendLine($"Round {round} is a draw");
                }

                if (deckA.Count == 0)
                {
                    winner = playerB;
                    looser = playerA;
                    break;
                }
                else if (deckB.Count == 0)
                {
                    winner = playerA;
                    looser = playerB;
                    break;
                }
            }

            if (winner == null && looser == null)
            {
                rounds.AppendLine("The battle is a draw (exceeded 100 rounds).");
            }

            
            // Update player stats (BattleCount and ELO calculation) => in DB over username

            return new BattleLog(rounds, looser.Username, winner.Username);
        }

        private Card DrawCard(List<Card> cards) 
        {
            Random random = new Random();
            int randomIndex = random.Next(cards.Count);

            Card drawnCard = cards[randomIndex];

            return drawnCard;
        }
    }
}