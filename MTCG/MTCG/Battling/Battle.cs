using System.Text;
using MTCG.Model;
using MTCG.Repositorys;
using Npgsql.Replication.PgOutput.Messages;

namespace MTCG.Battling
{
    internal class Battle
    {
        private User winner;
        private User looser;

        public BattleLog StartBattle(User playerA, User playerB)
        {
            BattleLog log = new BattleLog();
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
                    log.AddMessage($"{playerB.Username}: {cardB.Name} ({damageB} Damage) defeated {playerA.Username}: {cardA.Name} ({damageA} Damage)");
                }
                else if (damageB > damageA)
                {
                    deckA.Remove(cardA);
                    deckB.Add(cardA);
                    log.AddMessage($"{playerA.Username}: {cardA.Name} ({damageA} Damage) defeated {playerB.Username}: {cardB.Name} ({damageB} Damage)");
                }
                else
                {
                    log.AddMessage($"Round {round} is a draw");
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
                log.Draw = true;
                log.LooserID = playerA.UserID;
                log.WinnerID = playerB.UserID;
            }
            else
            {
                log.Draw = false;
                log.LooserID = looser.UserID;
                log.WinnerID = winner.UserID;
                UpdateUserElo(winner, looser);
            }   
            return log;
        }

        private Card DrawCard(List<Card> cards) 
        {
            Random random = new Random();
            int randomIndex = random.Next(cards.Count);

            Card drawnCard = cards[randomIndex];

            return drawnCard;
        }

        public static void UpdateUserElo(User winner, User looser)
        {
            int kFactor = 10;

            double expectedWinProbabilityA = 1 / (1 + Math.Pow(10, (looser.Elo - winner.Elo) / 400.0));
            double expectedWinProbabilityB = 1 / (1 + Math.Pow(10, (winner.Elo - looser.Elo) / 400.0));

            int newEloA = winner.Elo + (int)(kFactor * (1 - expectedWinProbabilityA));
            int newEloB = looser.Elo + (int)(kFactor * (0 - expectedWinProbabilityB));

            winner.Elo = newEloA;
            looser.Elo = newEloB;

            UserRepository.UpdateUser(winner);
            UserRepository.UpdateUser(looser);
        }
    }
}