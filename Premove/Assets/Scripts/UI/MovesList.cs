using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovesList : SingeltonPersistant<MovesList>
{
    [SerializeField] MovesListItem movesListItemPrefab;

    public void AddMove(Piece piece)
    {
        MovesListItem listItem = Instantiate(movesListItemPrefab, transform);
        Sprite icon = piece.pieceData.teamColor == TeamColor.White ? piece.pieceUIData.whiteIcon : piece.pieceUIData.blackIcon;
        listItem.SetData(icon, piece.pieceData.square);
    }

    public void RemoveMove()
    {
        if(transform.childCount > 0)
            Destroy(transform.GetChild(transform.childCount - 1).gameObject);
    }
}
