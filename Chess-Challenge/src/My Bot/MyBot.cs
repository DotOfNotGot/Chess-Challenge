using System;
using System.Collections.Generic;
using System.Linq;
using ChessChallenge.API;
using ChessChallenge.Application.APIHelpers;
using ChessChallenge.Chess;
using Board = ChessChallenge.API.Board;
using Move = ChessChallenge.API.Move;

public struct EvaluatedMove
{
    public Move move;
    public float evaluation;
}

public class MyBot : IChessBot
{

    private bool _isWhite;
    private float _timerSize;

    private int _depth = 3;
    private Move _bestMove;
    private float _bestScore;


    private int[] _pieceValues = { 0, 100, 300, 310, 500, 900, 10000 };

    public Move Think(Board board, Timer timer)
    {
        _timerSize = timer.MillisecondsRemaining;
        
        Span<Move> moves = stackalloc Move[256];
        board.GetLegalMovesNonAlloc(ref moves);

        EvaluateMove(board, _depth, float.NegativeInfinity, float.PositiveInfinity, board.IsWhiteToMove ? 1 : -1);
        Console.WriteLine($"Best score: {_bestScore}, {_bestMove}");

        return _bestMove;
    }

    private float EvaluateMove(Board board, int depth, float alpha, float beta, int color)
    {
        Move[] moves;

        if (board.IsDraw()) return 0;

        if (depth == 0 || (moves = board.GetLegalMoves()).Length == 0)
        {
            //int wMobility = color ? moves.Length : opponentMovesCount;
            //int bMobility = color ? opponentMovesCount : moves.Length;

            float sum = 0.0f;

            if (board.IsInCheckmate()) return -99999999.0f;

            for (int i = 0; ++i < 7;)
                sum += (board.GetPieceList((PieceType)i, true).Count - (board.GetPieceList((PieceType)i, false).Count * _pieceValues[i]));

            return color * sum;
        }

        float recordValue = float.NegativeInfinity;

        foreach (var move in moves)
        {
            board.MakeMove(move);
            float value = -EvaluateMove(board, depth - 1, -beta, -alpha, -color);
            board.UndoMove(move);

            if(recordValue < value)
            {
                recordValue = value;
                if (depth == _depth)
                {
                    _bestMove = move;
                    _bestScore = recordValue;
                }
            }

            alpha = Max(alpha, recordValue);
            if (alpha >= beta) break;
        }

        return recordValue;
    }

    private float Max(float f1, float f2)
    {
        if (f1 >= f2) return f1; else return f2;
    }
    private float Min(float f1, float f2)
    {
        if (f1 <= f2) return f1; else return f2;
    }

    private int Clamp(int value, int min, int max)
    {
        if(value < min) return min; 
        if(value > max) return max; 
        return value;
    }
}