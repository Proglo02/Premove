using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : Piece
{
    public bool hasJumped = false;

    private Vector2Int leftEnPassantMove = new Vector2Int(-1, -1);
    private Vector2Int rightEnPassantMove = new Vector2Int(-1, -1);

    private Pawn leftPawn;
    private Pawn rightPawn;

    public override List<Vector2Int> SelectAvailableSquares()
    {
        availableMoves.Clear();
        AddStandardMoves();
        AddTakeMoves();
        AddEnPassantMoves();
        return availableMoves;
    }

    private void AddStandardMoves()
    {
        Vector2Int direction = teamColor == TeamColor.White ? Vector2Int.up : Vector2Int.down;
        int range = hasMoved ? 1 : 2;
        for (int i = 1; i <= range; i++)
        {
            Vector2Int nextCoords = square + direction * i;
            Piece piece = board.GetPieceOnSquare(nextCoords);
            if (!board.CoordsOnBoard(nextCoords))
                break;
            if (piece == null)
                TryAddMove(nextCoords);
            else
                break;
        }
    }

    private void AddTakeMoves()
    {
        Vector2Int direction = teamColor == TeamColor.White ? Vector2Int.up : Vector2Int.down;
        Vector2Int[] takeDirections = new Vector2Int[] { new Vector2Int(1, direction.y), new Vector2Int(-1, direction.y) };
        for (int i = 0; i < takeDirections.Length; i++)
        {
            Vector2Int nextCoords = square + takeDirections[i];
            Piece piece = board.GetPieceOnSquare(nextCoords);
            if (!board.CoordsOnBoard(nextCoords))
                continue;
            if (piece != null && !piece.IsSameTeam(this))
                TryAddMove(nextCoords);
        }
    }

    private void AddEnPassantMoves()
    {
        Vector2Int direction = teamColor == TeamColor.White ? Vector2Int.up : Vector2Int.down;

        leftPawn = (Pawn)TryGetPieceInDirection<Pawn>(teamColor == TeamColor.White ? TeamColor.Black : TeamColor.White, Vector2Int.left);
        if (leftPawn && leftPawn.hasJumped)
        {
            leftEnPassantMove = square + direction + Vector2Int.left;
            availableMoves.Add(leftEnPassantMove);
        }

        rightPawn = (Pawn)TryGetPieceInDirection<Pawn>(teamColor == TeamColor.White ? TeamColor.Black : TeamColor.White, Vector2Int.right);
        if (rightPawn && rightPawn.hasJumped)
        {
            rightEnPassantMove = square + direction + Vector2Int.right;
            availableMoves.Add(rightEnPassantMove);
        }
    }

    public override void MovePiece(Vector2Int coords)
    {
        Vector2Int prevCoords = square;

        base.MovePiece(coords);

        hasJumped = Mathf.Abs(prevCoords.y - square.y) > 1;

        if(coords.y == (teamColor == TeamColor.White ? 7 : 0))
        {
            board.PromotePiece(this);
        }

        if (coords == leftEnPassantMove)
        {
            board.TakePiece(leftPawn);
        }
        else if (coords == rightEnPassantMove)
        {
            board.TakePiece(rightPawn);
        }
    }
}
