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

    public override List<Vector2Int> SelectAvailableSquares(bool ignoreOwnPieces, bool blockOverride = false)
    {
        availableMoves.Clear();
        AddStandardMoves(ignoreOwnPieces, blockOverride);
        AddTakeMoves(ignoreOwnPieces, blockOverride);
        AddEnPassantMoves();
        return availableMoves;
    }

    private void AddStandardMoves(bool ignoreOwnPieces, bool blockOverride = false)
    {
        Vector2Int direction = pieceData.teamColor == TeamColor.White ? Vector2Int.up : Vector2Int.down;
        int range = pieceData.numMoves > 0 ? 1 : 2;
        for (int i = 1; i <= range; i++)
        {
            Vector2Int nextCoords = pieceData.square + direction * i;
            Piece piece = board.GetPieceOnSquare(nextCoords);
            if (!board.CoordsOnBoard(nextCoords))
                break;
            if (piece == null)
                TryAddMove(nextCoords);
            else if(GameManager.Instance.gameState != GameState.Looping && !ignoreOwnPieces)
                TryAddMoveOnBlock(nextCoords, piece, blockOverride, out bool stopLooping, ignoreOwnPieces);
        }
    }

    private void AddTakeMoves(bool ignoreOwnPieces, bool blockOverride = false)
    {
        Vector2Int direction = pieceData.teamColor == TeamColor.White ? Vector2Int.up : Vector2Int.down;
        Vector2Int[] takeDirections = new Vector2Int[] { new Vector2Int(1, direction.y), new Vector2Int(-1, direction.y) };
        for (int i = 0; i < takeDirections.Length; i++)
        {
            Vector2Int nextCoords = pieceData.square + takeDirections[i];
            Piece piece = board.GetPieceOnSquare(nextCoords);
            if (!board.CoordsOnBoard(nextCoords))
                continue;
            if (piece != null && !piece.IsSameTeam(this))
                TryAddMove(nextCoords);
            else if(!ignoreOwnPieces || GameManager.Instance.gameState != GameState.Looping)
                TryAddMoveOnBlock(nextCoords, piece, blockOverride, out bool stopLooping, ignoreOwnPieces);
        }
    }

    private void AddEnPassantMoves()
    {
        Vector2Int direction = pieceData.teamColor == TeamColor.White ? Vector2Int.up : Vector2Int.down;

        leftPawn = (Pawn)TryGetPieceInDirection<Pawn>(pieceData.teamColor == TeamColor.White ? TeamColor.Black : TeamColor.White, Vector2Int.left);
        if (leftPawn && leftPawn.hasJumped)
        {
            leftEnPassantMove = pieceData.square + direction + Vector2Int.left;
            availableMoves.Add(leftEnPassantMove);
        }

        rightPawn = (Pawn)TryGetPieceInDirection<Pawn>(pieceData.teamColor == TeamColor.White ? TeamColor.Black : TeamColor.White, Vector2Int.right);
        if (rightPawn && rightPawn.hasJumped)
        {
            rightEnPassantMove = pieceData.square + direction + Vector2Int.right;
            availableMoves.Add(rightEnPassantMove);
        }
    }

    public override void MovePiece(Vector2Int coords , bool addMove = true, PieceData takenPiece = new PieceData(), bool doubleMove = false)
    {
        Vector2Int prevCoords = pieceData.square;

        base.MovePiece(coords, addMove, takenPiece);

        hasJumped = Mathf.Abs(prevCoords.y - pieceData.square.y) > 1;

        if(coords.y == (pieceData.teamColor == TeamColor.White ? 7 : 0))
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

    public override void ResetMove()
    {
        pieceData.numMoves--;
        hasJumped = false;
    }
}
