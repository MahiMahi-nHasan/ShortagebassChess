using System.Collections.Generic;
using UnityEngine;

public class ChessAI : MonoBehaviour
{
    public int maxPly = 4;
    public GameManager manager;

    public Move GetBestMove(bool maximizingPlayer)
    {
        PieceData?[,] board = manager.GetBoardState();

        List<Move> moves = BoardLogic.GetLegalMoves(board, maximizingPlayer);
        moves = FilterValidMoves(moves);  
        moves = OrderMoves(board, moves);

        if (moves.Count == 0)
            return default; 

        Move bestMove = moves[0];
        float bestScore = maximizingPlayer ? float.NegativeInfinity : float.PositiveInfinity;

        foreach (Move move in moves)
        {
            if (!IsOnBoard(move.from) || !IsOnBoard(move.to))
                continue;

            PieceData?[,] newBoard = BoardLogic.SimulateMove(board, move.from, move.to);

            if (IsKingInCheck(newBoard, maximizingPlayer))
                continue;

            float score = Minimax(newBoard, maxPly - 1, !maximizingPlayer,
                                  float.NegativeInfinity, float.PositiveInfinity);

            if (maximizingPlayer && score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
            else if (!maximizingPlayer && score < bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
        }

        return bestMove;
    }

    float Minimax(PieceData?[,] board, int depth, bool maximizingPlayer, float alpha, float beta)
    {
        if (depth == 0)
            return EvaluateBoard(board);

        List<Move> moves = BoardLogic.GetLegalMoves(board, maximizingPlayer);
        moves = FilterValidMoves(moves);
        moves = OrderMoves(board, moves);

        if (moves.Count == 0)
            return EvaluateBoard(board);

        if (maximizingPlayer)
        {
            float bestScore = float.NegativeInfinity;

            foreach (Move move in moves)
            {
                if (!IsOnBoard(move.from) || !IsOnBoard(move.to))
                    continue;

                var newBoard = BoardLogic.SimulateMove(board, move.from, move.to);

                if (IsKingInCheck(newBoard, maximizingPlayer))
                    continue;

                float score = Minimax(newBoard, depth - 1, false, alpha, beta);
                bestScore = Mathf.Max(bestScore, score);

                alpha = Mathf.Max(alpha, score);
                if (beta <= alpha) break;
            }

            return bestScore;
        }
        else
        {
            float bestScore = float.PositiveInfinity;

            foreach (Move move in moves)
            {
                if (!IsOnBoard(move.from) || !IsOnBoard(move.to))
                    continue;

                var newBoard = BoardLogic.SimulateMove(board, move.from, move.to);

                if (IsKingInCheck(newBoard, maximizingPlayer))
                    continue;

                float score = Minimax(newBoard, depth - 1, true, alpha, beta);
                bestScore = Mathf.Min(bestScore, score);

                beta = Mathf.Min(beta, score);
                if (beta <= alpha) break;
            }

            return bestScore;
        }
    }

    List<Move> FilterValidMoves(List<Move> moves)
    {
        var valid = new List<Move>();
        foreach (var move in moves)
            if (IsOnBoard(move.from) && IsOnBoard(move.to))
                valid.Add(move);
        return valid;
    }
    List<Move> OrderMoves(PieceData?[,] board, List<Move> moves)
    {
        // Precompute scores so comparison is stable
        var scoredMoves = new List<(Move move, int score)>();
        foreach (var move in moves)
        {
            scoredMoves.Add((move, MoveScore(board, move)));
        }

        // Sort by precomputed score descending
        scoredMoves.Sort((a, b) => b.score.CompareTo(a.score));

        // Extract sorted moves
        var sortedMoves = new List<Move>();
        foreach (var item in scoredMoves)
            sortedMoves.Add(item.move);

        return sortedMoves;
    }

    int MoveScore(PieceData?[,] board, Move move)
    {
        // Ensure move is within board bounds
        if (!IsOnBoard(move.from) || !IsOnBoard(move.to))
            return 0;

        var target = board[move.to.x, move.to.y];
        if (target != null)
        {
            // Score is the value of the piece being captured
            return GetPieceValue(target.Value.type);
        }

        return 0; // non-capture moves have neutral score
    }

    bool IsOnBoard(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < 8 && pos.y >= 0 && pos.y < 8;
    }
    bool IsKingInCheck(PieceData?[,] board, bool whiteKing)
    {
        Vector2Int kingPos = FindKing(board, whiteKing ? PieceColor.White : PieceColor.Black);

        PieceColor enemyColor = whiteKing ? PieceColor.Black : PieceColor.White;

        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                var piece = board[x, y];
                if (piece == null || piece.Value.color != enemyColor)
                    continue;

                Vector2Int from = new Vector2Int(x, y);
                var moves = BoardLogic.GetMovesForPiece(board, from, piece.Value);

                foreach (var move in moves)
                {
                    if (move == kingPos)
                        return true;
                }
            }
        }

        return false;
    }

    Vector2Int FindKing(PieceData?[,] board, PieceColor color)
    {
        for (int x = 0; x < 8; x++)
            for (int y = 0; y < 8; y++)
                if (board[x, y]?.type == PieceType.King &&
                    board[x, y]?.color == color)
                    return new Vector2Int(x, y);

        return new Vector2Int(-1, -1);
    }

    float EvaluateBoard(PieceData?[,] board)
    {
        float score = 0f;

        int whiteMobility = 0;
        int blackMobility = 0;

        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                var piece = board[x, y];
                if (piece == null) continue;

                int value = GetPieceValue(piece.Value.type);

                Vector2Int from = new Vector2Int(x, y);
                var moves = BoardLogic.GetMovesForPiece(board, from, piece.Value);

                // count only pseudo-legal mobility
                int mobility = moves.Count;

                if (piece.Value.color == PieceColor.White)
                {
                    score += value;
                    whiteMobility += mobility;
                }
                else
                {
                    score -= value;
                    blackMobility += mobility;
                }
            }
        }

        float mobilityWeight = 0.1f;

        score += (whiteMobility - blackMobility) * mobilityWeight;

        return score;
    }

    int GetPieceValue(PieceType type)
    {
        switch (type)
        {
            case PieceType.Pawn: return 1;
            case PieceType.Knight: return 3;
            case PieceType.Bishop: return 3;
            case PieceType.Rook: return 5;
            case PieceType.Queen: return 9;
            case PieceType.King: return 1000;
            default: return 0;
        }
    }
}