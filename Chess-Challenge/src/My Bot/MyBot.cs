﻿using System;
using System.Collections.Generic;
using System.Linq;
using ChessChallenge.API;
using ChessChallenge.Application.APIHelpers;

public class MyBot : IChessBot
{
    private Move[] _moves = new Move[4];
    // Piece values: null, pawn, knight, bishop, rook, queen, king
    private readonly int[] _pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };
    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();
        var random = new Random();

        var moveToPlay = new Move();
        int highestValueCapture = 0;

        var passedPawns = GetPassedPawns(board);

        List<Move> goodMoves = new List<Move>();
        
        goodMoves.AddRange(moves);

        List<Move> startSquareAttackedMoves = new List<Move>();

        for (int i = 0; i < moves.Length; i++)
        {
            if (_moves.Contains(moves[i]))
            {
                goodMoves.Remove(moves[i]);
                continue;
            }
            
            if (MoveIsCheckmate(board, moves[i]))
            {
                moveToPlay = moves[i];
                break;
            }

            bool targetSquareSafe = board.SquareIsAttackedByOpponent(moves[i].TargetSquare) == false;
            
            if (MoveIsCheck(board, moves[i]) && targetSquareSafe)
            {
                moveToPlay = moves[i];
                break;
            }

            // Find highest value capture
            if(FindBestCapture(board, moves[i], ref highestValueCapture))
            {
                moveToPlay = moves[i];
            }

            if (board.SquareIsAttackedByOpponent(moves[i].StartSquare))
            {
                startSquareAttackedMoves.Add(moves[i]);
            }
            
            for (int j = 0; j < passedPawns.Count; j++) 
            {
                if (moves[i].StartSquare == passedPawns[j].Square &&
                    targetSquareSafe)
                {
                    moveToPlay = moves[i];
                    break;
                }
            }

            if (moveToPlay != moves[i] && !targetSquareSafe)
            {
                goodMoves.Remove(moves[i]);
            }
        }

        int highestValueCounterTake = 0;
        if (startSquareAttackedMoves.Count != 0)
        {
            goodMoves.Clear();
            moveToPlay = new Move();
            for (int i = 0; i < startSquareAttackedMoves.Count; i++)
            {
                if (FindBestCapture(board, startSquareAttackedMoves[i], ref highestValueCounterTake))
                {
                    goodMoves.Add(startSquareAttackedMoves[i]);
                }
            }
        }
        
        if (moveToPlay == Move.NullMove && goodMoves.Count != 0)
        {
            moveToPlay = goodMoves[random.Next(0, goodMoves.Count)];
        }
        else if(moveToPlay == Move.NullMove)
        {
            moveToPlay = moves[random.Next(0, moves.Length)];
        }

        UpdateMoveArray(moveToPlay);
        
        return moveToPlay;
    }

    private bool FindBestCapture(Board board, Move move, ref int highestValueCapture)
    {
        Piece capturedPiece = board.GetPiece(move.TargetSquare);
        int capturedPieceValue = _pieceValues[(int)capturedPiece.PieceType];
        int movePieceValue = _pieceValues[(int)move.MovePieceType];
            
        if (capturedPieceValue > highestValueCapture)
        {
            highestValueCapture = capturedPieceValue;
        }

        return capturedPieceValue >= movePieceValue || board.SquareIsAttackedByOpponent(move.TargetSquare) == false;
    }
    
    // Test if this move gives checkmate
    private bool MoveIsCheckmate(Board board, Move move)
    {
        board.MakeMove(move);
        bool isMate = board.IsInCheckmate();
        board.UndoMove(move);
        return isMate;
    }
    
    bool MoveIsCheck(Board board, Move move)
    {
        board.MakeMove(move);
        bool isCheck = board.IsInCheck();
        board.UndoMove(move);
        return isCheck;
    }

    private float EvaluateMove(Board board, Move move)
    {
        board.MakeMove(move);
        var moves = board.GetLegalMoves();
        
        for (int i = 0; i < moves.Length; i++)
        {
        }
        
        board.UndoMove(move);
        return 1.0f;
    }

    private List<Piece> GetPassedPawns(Board board)
    {
        var myBotPawns = board.GetPieceList(PieceType.Pawn, board.IsWhiteToMove);
        var opponentPawns = board.GetPieceList(PieceType.Pawn, !board.IsWhiteToMove);

        List<Piece> passedPawns = new List<Piece>();
        
        for (int i = 0; i < myBotPawns.Count; i++)
        {
            bool addToList = true;
            for (int j = 0; j < opponentPawns.Count; j++)
            {
                if (myBotPawns[i].Square.File == opponentPawns[j].Square.File)
                {
                    addToList = false;
                    break;
                }
            }

            if (addToList)
            {
                passedPawns.Add(myBotPawns[i]);
            }
        }

        return passedPawns;
    }

    private void UpdateMoveArray(Move newMove)
    {
        for (int i = 0; i < _moves.Length; i++)
        {
            if (_moves[i] != Move.NullMove) continue;
            
            _moves[i] = newMove;
            break;
        }

        if (_moves.Contains(newMove)) return;
        
        for (int i = _moves.Length - 1; i > 0; i--) 
        { 
            _moves[i] = _moves[i - 1];
        }
        _moves[0] = newMove;
    }
}