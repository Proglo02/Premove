using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(IObjectTweener), typeof(MaterialSetter))]
public abstract class Piece : MonoBehaviour
{
    private MaterialSetter materialSetter;
    private IObjectTweener tweener;

    public Board board { protected get; set; }

    public Vector2Int square;
    public TeamColor teamColor;
    public int value = 0;
    [HideInInspector] public int id = 0;

    public bool hasMoved { get; private set; } = false;
    public List<Vector2Int> availableMoves = new List<Vector2Int>();

    public abstract List<Vector2Int> SelectAvailableSquares();

    private void Awake()
    {
        tweener = GetComponent<IObjectTweener>();
        materialSetter = GetComponent<MaterialSetter>();
    }

    /// <summary>
    /// Sets the material of the piece
    /// </summary>
    public void SetMaterial(Material material)
    {
        materialSetter.SetMaterial(material);
    }

    /// <summary>
    /// Checks if the given piece is on the same team as this piece
    /// </summary>
    public bool IsSameTeam(Piece piece)
    {
        return piece.teamColor == teamColor;
    }

    /// <summary>
    /// Checks if the piece can move to the given coordinates
    /// </summary>
    public bool CanMoveTo(Vector2Int coords)
    {
        return availableMoves.Contains(coords);
    }

    /// <summary>
    /// Moves the piece to the given coordinates
    /// </summary>
    public virtual void MovePiece(Vector2Int coords, bool addMove = true)
    {
        if(addMove)
            board.AddMove(square, coords, teamColor, id);

        Vector3 targetPosition = board.CalculatePositionFromCoords(coords);
        square = coords;
        hasMoved = true;
        tweener.MoveTo(transform, targetPosition);
    }

    /// <summary>
    /// Tries to add the given coordinates to the available moves list
    /// </summary>
    protected void TryAddMove(Vector2Int coords)
    {
        availableMoves.Add(coords);
    }

    /// <summary>
    /// Set the data of the piece
    /// </summary>
    public void SetData(Vector2Int coords, TeamColor color, Board board, int id)
    {
        teamColor = color;
        square = coords;
        this.board = board;
        this.id = id;

        transform.position = board.CalculatePositionFromCoords(coords);
    }

    /// <summary>
    /// Checks if the piece is attacking another piece of type
    /// </summary>
    public bool IsAttackingPieceOfType<T>() where T : Piece
    {
        foreach(var square in availableMoves)
        {
            if (board.GetPieceOnSquare(square) is T)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Tries to get the piece of a type in the given direction
    /// </summary>
    protected Piece TryGetPieceInDirection<T>(TeamColor teamColor, Vector2Int direction) where T : Piece
    {
        for(int i = 1; i <= Board.BOARD_WIDTH; i++)
        {
            Vector2Int nextCoords = square + direction * i;
            Piece piece = board.GetPieceOnSquare(nextCoords);
            if (!board.CoordsOnBoard(nextCoords))
                return null;
            if(piece != null)
            {
                if (piece.teamColor != teamColor || !(piece is T))
                    return null;
                else if (piece.teamColor == teamColor && piece is T)
                    return piece;
            }
        }
        return null;
    }
}
