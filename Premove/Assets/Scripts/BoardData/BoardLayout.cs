using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BoardLayout", menuName = "ScriptableObjects/Board/Layout", order = 1)]
public class BoardLayout : ScriptableObject
{
    [Serializable]
    private class BoardSetupSquare
    {
        public Vector2Int position;
        public PieceType pieceType;
        public TeamColor teamColor;
    }

    [SerializeField] private BoardSetupSquare[] squares;

    /// <summary>
    /// Returns the total number of pieces defined in the layout
    /// </summary>
    public int GetPiecesCount()
    {
        return squares.Length;
    }

    /// <summary>
    ///Returns the board coordinates of the piece at the given index
    /// </summary>
    public Vector2Int GetSquareCoordsAtIndex(int index)
    {
        if(squares.Length <= index)
        {
            throw new IndexOutOfRangeException("Index out of range in BoardLayout.GetSquareCoordsAtIndex");
        }

        return new Vector2Int(squares[index].position.x - 1, squares[index].position.y - 1);
    }

    /// <summary>
    ///Returns the piece type name at the given index
    /// </summary>
    public string GetPieceNameAtIndex(int index)
    {
        if (squares.Length <= index)
        {
            throw new IndexOutOfRangeException("Index out of range in BoardLayout.GetPieceNameAtIndex");
        }

        return squares[index].pieceType.ToString();
    }

    /// <summary>
    ///Returns the team color at the given index
    /// </summary>
    public TeamColor GetPieceTeamColorAtIndex(int index)
    {
        if (squares.Length <= index)
        {
            throw new IndexOutOfRangeException("Index out of range in BoardLayout.GetPieceTeamColorAtIndex");
        }

        return squares[index].teamColor;
    }
}
