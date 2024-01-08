using MTCG.Model;
using MTCG.Repositorys;

namespace MTCG.Battling
{ 
    public class Battle
    {
        private User? winner;
        private User? loser;
        private const int MaxRounds = 100;
        private const int BuffThreshold = 3;
        private const int BuffDamage = 10;
        BattleLog log;

        public Battle()
        {
            log = new BattleLog();
        }

        public BattleLog StartBattle(User playerA, User playerB)
        {
            List<Card> deckA = playerA.Deck;
            List<Card> deckB = playerB.Deck;

            for (int round = 1; round <= MaxRounds; round++)
            {;
                Card cardA = DrawCard(deckA);
                Card cardB = DrawCard(deckB);    
                
                int damageA = CalculateBuffDamage(playerA, playerB, cardA, cardB);               
                int damageB = CalculateBuffDamage(playerB, playerA, cardB, cardA);

                if (damageA > damageB)
                {
                    deckB.Remove(cardB);
                    deckA.Add(cardB);
                    log.AddMessage($"{playerA.Username}: {cardA.Name} ({damageA} Damage) defeated {playerB.Username}: {cardB.Name} ({damageB} Damage)");
                }
                else if (damageB > damageA)
                {
                    deckA.Remove(cardA);
                    deckB.Add(cardA);
                    log.AddMessage($"{playerB.Username}: {cardB.Name} ({damageB} Damage) defeated {playerA.Username}: {cardA.Name} ({damageA} Damage)");                    
                }
                else
                {
                    log.AddMessage($"Round {round} is a draw: {playerB.Username}: {cardB.Name} ({damageB} Damage) vs {playerA.Username}: {cardA.Name} ({damageA} Damage)");
                }
                
                if (deckA.Count == 0)
                {
                    winner = playerB;
                    loser = playerA;
                    break;
                }
                else if (deckB.Count == 0)
                {
                    winner = playerA;
                    loser = playerB;
                    break;
                }
            }

            if (winner == null && loser == null)
            {             
                log.Draw = true;
                log.LoserID = playerA.UserID;
                log.WinnerID = playerB.UserID;
            }
            else
            {
                log.Draw = false;
                log.LoserID = loser.UserID;
                log.WinnerID = winner.UserID;
                UpdateUserElo(winner, loser);
            }   
            return log;
        }

        public Card DrawCard(List<Card> cards) 
        {
            Random random = new Random();
            int randomIndex = random.Next(cards.Count);
            Card drawnCard = cards[randomIndex];

            return drawnCard;
        }

        public bool CheckForBuff(List<Card> deck, ERegions targetRegion)
        {
            int count = deck.Count(card => card.Region == targetRegion);
            
            return count >= BuffThreshold;
        }

        private int CalculateBuffDamage(User playerA, User playerB, Card cardA, Card cardB)
        {
            int damage = cardA.CalculateDamage(cardB);
            
            if (CheckForBuff(playerB.Deck, ERegions.NORMAL) && cardB.Region == ERegions.NORMAL)
            {
                if (cardA.CalculateDamage(cardB) < cardA.Damage)
                {
                    damage = cardA.CalculateDamage(cardB);
                }
                else
                {
                    damage = cardA.Damage;
                }
                log.AddMessage($"Buff activated for {playerB.Username}: Neutral Ground: Nullify opponent's elemental advantages");              
            }
            if (CheckForBuff(playerB.Deck, ERegions.WATER) && cardB.Region == ERegions.WATER)
            {                           
                damage -= BuffDamage;
                log.AddMessage($"Buff activated for {playerB.Username}: Frostbite -> Inflict a debuff on the opponent, reducing their damage");
            }
            if(CheckForBuff(playerA.Deck, ERegions.FIRE) && cardA.Region == ERegions.FIRE)
            {
                damage += BuffDamage;
                log.AddMessage($"Buff activated for {playerA.Username}: Inferno: Increase the damage dealt to the opponent");
            }
            return damage;            
        }

        public void UpdateUserElo(User winner, User loser)
        {
            int kFactor = 10;
            
            double expectedWinProbabilityA = 1 / (1 + Math.Pow(10, (loser.Elo - winner.Elo) / 400.0));
            double expectedWinProbabilityB = 1 / (1 + Math.Pow(10, (winner.Elo - loser.Elo) / 400.0));

            int newEloA = winner.Elo + (int)(kFactor * (1 - expectedWinProbabilityA));
            int newEloB = loser.Elo + (int)(kFactor * (0 - expectedWinProbabilityB));

            newEloA = (newEloA <= 0) ? 0 : newEloA;
            newEloB = (newEloB <= 0) ? 0 : newEloB;

            winner.Elo = newEloA;
            loser.Elo = newEloB;

            UserRepository.UpdateUser(winner);
            UserRepository.UpdateUser(loser);
        }
    }
}