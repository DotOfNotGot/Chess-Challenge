using System;
using System.Collections.Generic;
using System.Linq;
using ChessChallenge.API;
using ChessChallenge.Application.APIHelpers;

public struct EvaluatedMove
{
    public Move move;
    public float evaluation;
}

public class MyBot : IChessBot
{
    // private Move[] _moves = new Move[4];
    // Piece values: null, pawn, knight, bishop, rook, queen, king
    // private readonly int[] _pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };
    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();
        float bestScore = 0.0f;

        Move moveToMake = new Move();
        
        foreach (var move in moves)
        {
            float currentScore =
                EvaluateMove(board, move, 4, float.NegativeInfinity, float.PositiveInfinity, board.IsWhiteToMove);
            
            if ((board.IsWhiteToMove && currentScore >= bestScore) || (!board.IsWhiteToMove && currentScore <= bestScore))
            {
                moveToMake = move;
                bestScore = currentScore;
            }
        }

        Console.WriteLine($"Best score: {bestScore}, Move: {moveToMake}");

        return moveToMake;
    }

    // private bool FindBestCapture(Board board, Move move, ref int highestValueCapture)
    // {
    //     Piece capturedPiece = board.GetPiece(move.TargetSquare);
    //     int capturedPieceValue = _pieceValues[(int)capturedPiece.PieceType];
    //     int movePieceValue = _pieceValues[(int)move.MovePieceType];
    //         
    //     if (capturedPieceValue > highestValueCapture)
    //     {
    //         highestValueCapture = capturedPieceValue;
    //     }
    //
    //     return capturedPieceValue >= movePieceValue || board.SquareIsAttackedByOpponent(move.TargetSquare) == false;
    // }
    
    // Test if this move gives checkmate
    // private bool MoveIsCheckmate(Board board, Move move)
    // {
    //     board.MakeMove(move);
    //     bool isMate = board.IsInCheckmate();
    //     board.UndoMove(move);
    //     return isMate;
    // }
    
    // bool MoveIsCheck(Board board, Move move)
    // {
    //     board.MakeMove(move);
    //     bool isCheck = board.IsInCheck();
    //     board.UndoMove(move);
    //     return isCheck;
    // }
    
    private float EvaluateMove(Board board, Move evMove, int depth, float alpha, float beta, bool isWhite)
    {
        int opponentMovesCount = board.GetLegalMoves().Length;
        board.MakeMove(evMove);
        var moves = board.GetLegalMoves();
        int who2Move = isWhite ? 1 : -1;

        float tempEval = float.NegativeInfinity;

        if (board.IsInCheckmate())
        {
            board.UndoMove(evMove);
            return 99999999.0f * who2Move;
        }
        
        if (moves.Length == 0) 
        {
            board.UndoMove(evMove); 
            return 0;
        }


        if (depth == 0)
        {
            int wMobility = isWhite ? moves.Length : opponentMovesCount;
            int bMobility = isWhite ? opponentMovesCount : moves.Length;
            
            board.UndoMove(evMove);
            return (100 * (board.GetPieceList(PieceType.Pawn, board.IsWhiteToMove).Count -
                          board.GetPieceList(PieceType.Pawn, !board.IsWhiteToMove).Count)
                   + 300 * (board.GetPieceList(PieceType.Knight, board.IsWhiteToMove).Count -
                            board.GetPieceList(PieceType.Knight, !board.IsWhiteToMove).Count)
                   + 300 * (board.GetPieceList(PieceType.Bishop, board.IsWhiteToMove).Count -
                            board.GetPieceList(PieceType.Bishop, !board.IsWhiteToMove).Count)
                   + 500 * (board.GetPieceList(PieceType.Rook, board.IsWhiteToMove).Count -
                            board.GetPieceList(PieceType.Rook, !board.IsWhiteToMove).Count)
                   + 900 * (board.GetPieceList(PieceType.Queen, board.IsWhiteToMove).Count -
                            board.GetPieceList(PieceType.Queen, !board.IsWhiteToMove).Count) + 5 * (wMobility-bMobility)) * who2Move;
        }

        
        foreach (var move in moves)
        {
            // if (isWhite)
            
                tempEval = Max(tempEval, -EvaluateMove(board, move, depth - 1, -beta, -alpha, !isWhite));
                alpha = Max(alpha, tempEval);

                if (alpha >= beta)
                {
                    Console.WriteLine("Skipped");
                    break;
                }
                else
                {
                    Console.WriteLine("Not Skipped");
                }
            
            // else
            // {
            //     tempEval = Min(tempEval, EvaluateMove(board, move, depth - 1, alpha, beta, !isWhite));
            //     beta = Min(beta, tempEval);
            //
            //     if (tempEval < alpha)
            //     {
            //         Console.WriteLine("Skipped");
            //         break;
            //     }
            //     else
            //     {
            //         Console.WriteLine("Not Skipped");
            //     }
            // }
        }
        
        board.UndoMove(evMove);
        return tempEval;
    }
    
    private float Max(float f1, float f2) {
        if (f1 >= f2) return f1; else return f2;
    }
    private float Min(float f1, float f2) {
        if (f1 <= f2) return f1; else return f2;
    }
    
    // private List<Piece> GetPassedPawns(Board board)
    // {
    //     var myBotPawns = board.GetPieceList(PieceType.Pawn, board.IsWhiteToMove);
    //     var opponentPawns = board.GetPieceList(PieceType.Pawn, !board.IsWhiteToMove);
    //
    //     List<Piece> passedPawns = new List<Piece>();
    //     
    //     for (int i = 0; i < myBotPawns.Count; i++)
    //     {
    //         bool addToList = true;
    //         for (int j = 0; j < opponentPawns.Count; j++)
    //         {
    //             if (myBotPawns[i].Square.File == opponentPawns[j].Square.File)
    //             {
    //                 addToList = false;
    //                 break;
    //             }
    //         }
    //
    //         if (addToList)
    //         {
    //             passedPawns.Add(myBotPawns[i]);
    //         }
    //     }
    //
    //     return passedPawns;
    // }
    //
    // private void UpdateMoveArray(Move newMove)
    // {
    //     for (int i = 0; i < _moves.Length; i++)
    //     {
    //         if (_moves[i] != Move.NullMove) continue;
    //         
    //         _moves[i] = newMove;
    //         break;
    //     }
    //
    //     if (_moves.Contains(newMove)) return;
    //     
    //     for (int i = _moves.Length - 1; i > 0; i--) 
    //     { 
    //         _moves[i] = _moves[i - 1];
    //     }
    //     _moves[0] = newMove;
    // }
}