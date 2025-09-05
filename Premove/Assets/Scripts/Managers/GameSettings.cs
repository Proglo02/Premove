using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSettings : SingeltonPersistant<GameSettings>
{
    protected override bool persistant => true;

    public bool forceTakeKing = false;
    public bool showAnimations = false;
    public bool showMoves = true;
    public bool doPityTurnOrder = true;
    public bool piecesBlockMoves = false;

    public int maxMoves = 10;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeOnLoad()
    {
        var _ = Instance;
    }
}
