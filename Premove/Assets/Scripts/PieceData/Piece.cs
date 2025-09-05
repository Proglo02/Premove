using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct PieceUIData
{
    public Sprite whiteIcon;
    public Sprite blackIcon;
}

public struct PieceData
{
    public Type type;
    public Vector2Int square;
    public TeamColor teamColor;
    public int value;
    public int id;
    public int numMoves;
    public int promotionMove;
    public bool hasJumped;
}

[RequireComponent(typeof(IObjectTweener), typeof(MaterialSetter))]
public abstract class Piece : MonoBehaviour
{
    [SerializeField] public PieceUIData pieceUIData;

    private MaterialSetter materialSetter;

    public Board board { protected get; set; }

    public PieceData pieceData = new PieceData();

    public List<Vector2Int> availableMoves = new List<Vector2Int>();

    public abstract List<Vector2Int> SelectAvailableSquares(bool ignoreOwnPieces, bool blockOverride = false);

    private void Awake()
    {
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
        return piece.pieceData.teamColor == pieceData.teamColor;
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
    public virtual void MovePiece(Vector2Int coords, bool addMove = true, PieceData takenPiece = new PieceData(), bool doubleMove = false)
    {
        if (addMove)
        {
            board.AddMove(pieceData.square, takenPiece, coords, pieceData.teamColor, pieceData.id, doubleMove);
            pieceData.numMoves++;
        }

        Vector3 targetPosition = board.CalculatePositionFromCoords(coords);
        pieceData.square = coords;
        AnimationManager.Instance.MoveTo(transform, targetPosition);

        if(addMove)
            MovesList.Instance.AddMove(this);
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
    public void SetData(PieceData pieceData, Board board)
    {
        this.pieceData.type = pieceData.type;
        this.pieceData.teamColor = pieceData.teamColor;
        this.pieceData.square = pieceData.square;
        this.board = board;
        this.pieceData.id = pieceData.id;
        this.pieceData.numMoves = pieceData.numMoves;
        this.pieceData.promotionMove = pieceData.promotionMove;
        this.pieceData.hasJumped = pieceData.hasJumped;

        if (this is Pawn)
            GetComponent<Pawn>().hasJumped = pieceData.hasJumped;

        transform.position = board.CalculatePositionFromCoords(pieceData.square);
    }

    /// <summary>
    /// Checks if the piece is attacking another piece of type
    /// </summary>
    public bool IsAttackingPieceOfType<T>() where T : Piece
    {
        foreach(var square in availableMoves)
        {
            Piece piece = board.GetPieceOnSquare(square);

            if (piece is T && piece.pieceData.teamColor != pieceData.teamColor)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Tries to get the piece of a type in the given direction
    /// </summary>
    protected Piece TryGetPieceInDirection<T>(TeamColor teamColor, Vector2Int direction, bool ignoreOtherPieces = true) where T : Piece
    {
        for(int i = 1; i <= Board.BOARD_WIDTH; i++)
        {
            Vector2Int nextCoords = pieceData.square + direction * i;
            Piece piece = board.GetPieceOnSquare(nextCoords);
            if (!board.CoordsOnBoard(nextCoords))
                return null;
            if(piece != null)
            {
                if ((piece.pieceData.teamColor != teamColor || !(piece is T)) && ignoreOtherPieces)
                    return null;

                if (piece.pieceData.teamColor == teamColor && piece is T)
                    return piece;
            }
        }
        return null;
    }

    protected bool TryAddMoveOnBlock(Vector2Int nextCoords, Piece piece, bool blockOverride, out bool stopLooping, bool ignoreOwnPieces)
    {
        stopLooping = false;

        if (GameManager.Instance.gameState != GameState.Active && GameManager.Instance.gameState != GameState.Init)
        {
            if(piece!= null && !IsSameTeam(piece))
                TryAddMove(nextCoords);

            return true;
        }

        if (ignoreOwnPieces || piece == null || !IsSameTeam(piece))
            TryAddMove(nextCoords);

        if (GameSettings.Instance.piecesBlockMoves || blockOverride)
        {
            stopLooping = true;
            return true;
        }

        return false;
    }

    protected bool ShouldPieceBlockMoves()
    {
        if (GameManager.Instance.gameState != GameState.Active && GameManager.Instance.gameState != GameState.Init)
            return true;

        if (GameSettings.Instance.piecesBlockMoves)
            return true;

        return false;
    }

    public virtual void ResetMove()
    {
        pieceData.numMoves--;
    }
}
