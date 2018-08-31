using System.Collections.Generic;
using NUnit.Framework;
using OnitamaTestClient.Models;
using OnitamaTestClient.Models.Enums;

using static OnitamaTestClient.Models.Enums.PlayerIdentityEnum;

[TestFixture]
public class Node_MinimaxShould
{
    private Node node;

    public Node_MinimaxShould()
    {
        GameState gameState = createGameState(Player1, pawn(Player1, 0, 0));

        node = new Node(gameState, Player1);
    }

    private static Piece pawn(PlayerIdentityEnum owner, int x, int y) {
        return createPiece(owner, PieceTypeEnum.Pawn, x, y);
    }

    private static Piece master(PlayerIdentityEnum owner, int x, int y) {
        return createPiece(owner, PieceTypeEnum.MasterPawn, x, y);
    }

    private static Piece createPiece(PlayerIdentityEnum owner, PieceTypeEnum type, int x, int y)
    {
        var piece = new Piece();
        piece.Owner = owner;
        piece.Type = type;
        piece.PositionOnBoard = new Position(x, y);
        return piece;
    }

    private static GameState createGameState(PlayerIdentityEnum current, params Piece[] pieces)
    {
        return createGameState(current, Player1, pieces);
    }
    private static GameState createGameState(PlayerIdentityEnum current, PlayerIdentityEnum mine, params Piece[] pieces)
    {
        var crane = new Card(CardType.Crane, new Position(0, 1));
        var crab = new Card(CardType.Crab, new Position(0, 1));
        // Fifth card is placed in the correct direction for the currently playing player to take on hand.
        var dragon = new Card(CardType.Dragon, new Position(0, 1));

        var gameState = new GameState();
        gameState.CurrentlyPlaying = current;
        if ( mine == Player1 ) {
            gameState.MyHand = new List<Card>() {crane};
            gameState.OpponentsHand = new List<Card>() {crab.flip()};
        } else {
            gameState.MyHand = new List<Card>() {crab.flip()};
            gameState.OpponentsHand = new List<Card>() {crane};
        }
        gameState.FifthCard = current == Player1 ? dragon : dragon.flip();
        gameState.Pieces = new List<Piece>(pieces);
        return gameState;
    }

    [Test]
    public void ReturnMaxValueGivenWinningBoardByCaptureOfMaster()
    {
        node = new Node(createGameState(Player1, master(Player1, 0, 0)), Player1);

        var result = node.Minimax(0);

        Assert.AreEqual(int.MaxValue, result);
    }

    [Test]
    public void ReturnMinValueGivenLosingBoardByLossOfMaster()
    {
        node = new Node(createGameState(Player1, master(Player2, 0, 0)), Player1);

        var result = node.Minimax(0);

        Assert.AreEqual(int.MinValue, result);
    }

    [Test]
    public void ReturnEvaluationScoreConsideringAllPieces()
    {
        node = new Node(createGameState(Player1, 
                                        master(Player1, 0, 0), pawn(Player1, 1, 0), pawn(Player1, 2, 0),
                                        master(Player2, 0, 4), pawn(Player2, 1, 4)), Player1);

        var result = node.Minimax(0);

        Assert.AreEqual(10, result);
    }

    [Test]
    public void ReturnZeroValueIfPiecesAreBalanced()
    {
        node = new Node(createGameState(Player1, master(Player1, 0, 0), master(Player2, 0, 4)), Player1);

        var result = node.Minimax(0);

        Assert.AreEqual(0, result);
    }

    [Test]
    public void ReturnMaxValueGivenWinningBoardByTakingOfTemple()
    {
        node = new Node(createGameState(Player1, master(Player1, 2, 4), master(Player2, 4, 4)), Player1);

        var result = node.Minimax(0);

        Assert.AreEqual(int.MaxValue, result);
    }

    [Test]
    public void ReturnMinValueGiveLosingBoardByTakingOfTemple()
    {
        node = new Node(createGameState(Player1, master(Player1, 0, 0), master(Player2, 2, 0)), Player1);

        var result = node.Minimax(0);

        Assert.AreEqual(int.MinValue, result);
    }

    [Test]
    public void ReturnMinValueGiveLosingBoardByTakingOfTempleInOneStep()
    {
        node = new Node(createGameState(Player1, master(Player1, 0, 0), master(Player2, 2, 1)), Player1);

        var result = node.Minimax(2);

        Assert.AreEqual(int.MinValue, result);
    }

    [Test]
    public void ReturnMaxValueGiveWinningBoardByTakingOfTempleInOneStep()
    {
        node = new Node(createGameState(Player1, master(Player1, 2, 3), master(Player2, 2, 1)), Player1);

        var result = node.Minimax(1);

        Assert.AreEqual(int.MaxValue, result);
    }

    [Test]
    public void ReturnMaxValueGiveWinningBoardByTakingOfTempleInThreeSteps()
    {
        node = new Node(createGameState(Player1, master(Player1, 2, 2), master(Player2, 4, 4)), Player1);

        var result = node.Minimax(3);

        Assert.AreEqual(int.MaxValue, result);
    }

    [Test]
    public void ReturnMaxValueGiveWinningBoardByTakingOfTempleInThreeStepsMaximizingPlayer2()
    {
        node = new Node(createGameState(Player2, master(Player1, 0, 0), master(Player2, 2, 2)), Player2);

        var result = node.Minimax(3);

        Assert.AreEqual(int.MaxValue, result);
    }

    [Test]
    public void ReturnMaxValueGiveWinningBoardByTakingOfMasterInOneStepsPlayer1()
    {
        node = new Node(createGameState(Player1, Player1, master(Player1, 2, 2), master(Player2, 2, 3)), Player1);

        var result = node.Minimax(1);

        Assert.AreEqual(int.MaxValue, result);
        var move = node.BestMove as Move.Play;
        Assert.AreEqual(move.UsedCard, CardType.Crane);
        Assert.AreEqual(move.From, new Position(2,2));
        Assert.AreEqual(move.To, new Position(2,3));
    }

    [Test]
    public void ReturnMinValueGiveLosingBoardByTakingOfMasterInOneStepsPlayer1()
    {
        node = new Node(createGameState(Player2, Player1, master(Player1, 2, 2), master(Player2, 2, 3)), Player1);

        var result = node.Minimax(1);

        Assert.AreEqual(int.MinValue, result);
    }

    [Test]
    public void ReturnMaxValueGiveWinningBoardByTakingOfMasterInOneStepsPlayer2()
    {
        node = new Node(createGameState(Player2, Player2, master(Player1, 2, 2), master(Player2, 2, 3)), Player2);

        var result = node.Minimax(1);

        Assert.AreEqual(int.MaxValue, result);
        var move = node.BestMove as Move.Play;
        Assert.AreEqual(move.UsedCard, CardType.Crab);
        Assert.AreEqual(move.From, new Position(2,3));
        Assert.AreEqual(move.To, new Position(2,2));
    }

    [Test]
    public void ReturnMinValueGiveLosingBoardByTakingOfMasterInOneStepsPlayer2()
    {
        node = new Node(createGameState(Player1, Player2, master(Player1, 2, 2), master(Player2, 2, 3)), Player2);

        var result = node.Minimax(1);

        Assert.AreEqual(int.MinValue, result);
    }

    private static GameState createGameStateBrokenGame(params Piece[] pieces)
    {
        // Player 1 at start.
        var crane12 = new Card(CardType.Crane, new Position(-1, -1), new Position(0, 1), new Position(1, -1));
        var rooster7 = new Card(CardType.Rooster, new Position(-1, -1), new Position(-1, 0), new Position(1, 0), new Position(1,1));
        // Player 2 at start.
        var boar13 = new Card(CardType.Boar, new Position(1, 0), new Position(0, -1), new Position(-1, 0));
        var monkey8 = new Card(CardType.Monkey, new Position(1, 1), new Position(1, -1), new Position(-1, 1), new Position(-1,-1));        
        // Fifth card is placed in the correct direction for the currently playing player to take on hand.
        var frog2 = new Card(CardType.Frog, new Position(-2, 0), new Position(-1, 1), new Position(1, -1));

        var gameState = new GameState();
        gameState.CurrentlyPlaying = Player1;
        gameState.MyHand = new List<Card>() {monkey8.flip(), boar13.flip()};
        gameState.OpponentsHand = new List<Card>() {crane12.flip(), rooster7.flip()};
        gameState.FifthCard = frog2;
        gameState.Pieces = new List<Piece>() {master(Player1, 2, 0), pawn(Player1, 0, 1), pawn(Player1, 3,0), pawn(Player1, 4, 0),
                                              master(Player2, 2, 4), pawn(Player2, 3, 2), pawn(Player2, 2, 1), pawn(Player2, 3, 1)};
        return gameState;
    }

    private static GameState createGameStateStart(params Piece[] pieces)
    {
        // Player 1 at start.
        var dragon = new Card(CardType.Dragon, new Position(-1, -1), new Position(-2, 1), new Position(2, 1), new Position(1, -1));
        var rooster7 = new Card(CardType.Rooster, new Position(-1, -1), new Position(-1, 0), new Position(1, 0), new Position(1,1));
        // Player 2 at start.
        var elephant = new Card(CardType.Elephant, new Position(1, 0), new Position(1, -1), new Position(-1, -1), new Position(-1, 0));
        var monkey8 = new Card(CardType.Monkey, new Position(1, 1), new Position(1, -1), new Position(-1, 1), new Position(-1,-1));        
        // Fifth card is placed in the correct direction for the currently playing player to take on hand.
        var frog2 = new Card(CardType.Frog, new Position(-2, 0), new Position(-1, 1), new Position(1, -1));

        var gameState = new GameState();
        gameState.CurrentlyPlaying = Player1;
        gameState.MyHand = new List<Card>() {monkey8, elephant};
        gameState.OpponentsHand = new List<Card>() {dragon, rooster7};
        gameState.FifthCard = frog2;
        gameState.Pieces = new List<Piece>() {master(Player1, 2, 1), pawn(Player1, 0, 1),  pawn(Player1, 1, 1), pawn(Player1, 3,1), pawn(Player1, 4, 1),
                                              master(Player2, 2, 3), pawn(Player2, 0, 3),  pawn(Player2, 1, 3), pawn(Player2, 3,3), pawn(Player2, 4, 3)};
        return gameState;
    }


    [Test, MaxTime(1500)]
    public void DoesNotHangInWeirdCase()
    {
        node = new Node(createGameStateBrokenGame(), Player1);

        var result = node.Minimax(5);

        Assert.AreEqual(int.MinValue, result);
        var move = node.BestMove as Move.Play;
        Assert.AreEqual(CardType.Monkey, move.UsedCard);
        Assert.AreEqual(new Position(2,0), move.From);
        Assert.AreEqual(new Position(1,1), move.To);
    }

    [Test, MaxTime(1500)]
    public void RunsFastEnough()
    {
        node = new Node(createGameStateStart(), Player1);

        var result = node.Minimax(5);

        Assert.AreEqual(0, result);
        var move = node.BestMove as Move.Play;
        Assert.AreEqual(CardType.Monkey, move.UsedCard);
        Assert.AreEqual(new Position(2,1), move.From);
        Assert.AreEqual(new Position(3,0), move.To);
    }

    // [Test]
    // public void ReturnMaxValueGiveWinningBoardByTakingOfTempleInOneStepMultipleOptions()
    // {
    //     node = new Node(createGameState(master(Player1, 2, 3), master(Player2, 2, 1)));

    //     var result = node.Minimax(1);

    //     Assert.AreEqual(int.MaxValue, result);
    // }
}