# Desgin
Model-Classes for Modelling
Controller-Classes for Server and Client Controlling
    Switch for the separate Endpoints
Repositoryies for DB Handling the Models

# Lessons Learned
Token:
I first worked with the JWT Token framework, which was very interesting, but since I saw in the curlscript that a normal string was used for a token, I rewritten the methods. 
Which was of course very time-consuming

HTTPListener:
At the beginning I used the HTTP Listener but I wasn't sure as it's not really an HTTP framework but then I converted to just sockets to be sure which was also very time consuming

In total:
I should have started with the Model Classes and the Server and not prioritising the Battle Logic cause i wanted to do my own Battle but it became too complicated so that's why I revised it and in the end i and finally I added my unique feature and tested everything


unique feature: 
If a player has more than 3 cards with the same region and plays one of them, it gets a bonus that depends on the region.

Water:
    -10 damage to the opponent
Fire:
    +10 damage to self
Normal:
    The elemental boost for the opponent card is not used
# unit test design
Battle Logic Tests:

Testing of standard Battle
    public void TestBattleLogic()

Testing Battle with Fixed Winner
    public void TestBattleWithWinner()
    
Testing Battle with Fixed Draw
    public void TestBattleWithDraw()
    
Testing Buffs activation during Battle
    public void TestBuffActivation()
    
Testing BuffActivation Methode for Water
    public void TestBattleWATERBuff()
    
Testing BuffActivation Methode for Fire
    public void TestBattleFIREBuff()
    
Testing BuffActivation Methode for Normal
    public void TestBattleNORMALBuff()
    

Repository Tests:

Testing Updates User
    public void TestUpdateUser()

Full Testing of Trads with trade accepting
    public void TestTradeRepository()

Test get Scoreboard with right Params
    public void TestScoreboard()

Testing Create DB Calls
    public void TestCreateDeckRepository()
    public void TestCreateStackRepository()
    public void TestCreateAndAcceptTrade()
    public void TestCreateUser()

Testing Purchase Package
    public void TestPurchasePackage()
    public void TestPurchasePackageWithInsufficientCoins()

Testing for no unforeseen Exception
    public void TestInvalidCardName()
    public void TestInvalidUserName()
    public void TestInvalidTradID()
    public void TestDeckRepositoryWithInvalidCardID()

# time spent
39 Commits:
~69.5 h

# link to git:
https://github.com/FloberPoP/Swen1-MTCG.git