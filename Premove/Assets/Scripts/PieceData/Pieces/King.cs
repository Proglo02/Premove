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

    public override List<Vector2Int> SelectAvailableSquares(bool ignoreOwnPieces, bool blockOverride = false)
    {
        availableMoves.Clear();
        AddStandardMoves(ignoreOwnPieces, blockOverride);
        AddCastlingMoves(ignoreOwnPieces, blockOverride);
        return availableMoves;
    }
    private void AddStandardMoves(bool ignoreOwnPieces, bool blockOverride = false)
    {
        int range = 1;

        foreach (var direction in directions)
        {
            for (int i = 1; i <= range; i++)
            {
                Vector2Int nextCoords = square + direction * i;
                Piece piece = board.GetPieceOnSquare(nextCoords);
                if (!board.CoordsOnBoard(nextCoords))
                    break;
                if (piece == null)
                    TryAddMove(nextCoords);
                else
                    TryAddMoveOnBlock(nextCoords, piece, blockOverride, out bool stopLooping, ignoreOwnPieces);
            }
        }
    }
    private void AddCastlingMoves(bool ignoreOwnPieces, bool blockOverride = false)
    {
        if (hasMoved || blockOverride)
            return;

        leftRook = TryGetPieceInDirection<Rook>(teamColor, Vector2Int.left, GameSettings.Instance.piecesBlockMoves);
        if(leftRook && !leftRook.hasMoved)
        {
            leftCastlingMove = square + Vector2Int.left * 2;
            availableMoves.Add(leftCastlingMove);
        }

        rightRook = TryGetPieceInDirection<Rook>(teamColor, Vector2Int.right, GameSettings.Instance.piecesBlockMoves);
        if (rightRook && !rightRook.hasMoved)
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
            board.TakePiece(board.GetPieceOnSquare(coords + Vector2Int.right));
            board.UpdateBoardOnPieceMove(coords + Vector2Int.right, leftRook.square, leftRook, null);
            leftRook.MovePiece(coords + Vector2Int.right);
        }
        else if (coords == rightCastlingMove)
        {
            board.TakePiece(board.GetPieceOnSquare(coords + Vector2Int.left));
            board.UpdateBoardOnPieceMove(coords + Vector2Int.left, rightRook.square, rightRook, null);
            rightRook.MovePiece(coords + Vector2Int.left);
        }
    }
}
