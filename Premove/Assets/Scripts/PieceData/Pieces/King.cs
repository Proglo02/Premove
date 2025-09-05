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
                Vector2Int nextCoords = pieceData.square + direction * i;
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
        if (pieceData.numMoves > 0 || blockOverride)
            return;

        leftRook = TryGetPieceInDirection<Rook>(pieceData.teamColor, Vector2Int.left, GameSettings.Instance.piecesBlockMoves);
        if(leftRook && leftRook.pieceData.numMoves <= 0)
        {
            leftCastlingMove = pieceData.square + Vector2Int.left * 2;
            availableMoves.Add(leftCastlingMove);
        }

        rightRook = TryGetPieceInDirection<Rook>(pieceData.teamColor, Vector2Int.right, GameSettings.Instance.piecesBlockMoves);
        if (rightRook && rightRook.pieceData.numMoves <= 0)
        {
            rightCastlingMove = pieceData.square + Vector2Int.right * 2;
            availableMoves.Add(rightCastlingMove);
        }
    }

    public override void MovePiece(Vector2Int coords, bool addMove = true, PieceData takenPiece = new PieceData(), bool doubleMove = false)
    {
        base.MovePiece(coords, addMove, takenPiece);
        if(coords == leftCastlingMove)
        {
            Piece pieceToTake = board.GetPieceOnSquare(coords + Vector2Int.right);

            PieceData pieceData = new PieceData();
            if (pieceToTake)
            {
                pieceData.type = pieceToTake.GetType();
                pieceData.square = pieceToTake.pieceData.square;
                pieceData.teamColor = pieceToTake.pieceData.teamColor;
                pieceData.value = pieceToTake.pieceData.value;
                pieceData.id = pieceToTake.pieceData.id;
                pieceData.numMoves = pieceToTake.pieceData.numMoves;
            }

            board.TakePiece(pieceToTake);
            board.UpdateBoardOnPieceMove(coords + Vector2Int.right, leftRook.pieceData.square, leftRook, null);
            leftRook.MovePiece(coords + Vector2Int.right, addMove, pieceData, true);
        }
        else if (coords == rightCastlingMove)
        {
            Piece pieceToTake = board.GetPieceOnSquare(coords + Vector2Int.left);

            PieceData pieceData = new PieceData();
            if (pieceToTake)
            {
                pieceData = pieceToTake.pieceData;
            }

            board.TakePiece(pieceToTake);
            board.UpdateBoardOnPieceMove(coords + Vector2Int.left, rightRook.pieceData.square, rightRook, null);
            rightRook.MovePiece(coords + Vector2Int.left, addMove, pieceData, true);
        }
    }
}
