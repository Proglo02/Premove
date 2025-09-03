using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Queen : Piece
{
    private Vector2Int[] directions = new Vector2Int[]
{
        new Vector2Int(1, 1),
        new Vector2Int(1, -1),
        new Vector2Int(-1, 1),
        new Vector2Int(-1, -1),
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
};
    public override List<Vector2Int> SelectAvailableSquares()
    {
        availableMoves.Clear();
        float range = Board.BOARD_WIDTH;

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
                else if (!piece.IsSameTeam(this))
                {
                    if (GameManager.Instance.gameState != GameState.Active && GameManager.Instance.gameState != GameState.Init)
                        break;

                    TryAddMove(nextCoords);

                    if (GameSettings.Instance.piecesBlockMoves)
                    {
                        i = (int)range;
                        break;
                    }
                }
                else if (piece.IsSameTeam(this))
                {
                    if (GameManager.Instance.gameState != GameState.Active && GameManager.Instance.gameState != GameState.Init)
                        break;

                    TryAddMove(nextCoords);

                    if (GameSettings.Instance.piecesBlockMoves)
                    {
                        i = (int)range;
                        break;
                    }
                }
            }
        }
        return availableMoves;
    }
}
