using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class King : Piece
{
    Vector2Int[] directions = new Vector2Int[]
    {
        new Vector2Int(-1, 1),
        new Vector2Int(0, 1),
        new Vector2Int(1, 1),
        new Vector2Int(-1, 0),
        new Vector2Int(1, 0),
        new Vector2Int(-1, -1),
        new Vector2Int(0, -1),
        new Vector2Int(1, -1),
    };

    private Vector2Int leftCastlingMove;
    private Vector2Int rightCastlingMove;

    private Piece leftRook;
    private Piece rightRook;

    public override List<Vector2Int> SelectAvailableSquares()
    {
        availableMoves.Clear();
        AddStandardMoves();
        AddCastlingMoves();
        return availableMoves;
    }
    private void AddStandardMoves()
    {
        float range = 1;

        foreach (var direction in directions)
        {
            for (int i = 1; i <= range; i++)
            {
                Vector2Int nextCoords = square + direction * i;
                Piece piece = board.GetPieceOnSquare(nextCoords);
                if (!board.CoordsOnBoard(nextCoords))
                    break;
                if (piece == null || !GameSettings.Instance.piecesBlockMoves)
                    TryAddMove(nextCoords);
                else if (!piece.IsSameTeam(this))
                {
                    TryAddMove(nextCoords);
                    break;
                }
                else if (piece.IsSameTeam(this))
                    break;
            }
        }
    }
    private void AddCastlingMoves()
    {
        if (hasMoved)
            return;

        leftRook = TryGetPieceInDirection<Rook>(teamColor, Vector2Int.left);
        if(leftRook && !leftRook.hasMoved)
        {
            leftCastlingMove = square + Vector2Int.left * 2;
            availableMoves.Add(leftCastlingMove);
        }

        rightRook = TryGetPieceInDirection<Rook>(teamColor, Vector2Int.right);
        if(rightRook && !rightRook.hasMoved)
        {
            rightCastlingMove = square + Vector2Int.right * 2;
            availableMoves.Add(rightCastlingMove);
        }
    }

    public override void MovePiece(Vector2Int coords, bool addMove = true)
    {
        base.MovePiece(coords, addMove);
        if(coords == leftCastlingMove)
        {
            board.UpdateBoardOnPieceMove(coords + Vector2Int.right, leftRook.square, leftRook, null);
            leftRook.MovePiece(coords + Vector2Int.right);
        }
        else if (coords == rightCastlingMove)
        {
            board.UpdateBoardOnPieceMove(coords + Vector2Int.left, rightRook.square, rightRook, null);
            rightRook.MovePiece(coords + Vector2Int.left);
        }
    }
}
