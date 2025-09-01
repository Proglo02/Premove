using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(HighlightManager))]
public class Board : MonoBehaviour
{
    [SerializeField] private Transform bottomLeftsquare;
    [SerializeField] private float squareWidth;

    private GameManager gameManager;
    private HighlightManager highlightManager;

    private Piece[,] grid;
    private Piece selectedPiece;

    public const int boardWidth = 8;

    private void Awake()
    {
        highlightManager = GetComponent<HighlightManager>();
        CreateGrid();
    }

    public void SetDependencies(GameManager gameManager)
    {
        this.gameManager = gameManager;
    }

    private void CreateGrid()
    {
        grid = new Piece[boardWidth, boardWidth];
    }

    /// <summary>  
    /// Get the world position from the given board coordinates  
    /// </summary>  
    public Vector3 CalculatePositionFromCoords(Vector2Int coords)
    {
        return bottomLeftsquare.position + new Vector3(-coords.y * squareWidth, 0f, coords.x * squareWidth);
    }

    internal bool HasPiece(Piece piece)
    {
        //Check if the piece exists on the board
        for (int i = 0; i < boardWidth; i++)
        {
            for (int j = 0; j < boardWidth; j++)
            {
                if (grid[i, j] == piece)
                {
                    return true;
                }
            }
        }
        return false;
    }

    internal void OnSquareSelected(Vector3 inputPosition)
    {
        //Only pass if game is active
        if (!gameManager.IsGameInProgress())
            return;

        Vector2Int coords = CalculateCoordsFromPosition(inputPosition);
        Piece piece = GetPieceOnSquare(coords);
        if(selectedPiece)
        {
            if (piece && selectedPiece == piece)
                DeselectPiece();
            else if (piece && selectedPiece != piece && gameManager.IsTeamTurnActive(piece.teamColor))
                SelectPiece(piece);
            else if(selectedPiece.CanMoveTo(coords))
                MoveSelectedPiece(coords, selectedPiece);
        }
        else
        {
            if(piece != null && gameManager.IsTeamTurnActive(piece.teamColor))
                SelectPiece(piece);
        }
    }

    private void SelectPiece(Piece piece)
    {
        gameManager.RemoveUnsafeMoves<King>(piece);
        selectedPiece = piece;
        List<Vector2Int> moves = selectedPiece.availableMoves;
        ShowMoves(moves);
    }

    //Creates data for the available highlights
    private void ShowMoves(List<Vector2Int> moves)
    {
        Dictionary<Vector3, bool> squaresData = new Dictionary<Vector3, bool>();
        for(int i = 0; i < moves.Count; i++)
        {
            Vector3 position = CalculatePositionFromCoords(moves[i]);
            bool isFree = GetPieceOnSquare(moves[i]) == null;
            squaresData.Add(position, isFree);
        }
        highlightManager.ShowHighlights(squaresData);
    }

    private void DeselectPiece()
    {
        selectedPiece = null;
        highlightManager.ClearSelection();
    }

    private void MoveSelectedPiece(Vector2Int coords, Piece piece)
    {
        TryToTakePiece(coords);
        UpdateBoardOnPieceMove(coords, piece.square, piece, null);
        selectedPiece.MovePiece(coords);
        DeselectPiece();
        gameManager.EndTurn();
    }

    private void TryToTakePiece(Vector2Int coords)
    {
        Piece piece = GetPieceOnSquare(coords);
        if (piece != null && !selectedPiece.IsSameTeam(piece))
            TakePiece(piece);
    }

    private void TakePiece(Piece piece)
    {
        if(piece)
        {
            grid[piece.square.x, piece.square.y] = null;
            gameManager.OnPieceRemoved(piece);
        }
    }

    /// <summary>
    /// Uppdates the board grid with the new piece
    /// </summary>
    public void UpdateBoardOnPieceMove(Vector2Int newCoords, Vector2Int oldCoords, Piece newPiece, Piece oldPiece)
    {
        grid[oldCoords.x, oldCoords.y] = oldPiece;
        grid[newCoords.x, newCoords.y] = newPiece;
    }

    // Calculate the board coordinates from the given world position
    private Vector2Int CalculateCoordsFromPosition(Vector3 inputPosition)
    {
        int x = Mathf.FloorToInt(transform.InverseTransformPoint(inputPosition).z / squareWidth) + boardWidth / 2;
        int y = Mathf.FloorToInt(-transform.InverseTransformPoint(inputPosition).x / squareWidth) + boardWidth / 2;
        return new Vector2Int(x, y);
    }
    /// <summary>
    /// Get the piece on the given square
    /// </summary>
    public Piece GetPieceOnSquare(Vector2Int coords)
    {
        if(CoordsOnBoard(coords))
            return grid[coords.x, coords.y];

        return null;
    }
    /// <summary>
    /// Check if the given coordinates are on the board
    /// </summary>
    public bool CoordsOnBoard(Vector2Int coords)
    {
        if(coords.x < 0 || coords.x >= boardWidth || coords.y < 0 || coords.y >= boardWidth)
            return false;

        return true;
    }

    /// <summary>
    /// Places the given piece on the board at the given coordinates
    /// </summary>
    public void PlacePieceOnBoard(Vector2Int coords, Piece piece)
    {
        if(CoordsOnBoard(coords))
            grid[coords.x, coords.y] = piece;
    }
}
