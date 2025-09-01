using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rook : Piece
{
    private Vector2Int[] directions = new Vector2Int[] { Vector2Int.left, Vector2Int.up, Vector2Int.right, Vector2Int.down };

    public override List<Vector2Int> SelectAvailableSquares()
    {
        availableMoves.Clear();
        float range = Board.BOARD_WIDTH;

        foreach(var direction in directions)
        {
            for(int i = 1; i <= range; i++)
            {
                Vector2Int nextCoords = square + direction * i;
                Piece piece = board.GetPieceOnSquare(nextCoords);
                if (!board.CoordsOnBoard(nextCoords))
                    break;
                if (piece == null)
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
        return availableMoves;
    }
}
