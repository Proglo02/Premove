using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player
{
    public TeamColor teamColor;
    public Board board;
    public List<Piece> activePieces { get; private set; }

    public Player(TeamColor team, Board board)
    {
        teamColor = team;
        this.board = board;
        activePieces = new List<Piece>();
    }

    public void AddPiece(Piece piece)
    {
        if (!activePieces.Contains(piece))
            activePieces.Add(piece);
    }

    public void RemovePiece(Piece piece)
    {
        if (activePieces.Contains(piece))
            activePieces.Remove(piece);
    }

    /// <summary>
    /// Generates all the possible moves for the players pieces
    /// </summary>
    public void GenerateAllPossibleMoves()
    {
        foreach(var piece in activePieces)
        {
            if (board.HasPiece(piece))
                piece.SelectAvailableSquares();
        }
    }

    /// <summary>
    /// Gets all the pieces that has the opposite king in check
    /// </summary>
    public Piece[] GetPiecesWithCheck()
    {
        return activePieces.Where(p => p.IsAttackingPieceOfType<King>()).ToArray();
    }

    /// <summary>
    /// Returns all pieces of type of the player
    /// </summary>
    public Piece[] GetPiecesOfType<T>() where T : Piece
    {
        return activePieces.Where(p => p is T).ToArray();
    }

    /// <summary>
    /// Removes all moves that would allow the piece to be taken
    /// </summary>
    public void RemoveUnsafeMoves<T>(Player otherPlayer, Piece selectedPiece) where T : Piece
    {
        List<Vector2Int> coordsToRemove = new List<Vector2Int>();
        foreach(var coords in selectedPiece.availableMoves)
        {
            Piece pieceOnSquare = board.GetPieceOnSquare(coords);
            board.UpdateBoardOnPieceMove(coords, selectedPiece.square, selectedPiece, null);
            otherPlayer.GenerateAllPossibleMoves();
            if (otherPlayer.CheckIfAttackingPiece<King>())
                coordsToRemove.Add(coords);
            board.UpdateBoardOnPieceMove(selectedPiece.square, coords, selectedPiece, pieceOnSquare);
        }
        
        foreach(var coords in coordsToRemove)
        {
            selectedPiece.availableMoves.Remove(coords);
        }
    }

    private bool CheckIfAttackingPiece<T>() where T : Piece
    {
        foreach(var piece in activePieces)
        {
            if (board.HasPiece(piece) && piece.IsAttackingPieceOfType<T>())
                return true;
        }

        return false;
    }

    /// <summary>
    /// Checks if there are any available pieces that can cover the king
    /// </summary>
    public bool CanCoverKing(Player otherPlayer)
    {
        foreach(var piece in activePieces)
        {
            foreach(var coords in piece.availableMoves)
            {
                Piece pieceOnCoords = board.GetPieceOnSquare(coords);
                board.UpdateBoardOnPieceMove(coords, piece.square, piece, null);
                otherPlayer.GenerateAllPossibleMoves();
                if(!otherPlayer.CheckIfAttackingPiece<King>())
                {
                    board.UpdateBoardOnPieceMove(piece.square, coords, piece, pieceOnCoords);
                    return true;
                }
                board.UpdateBoardOnPieceMove(piece.square, coords, piece, pieceOnCoords);
            }
        }

        return false;
    }
}
