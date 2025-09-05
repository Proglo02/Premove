using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MovesListItem : MonoBehaviour
{
    private Image image;
    private TMP_Text moveText;

    public void Awake()
    {
        image = GetComponentInChildren<Image>();
        moveText = GetComponentInChildren<TMP_Text>();
    }

    public void SetData(Sprite icon, Vector2Int coords)
    {
        image.sprite = icon;
        moveText.text = GetTextFromCoords(coords);
    }

    private string GetTextFromCoords(Vector2Int coords)
    {
        char[] letters = new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H' };

        return letters[coords.x].ToString() + (coords.y + 1);
    }
}
