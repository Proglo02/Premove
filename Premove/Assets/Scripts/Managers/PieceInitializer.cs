using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceInitializer : MonoBehaviour
{
    [SerializeField] private GameObject[] piecePrefabs;
    [SerializeField] private Material blackMaterial;
    [SerializeField] private Material whiteMaterial;

    private Dictionary<string, GameObject> nameToPieceDict = new Dictionary<string, GameObject>();

    private void Awake()
    {
        //Add all piece prefabs to dictionary
        foreach (var piece in piecePrefabs)
        {
            nameToPieceDict.Add(piece.GetComponent<Piece>().GetType().ToString(), piece);
        }
    }

    /// <summary>
    /// Initialize a piece of the given type
    /// </summary>
    public GameObject InitializePiece(Type type)
    {
        //Get the prefab from the dictionary
        GameObject prefab = nameToPieceDict[type.ToString()];

        if(prefab)
        {
            GameObject newPiece = Instantiate(prefab, transform);
            return newPiece;
        }
        return null;
    }

    /// <summary>
    /// Gets the material for the given team color
    /// </summary>
    public Material GetTeamMaterial(TeamColor team)
    {
        return team == TeamColor.Black ? blackMaterial : whiteMaterial;
    }
}
