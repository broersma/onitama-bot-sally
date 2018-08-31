using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using OnitamaTestClient.Models;
using OnitamaTestClient.Models.Enums;

public class Node {

    private readonly PlayerIdentityEnum maximizingPlayer;

    private readonly GameState gameState;

    public Node(GameState gameState, PlayerIdentityEnum maximizingPlayer) {
        this.maximizingPlayer = maximizingPlayer;
        this.gameState = gameState;
    }

    public Move.Play BestMove { get; private set; }

    public int Minimax(int depth) {
        return Minimax(depth, int.MinValue, int.MaxValue);
    }
    public int Minimax(int depth, int min, int max) {
        if ( depth == 0 || IsLeaf() ) {
            return Evaluate(this.gameState, this.maximizingPlayer);
        }

        if ( IsMaxNode()) {
            var v = min;
            // TODO invert dependency in v2
            foreach (var move in GetPossibleMoves(this.gameState, this.gameState.MyHand, maximizingPlayer)) {
                if ( BestMove == null ) {
                    BestMove = move;
                }

                var v2 = new Node(createGameStateWith(move), maximizingPlayer).Minimax(depth - 1, v, max);
                if (v2 > v) {
                    v = v2;
                    BestMove = move;
                }
                if ( v > max ) {
                    return max;
                }
            }

            // TODO add pass plays???? (if so remove passing code in MyBot:GetNextMove)
            return v;
        } else {
            var v = max;
            foreach (var move in GetPossibleMoves(this.gameState, this.gameState.OpponentsHand, Opponent(maximizingPlayer))) {
                var v2 = new Node(createGameStateWith(move), maximizingPlayer).Minimax(depth - 1, min, v);
                if (v2 < v) {
                    v = v2;
                }
                if ( v < min ) {
                    return min;
                }
            }
            // TODO add pass plays???? (if so remove passing code in MyBot:GetNextMove)
            return v;
        }
    }

    private GameState createGameStateWith(Move move)
    {
        // Copy game state.
        var newGameState = new GameState();
        newGameState.MyHand = new List<Card>(gameState.MyHand);
        newGameState.OpponentsHand = new List<Card>(gameState.OpponentsHand);
        newGameState.Pieces = new List<Piece>(gameState.Pieces);

        var currentlyPlayingHand = gameState.CurrentlyPlaying == maximizingPlayer ? newGameState.MyHand : newGameState.OpponentsHand;

        // Process play move.
        var usedCard = currentlyPlayingHand.Find(c => c.Type == move.UsedCard);

        currentlyPlayingHand.Remove(usedCard);
        currentlyPlayingHand.Add(gameState.FifthCard);
        newGameState.FifthCard = usedCard.flip();

        var play = move as Move.Play;
        if ( play != null ) {
            newGameState.Pieces.RemoveAll(p => p.PositionOnBoard.IsEqualTo(play.To));
            var movingPiece = newGameState.Pieces.Find(p => p.PositionOnBoard.IsEqualTo(play.From));
            newGameState.Pieces.Remove(movingPiece);
            newGameState.Pieces.Add(movingPiece.moveTo(play.To));
        }

        newGameState.CurrentlyPlaying = gameState.CurrentlyPlaying == PlayerIdentityEnum.Player1 ? PlayerIdentityEnum.Player2 : PlayerIdentityEnum.Player1;

        return newGameState;
    }

    private static IEnumerable<Move.Play> GetPossibleMoves(GameState gameState, List<Card> playersHand, PlayerIdentityEnum player) {
        //return GetPossibleMoves2(playersHand, player).ToList().OrderByDescending(x => x. Item1).Select(x => x.Item2);
    //}
   // private IEnumerable<Tuple<int, Move.Play>> GetPossibleMoves2(List<Card> playersHand, PlayerIdentityEnum player) {
        foreach (var piece in gameState.Pieces.Where(p => p.Owner == player)) {
            foreach (var card in playersHand) {
                foreach (var target in card.Targets) {
                    var newPosition = piece.PositionOnBoard + target;
                    var potentialMove = new Move.Play(card.Type, piece.PositionOnBoard, newPosition);


                    if (IsPlayValid(gameState, potentialMove)) {
                        //var score = Evaluate(createGameStateWith(potentialMove), maximizingPlayer);
                        //yield return new Tuple<int, Move.Play>(score, potentialMove);
                        yield return potentialMove;
                    }
                }
            }
        }
    }

    private static bool IsPlayValid(GameState gameState, Move.Play move) {
        //out of bounds
        if (move.To.X < 0 || move.To.X > 4 || move.To.Y < 0 || move.To.Y > 4) {
            return false;
        }

        //if there's a piece on the target position, only allow if piece is of the enemy
        var maybePiece = gameState.Pieces.Find(p => p.PositionOnBoard.IsEqualTo(move.To));
        if (maybePiece != null) {
            return maybePiece.Owner != gameState.CurrentlyPlaying;
        }

        // if is in bounds and noone is on the target position, go ahead.
        return true;
    }

    private bool IsLeaf()
    {
        return gameState.Pieces.FindAll(p => p.Type == PieceTypeEnum.MasterPawn).Count < 2 || gameState.Pieces.Exists(p => IsMasterOnOpposingTempleSquare(p));
    }

    private bool IsMaxNode()
    {
        return gameState.CurrentlyPlaying == maximizingPlayer;
    }

    private static int Evaluate(GameState gameState, PlayerIdentityEnum maximizingPlayer)
    {
        // TODO TODO consider postional stuff as well...
        // He considered some positional factors, subtracting ½ point for each doubled pawn, backward pawn, and isolated pawn.
        // Another positional factor in the evaluation function was mobility, adding 0.1 point for each legal move available.
        // Finally, he considered checkmate to be the capture of the king, and gave the king the artificial value of 200 points.
 
        var numMasters = gameState.Pieces.FindAll(p => p.Type == PieceTypeEnum.MasterPawn).Count;
        if ( numMasters < 2 ) {
            foreach ( Piece piece in gameState.Pieces ) {
                if(piece.Type == PieceTypeEnum.MasterPawn) {
                    return piece.Owner == maximizingPlayer ? int.MaxValue : int.MinValue;
                }
            }
        }

        int score = 0;
        foreach ( Piece piece in gameState.Pieces ) {
            if ( IsMasterOnOpposingTempleSquare(piece) ) {
                return piece.Owner == maximizingPlayer ? int.MaxValue : int.MinValue;
            }
            score += GetValue(piece, maximizingPlayer);
        }

        // TODO
        //score += GetPossibleMoves(gameState, gameState.MyHand, maximizingPlayer).Count();
        //score -= GetPossibleMoves(gameState, gameState.OpponentsHand, Opponent(maximizingPlayer)).Count();

        return score;
    }

    private static bool IsMasterOnOpposingTempleSquare(Piece piece) {
        return piece.Type == PieceTypeEnum.MasterPawn && piece.PositionOnBoard.IsEqualTo(TemplePosition(Opponent(piece.Owner)));
    }

    private static Position TemplePosition(PlayerIdentityEnum player) {
        return player == PlayerIdentityEnum.Player1 ? new Position(2, 0) : new Position(2, 4);
    }

    private static PlayerIdentityEnum Opponent(PlayerIdentityEnum player) {
        return player == PlayerIdentityEnum.Player1 ? PlayerIdentityEnum.Player2 : PlayerIdentityEnum.Player1;
    }

    private static int GetValue(Piece p, PlayerIdentityEnum maximizingPlayer)
    {
        int value = p.Type == PieceTypeEnum.MasterPawn ? 100 : 10;

        return p.Owner == maximizingPlayer ? value : -value;
    }
}