using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knight : Piece
{
    Vector2Int[] offsets = new Vector2Int[]
    {
        new Vector2Int(2, 1),
        new Vector2Int(2, -1),
        new Vector2Int(1, 2),
        new Vector2Int(1, -2),
        new Vector2Int(-2, 1),
        new Vector2Int(-2, -1),
        new Vector2Int(-1, 2),
        new Vector2Int(-1, -2),
    };
    public override List<Vector2Int> SelectAvailableSquares(bool ignoreOwnPieces, bool blockOverride = false)
    {
        availableMoves.Clear();
        for(int i = 0; i < offsets.Length; i++)
        {
            Vector2Int nextCoords = pieceData.square + offsets[i];
            Piece piece = board.GetPieceOnSquare(nextCoords);
            if (!board.CoordsOnBoard(nextCoords))
                continue;
            if (piece == null)
                TryAddMove(nextCoords);
            else
                TryAddMoveOnBlock(nextCoords, piece, blockOverride, out bool stopLooping, ignoreOwnPieces);
        }
        return availableMoves;
    }
}
