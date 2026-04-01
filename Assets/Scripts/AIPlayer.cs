using UnityEngine;

public class AIPlayer : MonoBehaviour
{
    public Player controlledPlayer;
    private bool hasMovedThisTurn = false;

    void Update()
    {
        if (GameManager.instance.currentPlayer != controlledPlayer)
        {
            hasMovedThisTurn = false; // reset when it's not AI's turn
            return;
        }

        if (!hasMovedThisTurn)
        {
            hasMovedThisTurn = true;
            ThinkAndMove();
        }
    }

    void ThinkAndMove()
    {
        Debug.Log("Turn Started");
        ChessAI ai = GetComponent<ChessAI>();
        Move bestMove = ai.GetBestMove(controlledPlayer.color == PieceColor.White);
        Debug.Log("Found A Move");

        GameObject movingPiece = GameManager.instance.PieceAtGrid(bestMove.from);
        GameObject targetPiece = GameManager.instance.PieceAtGrid(bestMove.to);

        if (targetPiece != null)
        {
            GameManager.instance.CapturePieceAt(bestMove.to);
        }

        GameManager.instance.Move(movingPiece, bestMove.to);

        GameManager.instance.NextPlayer();
    }
}