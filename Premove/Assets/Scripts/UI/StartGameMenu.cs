using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartGameMenu : MonoBehaviour
{
    [SerializeField] private Toggle takeKingToggle;
    [SerializeField] private Toggle showAnimationsToggle;
    [SerializeField] private Toggle showMovesToggle;
    [SerializeField] private Toggle doPityTurnOrderToggle;
    [SerializeField] private Slider maxMovesSlider;

    private void Start()
    {
        takeKingToggle.isOn         = GameSettings.Instance.forceTakeKing;
        showAnimationsToggle.isOn   = GameSettings.Instance.showAnimations;
        showMovesToggle.isOn        = GameSettings.Instance.showMoves;
        doPityTurnOrderToggle.isOn   = GameSettings.Instance.doPityTurnOrder;
        maxMovesSlider.value        = GameSettings.Instance.maxMoves;
    }

    public void StartGame()
    {
        GameSettings.Instance.forceTakeKing     = takeKingToggle.isOn;
        GameSettings.Instance.showAnimations    = showAnimationsToggle.isOn;
        GameSettings.Instance.showMoves         = showMovesToggle.isOn;
        GameSettings.Instance.doPityTurnOrder    = doPityTurnOrderToggle.isOn;
        GameSettings.Instance.maxMoves          = (int)maxMovesSlider.value;

        SceneManager.LoadScene("GameScene");
    }
}
