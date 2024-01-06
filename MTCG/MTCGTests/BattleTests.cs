namespace MTCGTests
{
    [TestClass]
    public class BattleTests
    {
        [TestInitialize]
        public void Init()
        {
            Seed.ClearDatabase();
            Seed.CreateTables();
        }

        [TestMethod]
        public void TestBattleLogic()
        {
            User playerA = new User("PlayerA", "PasswordA");
            User playerB = new User("PlayerB", "PasswordB");

            Card cardA1 = new Card("CardA1", 15, ERegions.FIRE, EType.MONSTER);
            cardA1.CardsID = 1;
            Card cardA2 = new Card("CardA2", 20, ERegions.WATER, EType.SPELL);
            cardA2.CardsID = 2;
            Card cardB1 = new Card("CardB1", 18, ERegions.NORMAL, EType.MONSTER);
            cardB1.CardsID = 3;
            Card cardB2 = new Card("CardB2", 25, ERegions.FIRE, EType.SPELL);
            cardB2.CardsID = 4;

            CardRepository.CreateCard(cardA1);
            CardRepository.CreateCard(cardA2);
            CardRepository.CreateCard(cardB1);
            CardRepository.CreateCard(cardB2);
            UserRepository.CreateUser(playerA);
            UserRepository.CreateUser(playerB);

            User TestUserA = UserRepository.GetUserByUsername(playerA.Username);
            User TestUserB = UserRepository.GetUserByUsername(playerB.Username);

            TestUserA.Deck = new List<Card> { cardA1, cardA2 };
            TestUserB.Deck = new List<Card> { cardB1, cardB2 };

            Battle battle = new Battle();
            BattleLog battleLog = battle.StartBattle(TestUserA, TestUserB);

            StatsRepository.InsertBattleLog(battleLog);

            Assert.IsNotNull(battleLog);
            Assert.IsFalse(battleLog.Draw);
            Assert.IsTrue(battleLog.Rounds.Length > 0);
            Assert.IsTrue(battleLog.LoserID > 0);
            Assert.IsTrue(battleLog.WinnerID > 0);
        }

        [TestMethod]
        public void TestBattleWithWinner()
        {
            User playerA = new User("PlayerA", "PasswordA");
            User playerB = new User("PlayerB", "PasswordB");

            Card cardA1 = new Card("CardA1", 150, ERegions.FIRE, EType.MONSTER);
            cardA1.CardsID = 1;
            Card cardB1 = new Card("CardB1", 10, ERegions.WATER, EType.MONSTER);
            cardB1.CardsID = 2;

            CardRepository.CreateCard(cardA1);
            CardRepository.CreateCard(cardB1);
            UserRepository.CreateUser(playerA);
            UserRepository.CreateUser(playerB);

            User TestUserA = UserRepository.GetUserByUsername(playerA.Username);
            User TestUserB = UserRepository.GetUserByUsername(playerB.Username);

            TestUserA.Deck = new List<Card> { cardA1 };
            TestUserB.Deck = new List<Card> { cardB1 };

            Battle battle = new Battle();
            BattleLog battleLog = battle.StartBattle(TestUserA, TestUserB);

            StatsRepository.InsertBattleLog(battleLog);

            Assert.IsNotNull(battleLog);
            Assert.IsFalse(battleLog.Draw);
            Assert.AreEqual(TestUserB.UserID, battleLog.LoserID);
            Assert.AreEqual(TestUserA.UserID, battleLog.WinnerID);
        }

        [TestMethod]
        public void TestBattleWithDraw()
        {
            User playerA = new User("PlayerA", "PasswordA");
            User playerB = new User("PlayerB", "PasswordB");

            Card cardA1 = new Card("CardA1", 10, ERegions.FIRE, EType.MONSTER);
            cardA1.CardsID = 1;
            Card cardB1 = new Card("CardB1", 10, ERegions.FIRE, EType.MONSTER);
            cardB1.CardsID = 2;

            CardRepository.CreateCard(cardA1);
            CardRepository.CreateCard(cardB1);
            UserRepository.CreateUser(playerA);
            UserRepository.CreateUser(playerB);

            User TestUserA = UserRepository.GetUserByUsername(playerA.Username);
            User TestUserB = UserRepository.GetUserByUsername(playerB.Username);

            TestUserA.Deck = new List<Card> { cardA1 };
            TestUserB.Deck = new List<Card> { cardB1 };

            Battle battle = new Battle();
            BattleLog battleLog = battle.StartBattle(TestUserA, TestUserB);

            StatsRepository.InsertBattleLog(battleLog);

            Assert.IsNotNull(battleLog);
            Assert.IsTrue(battleLog.Draw);
        }

        [TestMethod]
        public void TestBuffActivation()
        {
            User playerA = new User("PlayerA", "PasswordA");
            User playerB = new User("PlayerB", "PasswordB");

            Card cardA1 = new Card("CardA1", 20, ERegions.WATER, EType.MONSTER);
            cardA1.CardsID = 1;
            Card cardA2 = new Card("CardA2", 20, ERegions.WATER, EType.MONSTER);
            cardA2.CardsID = 2;
            Card cardA3 = new Card("CardA3", 20, ERegions.WATER, EType.MONSTER);
            cardA3.CardsID = 3;


            Card cardB1 = new Card("CardB1", 2, ERegions.NORMAL, EType.MONSTER);
            cardB1.CardsID = 4;
            Card cardB2 = new Card("CardB2", 2, ERegions.NORMAL, EType.MONSTER);
            cardB2.CardsID = 5;
            Card cardB3 = new Card("CardB3", 2, ERegions.NORMAL, EType.MONSTER);
            cardB3.CardsID = 6;

            CardRepository.CreateCard(cardA1);
            CardRepository.CreateCard(cardA2);
            CardRepository.CreateCard(cardB1);
            CardRepository.CreateCard(cardB2);
            UserRepository.CreateUser(playerA);
            UserRepository.CreateUser(playerB);

            User TestUserA = UserRepository.GetUserByUsername(playerA.Username);
            User TestUserB = UserRepository.GetUserByUsername(playerB.Username);

            TestUserA.Deck = new List<Card> { cardA1, cardA2, cardA3 };
            TestUserB.Deck = new List<Card> { cardB1, cardB2, cardB3 };

            Battle battle = new Battle();
            BattleLog battleLog = battle.StartBattle(TestUserA, TestUserB);

            StatsRepository.InsertBattleLog(battleLog);

            Assert.IsNotNull(battleLog);
            Assert.IsFalse(battleLog.Draw);
            Assert.AreEqual(TestUserB.UserID, battleLog.LoserID);
            Assert.AreEqual(TestUserA.UserID, battleLog.WinnerID);
            StringAssert.Contains(battleLog.Rounds, "Buff activated for PlayerA");
            StringAssert.Contains(battleLog.Rounds, "Buff activated for PlayerB");
        }
       
              
        [TestMethod]
        public void TestBattleWATERBuff()
        {
            Battle b = new Battle();
            List<Card> cards =
            [
                new Card("Card1", 15, ERegions.WATER, EType.MONSTER),
                new Card("Card2", 15, ERegions.WATER, EType.SPELL),
                new Card("Card3", 15, ERegions.WATER, EType.SPELL),
                new Card("Card4", 15, ERegions.NORMAL, EType.MONSTER),
            ];

            Assert.IsTrue(b.CheckForBuff(cards, ERegions.WATER));
            Assert.IsFalse(b.CheckForBuff(cards, ERegions.FIRE));
            Assert.IsFalse(b.CheckForBuff(cards, ERegions.NORMAL));
        }

        [TestMethod]
        public void TestBattleFIREBuff()
        {
            Battle b = new Battle();
            List<Card> cards =
            [
                new Card("Card1", 15, ERegions.FIRE, EType.MONSTER),
                new Card("Card2", 15, ERegions.FIRE, EType.SPELL),
                new Card("Card3", 15, ERegions.FIRE, EType.SPELL),
                new Card("Card4", 15, ERegions.NORMAL, EType.MONSTER),
            ];

            Assert.IsTrue(b.CheckForBuff(cards, ERegions.FIRE));
            Assert.IsFalse(b.CheckForBuff(cards, ERegions.WATER));
            Assert.IsFalse(b.CheckForBuff(cards, ERegions.NORMAL));
        }

        [TestMethod]
        public void TestBattleNORMALBuff()
        {
            Battle b = new Battle();
            List<Card> cards =
            [
                new Card("Card1", 15, ERegions.NORMAL, EType.MONSTER),
                new Card("Card2", 15, ERegions.NORMAL, EType.SPELL),
                new Card("Card3", 15, ERegions.NORMAL, EType.SPELL),
                new Card("Card4", 15, ERegions.WATER, EType.MONSTER),
            ];

            Assert.IsTrue(b.CheckForBuff(cards, ERegions.NORMAL));
            Assert.IsFalse(b.CheckForBuff(cards, ERegions.FIRE));
            Assert.IsFalse(b.CheckForBuff(cards, ERegions.WATER));
        }

        [TestCleanup]
        public void Cleanup()
        {
            Seed.ClearDatabase();
        }
    }
}
