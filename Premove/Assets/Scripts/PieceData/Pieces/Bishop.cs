using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bishop : Piece
{
    private Vector2Int[] directions = new Vector2Int[]
    {
        new Vector2Int(1, 1),
        new Vector2Int(1, -1),
        new Vector2Int(-1, 1),
        new Vector2Int(-1, -1)
    };

    public override List<Vector2Int> SelectAvailableSquares(bool ignoreOwnPieces, bool blockOverride = false)
    {
        availableMoves.Clear();
        int range = Board.BOARD_WIDTH;

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
                {
                    if(TryAddMoveOnBlock(nextCoords, piece, blockOverride, out bool stopLooping, ignoreOwnPieces))
                    {
                        i = stopLooping ? range : i;
                        break;
                    }
                }
            }
        }
        return availableMoves;
    }
}
