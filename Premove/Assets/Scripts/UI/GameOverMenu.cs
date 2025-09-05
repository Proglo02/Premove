using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverMenu : MonoBehaviour
{
    [SerializeField] private TMP_Text winText;

    public void SetWinText(TeamColor teamColor)
    {
        winText.text = (teamColor == TeamColor.White ? "White" : "Black") + "Wins!";
    }

    public void PlayAgain()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void QuitToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
