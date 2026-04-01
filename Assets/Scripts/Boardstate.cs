using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardState
{
    public PieceData[,] grid = new PieceData[8, 8];
}

public enum PieceColor { White, Black }

public struct PieceData
{
    public PieceType type;
    public PieceColor color;

    public PieceData(PieceType t, PieceColor c)
    {
        type = t;
        color = c;
    }
}
public struct Move
{
    public Vector2Int from;
    public Vector2Int to;

    public Move(Vector2Int f, Vector2Int t)
    {
        from = f;
        to = t;
    }
}
