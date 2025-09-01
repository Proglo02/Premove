using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightManager : MonoBehaviour
{
    [SerializeField] private Material freeSquareMaterial;
    [SerializeField] private Material opponentSquareMaterial;

    [SerializeField] private GameObject highlightPrefab;

    private List<GameObject> highlights = new List<GameObject>();

    /// <summary>
    /// Highlights all available moves for a selected piece
    /// </summary>
    public void ShowHighlights(Dictionary<Vector3, bool> squareData)
    {
        ClearSelection();
        foreach(var data in squareData)
        {
            GameObject highlight = Instantiate(highlightPrefab, data.Key, Quaternion.identity);
            highlight.transform.parent = transform;

            highlights.Add(highlight);

            foreach(var setter in highlight.GetComponentsInChildren<MaterialSetter>())
            {
                setter.SetMaterial(data.Value ? freeSquareMaterial : opponentSquareMaterial);
            }
        }
    }

    /// <summary>
    /// Removes all highlights on board
    /// </summary>
    public void ClearSelection()
    {
        foreach(var highlight in highlights)
        {
            Destroy(highlight.gameObject);
        }
    }
}
