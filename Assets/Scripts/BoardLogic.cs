using System.Collections.Generic;
using UnityEngine;

public static class BoardLogic
{
    static readonly Vector2Int[] rookDirs = {
        new Vector2Int(1,0), new Vector2Int(-1,0),
        new Vector2Int(0,1), new Vector2Int(0,-1)
    };

    static readonly Vector2Int[] bishopDirs = {
        new Vector2Int(1,1), new Vector2Int(1,-1),
        new Vector2Int(-1,1), new Vector2Int(-1,-1)
    };

    static readonly Vector2Int[] queenDirs = {
        new Vector2Int(1,0), new Vector2Int(-1,0),
        new Vector2Int(0,1), new Vector2Int(0,-1),
        new Vector2Int(1,1), new Vector2Int(1,-1),
        new Vector2Int(-1,1), new Vector2Int(-1,-1)
    };
    static readonly Vector2Int[] knightOffsets = {
        new Vector2Int(1,2), new Vector2Int(2,1),
        new Vector2Int(-1,2), new Vector2Int(-2,1),
        new Vector2Int(1,-2), new Vector2Int(2,-1),
        new Vector2Int(-1,-2), new Vector2Int(-2,-1)
    };

    public static PieceData? GetPiece(PieceData?[,] board, Vector2Int pos)
    {
        if (pos.x < 0 || pos.x > 7 || pos.y < 0 || pos.y > 7)
            return null;

        return board[pos.x, pos.y];
    }

    public static PieceData?[,] Clone(PieceData?[,] original)
    {
        PieceData?[,] clone = new PieceData?[8, 8];

        for (int x = 0; x < 8; x++)
            for (int y = 0; y < 8; y++)
                clone[x, y] = original[x, y];

        return clone;
    }

    public static PieceData?[,] SimulateMove(
        PieceData?[,] board,
        Vector2Int from,
        Vector2Int to)
    {
        PieceData?[,] newBoard = Clone(board);

        newBoard[to.x, to.y] = newBoard[from.x, from.y];
        newBoard[from.x, from.y] = null;

        return newBoard;
    }
    public static List<Move> GetLegalMoves(PieceData?[,] board, bool maximizingPlayer)
    {
        List<Move> legalMoves = new List<Move>();
        PieceColor currentColor = maximizingPlayer ? PieceColor.White : PieceColor.Black;

        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                var piece = board[x, y];

                // Skip empty squares or squares of opponent pieces
                if (piece == null || piece.Value.color != currentColor)
                    continue;

                Vector2Int from = new Vector2Int(x, y);
               
                List<Vector2Int> destinations = GetMovesForPiece(board, from, piece.Value);

                foreach (Vector2Int to in destinations)
                {
                    legalMoves.Add(new Move(from, to));
                }
            }
        }

        return legalMoves;
    }

    public static List<Vector2Int> GetMovesForPiece(
    PieceData?[,] board,
    Vector2Int from,
    PieceData piece)
    {
        List<Vector2Int> moves = new List<Vector2Int>();

        switch (piece.type)
        {
            case PieceType.Pawn:
                AddPawnMoves(board, from, piece, moves);
                break;

            case PieceType.Knight:
                AddKnightMoves(board, from, piece, moves);
                break;

            case PieceType.Bishop:
                AddSlidingMoves(board, from, piece, moves, bishopDirs);
                break;

            case PieceType.Rook:
                AddSlidingMoves(board, from, piece, moves, rookDirs);
                break;

            case PieceType.Queen:
                AddSlidingMoves(board, from, piece, moves, queenDirs);
                break;

            case PieceType.King:
                AddKingMoves(board, from, piece, moves);
                break;
        }

        return moves;
    }
    static void AddPawnMoves(PieceData?[,] board, Vector2Int from, PieceData piece, List<Vector2Int> moves)
    {
        int dir = piece.color == PieceColor.White ? 1 : -1;

        Vector2Int forward = new Vector2Int(from.x, from.y + dir);

        if (GetPiece(board, forward) == null)
            moves.Add(forward);

        Vector2Int[] captures = {
        new Vector2Int(from.x + 1, from.y + dir),
        new Vector2Int(from.x - 1, from.y + dir)
    };

        foreach (var pos in captures)
        {
            var target = GetPiece(board, pos);
            if (target != null && target.Value.color != piece.color)
                moves.Add(pos);
        }
    }

    static void AddKnightMoves(PieceData?[,] board, Vector2Int from, PieceData piece, List<Vector2Int> moves)
    {
        foreach (var offset in knightOffsets)
        {
            Vector2Int to = from + offset;
            var target = GetPiece(board, to);

            if (target == null || target.Value.color != piece.color)
                moves.Add(to);
        }
    }
    static void AddSlidingMoves(
    PieceData?[,] board,
    Vector2Int from,
    PieceData piece,
    List<Vector2Int> moves,
    Vector2Int[] directions)
    {
        foreach (var dir in directions)
        {
            Vector2Int pos = from;

            while (true)
            {
                pos += dir;

                var target = GetPiece(board, pos);
                if (pos.x < 0 || pos.x > 7 || pos.y < 0 || pos.y > 7)
                    break;

                if (target == null)
                {
                    moves.Add(pos);
                }
                else
                {
                    if (target.Value.color != piece.color)
                        moves.Add(pos);
                    break;
                }
            }
        }
    }
    static void AddKingMoves(PieceData?[,] board, Vector2Int from, PieceData piece, List<Vector2Int> moves)
    {
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;

                Vector2Int to = new Vector2Int(from.x + dx, from.y + dy);
                var target = GetPiece(board, to);

                if (target == null || target.Value.color != piece.color)
                    moves.Add(to);
            }
        }
    }

}