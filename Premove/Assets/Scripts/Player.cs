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

    public int score = 0;

    public Player(TeamColor team, Board board)
    {
        teamColor = team;
        this.board = board;
        activePieces = new List<Piece>();
    }

    public void AddPiece(Piece piece)
    {
        if (!activePieces.Contains(piece))
        {
            activePieces.Add(piece);
            score += piece.pieceData.value;
        }
    }

    public void RemovePiece(Piece piece)
    {
        if (activePieces.Contains(piece))
            activePieces.Remove(piece);
    }

    /// <summary>
    /// Generates all the possible moves for the players pieces
    /// </summary>
    public void GenerateAllPossibleMoves(bool ignoreOwnPieces, bool blockOverride = false)
    {
        foreach(var piece in activePieces)
        {
            if (board.HasPiece(piece))
                piece.SelectAvailableSquares(ignoreOwnPieces, blockOverride);
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
    public void RemoveUnsafeMoves<T>(Player otherPlayer, Piece selectedPiece, bool ignoreOwnPieces = true) where T : Piece
    {
        if (selectedPiece == null)
            return;

        List<Vector2Int> coordsToRemove = new List<Vector2Int>();
        foreach(var coords in selectedPiece.availableMoves)
        {
            Piece pieceOnSquare = board.GetPieceOnSquare(coords);
            if(pieceOnSquare && !ignoreOwnPieces && pieceOnSquare.pieceData.teamColor == selectedPiece.pieceData.teamColor)
            {
                coordsToRemove.Add(coords);
                break;
            }

            board.UpdateBoardOnPieceMove(coords, selectedPiece.pieceData.square, selectedPiece, null);
            otherPlayer.GenerateAllPossibleMoves(false, true);
            if (otherPlayer.CheckIfAttackingPiece<King>())
                coordsToRemove.Add(coords);
            board.UpdateBoardOnPieceMove(selectedPiece.pieceData.square, coords, selectedPiece, pieceOnSquare);
        }
        
        foreach(var coords in coordsToRemove)
        {
            selectedPiece.availableMoves.Remove(coords);
        }
    }

    public void RemoveunsafeCastleMoves(Player otherPlayer, Piece selectedPiece)
    {
        List<Vector2Int> coordsToRemove = new List<Vector2Int>();
        for (int i = 1; i <= 3; i++)
        {
            Vector2Int coords = selectedPiece.pieceData.square + Vector2Int.left * i;
            Piece pieceOnSquare = board.GetPieceOnSquare(coords);
            board.UpdateBoardOnPieceMove(coords, selectedPiece.pieceData.square, selectedPiece, null);
            otherPlayer.GenerateAllPossibleMoves(false, true);
            if (otherPlayer.CheckIfAttackingPiece<King>())
            {
                for (int j = 1; j <= 3; j++)
                {
                    Vector2Int castleCoords = selectedPiece.pieceData.square + Vector2Int.left * j;
                    coordsToRemove.Add(castleCoords);
                }
                board.UpdateBoardOnPieceMove(selectedPiece.pieceData.square, coords, selectedPiece, pieceOnSquare);
                break;
            }
            board.UpdateBoardOnPieceMove(selectedPiece.pieceData.square, coords, selectedPiece, pieceOnSquare);
        }


            for (int i = 1; i <= 2; i++)
        {
            Vector2Int coords = selectedPiece.pieceData.square + Vector2Int.right * i;
            Piece pieceOnSquare = board.GetPieceOnSquare(coords);
            board.UpdateBoardOnPieceMove(coords, selectedPiece.pieceData.square, selectedPiece, null);
            otherPlayer.GenerateAllPossibleMoves(false, true);
            if (otherPlayer.CheckIfAttackingPiece<King>())
            {
                for (int j = 1; j <= 2; j++)
                {
                    Vector2Int castleCoords = selectedPiece.pieceData.square + Vector2Int.right * j;
                    coordsToRemove.Add(castleCoords);
                }
                board.UpdateBoardOnPieceMove(selectedPiece.pieceData.square, coords, selectedPiece, pieceOnSquare);
                break;
            }
            board.UpdateBoardOnPieceMove(selectedPiece.pieceData.square, coords, selectedPiece, pieceOnSquare);
        }

        foreach (var coords in coordsToRemove)
        {
            selectedPiece.availableMoves.Remove(coords);
        }
    }

    /// <summary>
    /// Checks if the player is attacking a piece of type
    /// </summary>
    public bool CheckIfAttackingPiece<T>() where T : Piece
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

                if (pieceOnCoords && pieceOnCoords.pieceData.teamColor == teamColor)
                    break;

                board.UpdateBoardOnPieceMove(coords, piece.pieceData.square, piece, null);
                otherPlayer.GenerateAllPossibleMoves(false, true);
                if(!otherPlayer.CheckIfAttackingPiece<King>())
                {
                    board.UpdateBoardOnPieceMove(piece.pieceData.square, coords, piece, pieceOnCoords);
                    return true;
                }
                board.UpdateBoardOnPieceMove(piece.pieceData.square, coords, piece, pieceOnCoords);
            }
        }

        return false;
    }

    public List<Vector2Int> GetAllAvailableMoves()
    {
        List<Vector2Int> availableMoves = new List<Vector2Int>();

        foreach(var piece in activePieces)
        {
            foreach(var move in piece.availableMoves)
            {
                availableMoves.Add(move);
            }
        }

        return availableMoves;
    }

    public bool CanAttackSquare(Vector2Int square)
    {
        foreach(var piece in activePieces)
        {
            piece.SelectAvailableSquares(true, true);

            if (piece.availableMoves.Contains(square))
                return true;
        }

        return false;
    }
}
