using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    [SerializeField] private Button readyButton;
    [SerializeField] private TMP_Text movesLeftText;

    private void Awake()
    {
        readyButton.interactable = false;
        SetMovesLeft(GameSettings.Instance.maxMoves);
    }

    public void EnableReadyButton()
    {
        readyButton.interactable = true;
    }

    public void DisableReadyButton()
    {
        readyButton.interactable = false;
    }

    public void OnReady()
    {
        readyButton.interactable = false;
        GameManager.Instance.PlayerReady();
    }

    public void UndoMove()
    {
        GameManager.Instance.UndoMove();
    }

    public void SetMovesLeft(int numMoves)
    {
        movesLeftText.text = "Moves Left: " + numMoves;
    }
}
