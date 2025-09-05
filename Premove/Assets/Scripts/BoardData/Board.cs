using OpenCover.Framework.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(HighlightManager))]
public class Board : MonoBehaviour
{
    [SerializeField] private Transform bottomLeftsquare;
    [SerializeField] private float squareWidth;

    private HighlightManager highlightManager;

    [HideInInspector] public Piece[,] grid;
    private Piece selectedPiece;

    public const int BOARD_WIDTH = 8;

    private void Awake()
    {
        highlightManager = GetComponent<HighlightManager>();
        CreateGrid();
    }

    private void CreateGrid()
    {
        grid = new Piece[BOARD_WIDTH, BOARD_WIDTH];
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
        for (int i = 0; i < BOARD_WIDTH; i++)
        {
            for (int j = 0; j < BOARD_WIDTH; j++)
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
        if (!GameManager.Instance.IsGameInProgress() || !GameManager.Instance.canInteract)
            return;

        Vector2Int coords = CalculateCoordsFromPosition(inputPosition);
        Piece piece = GetPieceOnSquare(coords);
        if(selectedPiece)
        {
            if (piece && selectedPiece == piece)
                DeselectPiece();
            else if (piece && selectedPiece != piece && !CanTakePiece(coords, selectedPiece) && selectedPiece.IsSameTeam(piece))
                SelectPiece(piece);
            else if (selectedPiece.CanMoveTo(coords))
                MoveSelectedPiece(coords, selectedPiece);
            else if (piece == null)
                DeselectPiece();
        }
        else
        {
            if(piece != null && GameManager.Instance.IsTeamTurnActive(piece.pieceData.teamColor))
                SelectPiece(piece);
        }
    }

    private void SelectPiece(Piece piece)
    {
        if (!GameSettings.Instance.forceTakeKing)
        {
            GameManager.Instance.RemoveUnsafeMoves<King>(piece);

            if(piece is King)
                GameManager.Instance.RemoveUnsafeCastleMoves();
        }
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
        Piece pieceToTake = GetPieceOnSquare(coords);

        PieceData pieceData = new PieceData();
        if (pieceToTake)
        {
            pieceData = pieceToTake.pieceData;
        }

        TryToTakePiece(coords, selectedPiece);
        UpdateBoardOnPieceMove(coords, piece.pieceData.square, piece, null);
        selectedPiece.MovePiece(coords, true, pieceData);
        DeselectPiece();
        GameManager.Instance.EndTurn();
    }

    public void MovePiece(Vector2Int coords, Piece piece, bool takePieceOverride = false)
    {
        if (TryToTakePiece(coords, piece) || takePieceOverride)
        {
            UpdateBoardOnPieceMove(coords, piece.pieceData.square, piece, null);
            piece.MovePiece(coords, false);
        }
    }

    private bool CanTakePiece(Vector2Int coords, Piece movedPiece)
    {
        Piece piece = GetPieceOnSquare(coords);

        if (!movedPiece.CanMoveTo(coords))
            return false;

        if (piece == null)
            return true;

        if (!movedPiece.IsSameTeam(piece) || GameManager.Instance.gameState == GameState.Active)
        {
            return true;
        }

        return false;
    }

    private bool TryToTakePiece(Vector2Int coords, Piece movedPiece)
    {
        Piece piece = GetPieceOnSquare(coords);

        if (CanTakePiece(coords, movedPiece))
        {
            TakePiece(piece);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Takes the given piece
    /// </summary>
    public void TakePiece(Piece piece)
    {
        if(piece)
        {
            grid[piece.pieceData.square.x, piece.pieceData.square.y] = null;
            GameManager.Instance.OnPieceRemoved(piece);
        }
    }

    public void PromotePiece(Piece piece)
    {
        TakePiece(piece);

        PieceData pieceData = new PieceData();

        pieceData = piece.pieceData;
        pieceData.type = typeof(Queen);
        pieceData.promotionMove = piece.pieceData.numMoves;

        GameManager.Instance.InitializePiece(pieceData);
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
        int x = Mathf.FloorToInt(transform.InverseTransformPoint(inputPosition).z / squareWidth) + BOARD_WIDTH / 2;
        int y = Mathf.FloorToInt(-transform.InverseTransformPoint(inputPosition).x / squareWidth) + BOARD_WIDTH / 2;
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
        if(coords.x < 0 || coords.x >= BOARD_WIDTH || coords.y < 0 || coords.y >= BOARD_WIDTH)
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

    /// <summary>
    /// Clears the board
    /// </summary>
    public void ClearBoard()
    {
        foreach(var piece in grid)
        {
            if(piece)
            {
                TakePiece(piece);
            }
        }

        CreateGrid();
    }

    /// <summary>
    /// Adds a new move to the player list
    /// </summary>
    public void AddMove(Vector2Int oldCoords, PieceData takenPiece, Vector2Int newCoords, TeamColor teamColor, int id, bool doubleMove)
    {
        Move move = new Move();
        move.oldCoords = oldCoords;
        move.takenPiece = takenPiece;
        move.newCoords = newCoords;
        move.id = id;
        move.doubleMove = doubleMove;

        if (teamColor == TeamColor.White)
            GameManager.Instance.whiteMoves.Add(move);
        else
            GameManager.Instance.blackMoves.Add(move);
    }
}
