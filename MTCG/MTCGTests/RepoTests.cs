using MTCG.Model;

namespace MTCGTests
{
    [TestClass]
    public class RepoTests
    {
        [TestInitialize]
        public void Init() 
        {
            Seed.ClearDatabase();
            Seed.CreateTables();
        }
       
        [TestMethod]
        public void TestCreateUser()
        {
            User newUser = new User("TestUser", "TestPassword");
            UserRepository.CreateUser(newUser);

            User retrievedUser = UserRepository.GetUserByUsername("TestUser");

            Assert.IsNotNull(retrievedUser);
            Assert.AreEqual("TestUser", retrievedUser.Username);
            Assert.AreEqual("TestPassword", retrievedUser.Password);
        }

        [TestMethod]
        public void TestUpdateUser()
        {
            User user = new User("TestUser", "TestPassword");
            UserRepository.CreateUser(user);
            User usertest = UserRepository.GetUserByUsername(user.Username);

            usertest.Bio = "TestBio";
            usertest.Image = "TestImage";
            UserRepository.UpdateUser(usertest);
            User updatedUser = UserRepository.GetUserByUsername(usertest.Username);

            Assert.AreEqual(user.Bio, updatedUser.Bio);
            Assert.AreEqual(user.Image, updatedUser.Image);
        }

        [TestMethod]
        public void TestPurchasePackage()
        {
            string testUsername = "TestUser";
            User newUser = new User(testUsername, "TestPassword");
            UserRepository.CreateUser(newUser);

            Package testPackage = new Package
            {
                PackageID = 1,
                Cards = new List<Card> { new Card("TestCard", 10, ERegions.FIRE, EType.MONSTER) },
                Price = 5
            };
            PackageRepository.CreatePackages(testPackage);

            bool purchaseResult = PackageRepository.PurchasePackage(testUsername);

            Assert.IsTrue(purchaseResult);
            User updatedUser = UserRepository.GetUserByUsername(testUsername);
            Assert.AreEqual(15, updatedUser.Coins);
        }

        [TestMethod]
        public void TestCreateAndAcceptTrade()
        { 
            User user1 = new User("User1", "Password1");
            User user2 = new User("User2", "Password2");
            UserRepository.CreateUser(user1);
            UserRepository.CreateUser(user2);
            User testuser1 = UserRepository.GetUserByUsername(user1.Username);
            User testuser2 = UserRepository.GetUserByUsername(user2.Username);

            Card card1 = new Card("Card1", 20, ERegions.FIRE, EType.MONSTER);
            Card card2 = new Card("Card2", 15, ERegions.WATER, EType.SPELL);
            card1.CardsID = 1;
            card2.CardsID = 2;
            CardRepository.CreateCard(card1);
            CardRepository.CreateCard(card2);

            StackRepository.AddCardToUserStack(testuser1.UserID, card1.CardsID);
            StackRepository.AddCardToUserStack(testuser2.UserID, card2.CardsID);

            TradeRequirement tradeOffer = new TradeRequirement
            {
                TradesID = 1,
                UsersID = testuser1.UserID,
                CardID = card1.CardsID,
                CardRegion = ERegions.WATER,
                CardType = EType.SPELL,
                MinimumDamage = 15
            };

            TradingRepository.CreateTrade(tradeOffer);
            TradingRepository.AcceptTrade(tradeOffer.TradesID, card2.CardsID, testuser2.UserID);

            List<Card> user1Stack = StackRepository.GetUserStack(testuser1.Username);
            List<Card> user2Stack = StackRepository.GetUserStack(testuser2.Username);

            Assert.AreEqual(1, user1Stack.Count);
            Assert.AreEqual(1, user2Stack.Count);
            Assert.AreEqual(card2.CardsID, user1Stack[0].CardsID);
            Assert.AreEqual(card1.CardsID, user2Stack[0].CardsID);           
        }

        [TestMethod]
        public void TestTradeRepository()
        {
            User user1 = new User("User1", "Password1");
            User user2 = new User("User2", "Password2");
            UserRepository.CreateUser(user1);
            UserRepository.CreateUser(user2);
            User test1 = UserRepository.GetUserByUsername(user1.Username);

            Card card1 = new Card("Card1", 20, ERegions.FIRE, EType.MONSTER);
            CardRepository.CreateCard(card1);
            Card testcard =CardRepository.GetCardByName(card1.Name);

            StackRepository.AddCardToUserStack(test1.UserID, testcard.CardsID);

            TradeRequirement trade1 = new TradeRequirement
            {
                TradesID = 1,
                UsersID = test1.UserID,
                CardID = testcard.CardsID,
                CardRegion = ERegions.FIRE,
                CardType = EType.MONSTER,
                MinimumDamage = 15
            };

            TradingRepository.CreateTrade(trade1);

            TradeRequirement retrievedTrade = TradingRepository.GetTradebyTradID(1);

            Assert.IsNotNull(retrievedTrade);
            Assert.AreEqual(trade1.CardID, retrievedTrade.CardID);
            Assert.AreEqual(trade1.MinimumDamage, retrievedTrade.MinimumDamage);
        }

        [TestMethod]
        public void TestScoreboard()
        {
            User player1 = new User("Player1", "Password1") { Elo = 150 };
            User player2 = new User("Player2", "Password2") { Elo = 200 };
            User player3 = new User("Player3", "Password3") { Elo = 180 };

            UserRepository.CreateUser(player1);
            UserRepository.CreateUser(player2);
            UserRepository.CreateUser(player3);

            List<UserScoreboardEntry> scoreboard = StatsRepository.GetScoreboard();

            Assert.IsNotNull(scoreboard);
            Assert.AreEqual(3, scoreboard.Count);
            Assert.AreEqual("Player2", scoreboard[0].Username);
            Assert.AreEqual("Player3", scoreboard[1].Username);
            Assert.AreEqual("Player1", scoreboard[2].Username);
        }

        [TestMethod]
        public void TestCreateDeckRepository()
        {
            User user = new User("TestUser", "TestPassword");
            UserRepository.CreateUser(user);
            User testuser = UserRepository.GetUserByUsername(user.Username);

            Card card1 = new Card("Card1", 20, ERegions.FIRE, EType.MONSTER);
            Card card2 = new Card("Card2", 15, ERegions.WATER, EType.SPELL);
            card1.CardsID = 1;
            card2.CardsID = 2;
            CardRepository.CreateCard(card1);
            CardRepository.CreateCard(card2);

            Card testcard1 = CardRepository.GetCardByName(card1.Name);
            Card testcard2 = CardRepository.GetCardByName(card2.Name);

            StackRepository.AddCardToUserStack(testuser.UserID, testcard1.CardsID);
            StackRepository.AddCardToUserStack(testuser.UserID, testcard2.CardsID);

            List<int> userDeck = new List<int> { testcard1.CardsID, testcard2.CardsID };
            DeckRepository.AddCardToUserDeck(testuser.UserID, userDeck);

            List<Card> retrievedDeck = DeckRepository.GetUserDeck(testuser.Username);

            Assert.IsNotNull(retrievedDeck);
            Assert.AreEqual(userDeck.Count, retrievedDeck.Count);
        }
        
        [TestMethod]
        public void TestCreateStackRepository()
        {
            User user = new User("TestUser", "TestPassword");
            UserRepository.CreateUser(user);
            User testuser = UserRepository.GetUserByUsername(user.Username);

            Card card1 = new Card("Card1", 20, ERegions.FIRE, EType.MONSTER);
            Card card2 = new Card("Card2", 15, ERegions.WATER, EType.SPELL);
            card1.CardsID = 1;
            card2.CardsID = 2;
            CardRepository.CreateCard(card1);
            CardRepository.CreateCard(card2);

            Card testcard1 = CardRepository.GetCardByName(card1.Name);
            Card testcard2 = CardRepository.GetCardByName(card2.Name);

            StackRepository.AddCardToUserStack(testuser.UserID, testcard1.CardsID);
            StackRepository.AddCardToUserStack(testuser.UserID, testcard2.CardsID);          

            List<Card> retrievedStack = StackRepository.GetUserStack(testuser.Username);
            List<int> userStack = new List<int> { testcard1.CardsID, testcard2.CardsID };

            Assert.IsNotNull(retrievedStack);
            Assert.AreEqual(userStack.Count, retrievedStack.Count);
        }
        
        [TestMethod]
        public void TestDeckRepositoryWithInvalidCardID()
        {
            User user = new User("TestUser", "TestPassword");
            UserRepository.CreateUser(user);
            user = UserRepository.GetUserByUsername(user.Username);

            Card card1 = new Card("Card1", 20, ERegions.FIRE, EType.MONSTER);
            CardRepository.CreateCard(card1);

            card1 = CardRepository.GetCardByName(card1.Name);

            // Invalid card ID in the deck
            List<int> userDeck = new List<int> { card1.CardsID, 999 }; // Non-existent card ID
            DeckRepository.AddCardToUserDeck(user.UserID, userDeck);

            List<Card> retrievedDeck = DeckRepository.GetUserDeck(user.Username);

            Assert.IsNotNull(retrievedDeck);
            Assert.AreEqual(1, retrievedDeck.Count);
            Assert.AreEqual(card1.CardsID, retrievedDeck[0].CardsID);
        }

        [TestMethod]
        public void TestPurchasePackageWithInsufficientCoins()
        {
            string testUsername = "TestUser";
            User newUser = new User(testUsername, "TestPassword");
            UserRepository.CreateUser(newUser);
            newUser.Coins = 3;
            UserRepository.UpdateUser(newUser);

            Package testPackage = new Package
            {
                PackageID = 1,
                Cards = new List<Card> { new Card("TestCard", 10, ERegions.FIRE, EType.MONSTER) },
                Price = 500
            };
            PackageRepository.CreatePackages(testPackage);

            bool purchaseResult = PackageRepository.PurchasePackage(testUsername);

            Assert.IsFalse(purchaseResult);
            User updatedUser = UserRepository.GetUserByUsername(testUsername);
            Assert.AreEqual(3, updatedUser.Coins); // Coins should not be deducted
        }

        [TestMethod]
        public void TestInvalidCardName()
        {
            Card invalidCard = CardRepository.GetCardByName("NonExistentCard");
            Assert.IsNull(invalidCard);
        }

        [TestMethod]
        public void TestInvalidUserName()
        {
            User invalidUser = UserRepository.GetUserByUsername("NonExistentCard");
            Assert.IsNull(invalidUser);
        }

        [TestMethod]
        public void TestInvalidTradID()
        {
            TradeRequirement invalidTrade = TradingRepository.GetTradebyTradID(999);
            Assert.IsNull(invalidTrade);
        }

        [TestCleanup]
        public void Cleanup()
        {
            Seed.ClearDatabase();
        }
    }
}
