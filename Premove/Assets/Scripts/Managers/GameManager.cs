using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum GameState
{
    Init,
    Active,
    Looping,
    Finished
}

public enum WinState
{
    None,
    Checkmate,
    Stalemate,
    InsuficientMaterial,
    KingTaken
}
public struct Move
{
    public Vector2Int oldCoords;
    public PieceData takenPiece;
    public Vector2Int newCoords;
    public int id;
    public bool doubleMove;
}

[RequireComponent(typeof(PieceInitializer))]
public class GameManager : SingeltonPersistant<GameManager>
{
    [SerializeField] private BoardLayout startingBoardLayout;
    [SerializeField] private Board board;

    private PieceInitializer pieceInitializer;

    private Player whitePlayer;
    private Player blackPlayer;
    private Player activePlayer;

    private int moveCount = 0;
    public bool canInteract { get; private set; } = true;

    public GameState gameState { get; private set; }
    private WinState winState;

    private BoardLayout savedGridState;

    public List<Move> whiteMoves = new List<Move>();
    public List<Move> blackMoves = new List<Move>();

    private float roundDelay = .1f;

    [Header("UI")]
    [SerializeField] private GameOverMenu gameOverMenu;
    [SerializeField] private HUD hud;

    protected override void Awake()
    {
        base.Awake();
        GetComponents();
        CreatePlayers();
    }

    private void Start()
    {
        StartNewGame();
    }

    private void GetComponents()
    {
        pieceInitializer = GetComponent<PieceInitializer>();
    }

    private void CreatePlayers()
    {
        blackPlayer = new Player(TeamColor.Black, board);
        whitePlayer = new Player(TeamColor.White, board);
    }

    private void StartNewGame()
    {
        SetGameState(GameState.Init);
        SetWinState(WinState.None);

        //Create pieces from set layout
        CreatePiecesFromLayout(startingBoardLayout);

        //Set the active player to white and generate their possible moves
        activePlayer = whitePlayer;
        GenerateAllPossiblePlayerMoves(activePlayer, true);

        savedGridState = ScriptableObject.CreateInstance<BoardLayout>();
        savedGridState.SetLayoutFromGrid(board.grid);

        SetGameState(GameState.Active);
    }

    //Sets the current state of the game
    private void SetGameState(GameState state)
    {
        gameState = state;
    }

    private void SetWinState(WinState state)
    {
        winState = state;
    }

    /// <summary>
    /// Checks if the current game is active
    /// </summary>
    public bool IsGameInProgress()
    {
        return gameState == GameState.Active;
    }

    private void CreatePiecesFromLayout(BoardLayout layout)
    {
        //Initialize all pieces defined in the layout
        for (int i = 0; i < layout.GetPiecesCount(); i++)
        {
            PieceData pieceData = new PieceData();

            string pieceName = layout.GetPieceNameAtIndex(i);
            Type type = Type.GetType(pieceName);

            pieceData.type = type;
            pieceData.square = layout.GetSquareCoordsAtIndex(i);
            pieceData.teamColor = layout.GetPieceTeamColorAtIndex(i);
            pieceData.id = layout.GetPieceId(i);
            pieceData.numMoves = layout.GetNumMoves(i);
            pieceData.promotionMove = layout.GetPromotionMove(i);
            pieceData.hasJumped = layout.HaveJumped(i);
            
            InitializePiece(pieceData);
        }
    }

    /// <summary>
    /// Initializes a piece at the given corrds of the type and color
    /// </summary>
    public Piece InitializePiece(PieceData pieceData)
    {
        //Creatse a new piece of the given type
        Piece newPiece = pieceInitializer.InitializePiece(pieceData.type).GetComponent<Piece>();
        newPiece.SetData(pieceData, board);

        //Set the material of the piece based on its team color
        Material teamMaterial = pieceInitializer.GetTeamMaterial(pieceData.teamColor);
        newPiece.SetMaterial(teamMaterial);

        board.PlacePieceOnBoard(pieceData.square, newPiece);

        Player currentPlayer = pieceData.teamColor == TeamColor.Black ? blackPlayer : whitePlayer;
        currentPlayer.AddPiece(newPiece);

        return newPiece;
    }

    private void GenerateAllPossiblePlayerMoves(Player player, bool ignoreOwnPieces, bool blockOverride = false)
    {
        player.GenerateAllPossibleMoves(ignoreOwnPieces, blockOverride);
    }

    /// <summary>
    /// Checks if it is the given team's turn
    /// </summary>
    internal bool IsTeamTurnActive(TeamColor teamColor)
    {
        return activePlayer.teamColor == teamColor;
    }

    /// <summary>
    /// Ends the current player's turn and switches to the other player
    /// </summary>
    public void EndTurn()
    {
        moveCount++;
        GenerateAllPossiblePlayerMoves(activePlayer, true);
        GenerateAllPossiblePlayerMoves(GetOtherPlayer(activePlayer), true);
        ClearEnPassant(activePlayer);

        hud.SetMovesLeft(GameSettings.Instance.maxMoves - moveCount);

        if (moveCount >= GameSettings.Instance.maxMoves)
        {
            canInteract = false;
            hud.EnableReadyButton();
        }
    }

    private IEnumerator DoGameLoop()
    {
        gameState = GameState.Looping;
        bool whiteToPlay = true;

        while (whiteMoves.Count > 0 || blackMoves.Count > 0)
        {
            if (whiteToPlay && whiteMoves.Count < 1)
                whiteToPlay = false;
            else if (blackMoves.Count < 1)
                whiteToPlay = true;

            GenerateAllPossiblePlayerMoves(activePlayer, true);
            GenerateAllPossiblePlayerMoves(GetOtherPlayer(activePlayer), true);

            List<Move> moveList = whiteToPlay ? whiteMoves : blackMoves;

            Vector2Int newCoords;
            Vector2Int oldCoords;

            newCoords = moveList.First().newCoords;
            oldCoords = moveList.First().oldCoords;

            Piece piece = board.GetPieceOnSquare(oldCoords);

            if(piece && piece.pieceData.id == moveList.First().id && piece.CanMoveTo(newCoords))
                board.MovePiece(newCoords, piece);

            moveList.RemoveAt(0);

            ClearEnPassant(whiteToPlay ? whitePlayer : blackPlayer);

            whiteToPlay = !whiteToPlay;

            yield return new WaitForSeconds(roundDelay);
        }

        savedGridState.SetLayoutFromGrid(board.grid);
        GenerateAllPossiblePlayerMoves(activePlayer, false, true);
        GenerateAllPossiblePlayerMoves(GetOtherPlayer(activePlayer), false, true);
        if (CheckGameFinished(out Player winPlayer))
            EndGame(winPlayer.teamColor);
        gameState = GameState.Active;
        GenerateAllPossiblePlayerMoves(GetOtherPlayer(activePlayer), true);
        GenerateAllPossiblePlayerMoves(activePlayer, true);
    }

    private void ClearEnPassant(Player player)
    {
        foreach(var piece in GetOtherPlayer(player).GetPiecesOfType<Pawn>())
        {
            Pawn pawn = (Pawn)piece;
            pawn.hasJumped = false;
        }
    }

    private bool CheckGameFinished(out Player winPlayer)
    {
        if (GameSettings.Instance.forceTakeKing)
            return CheckIfPlayersHaveKing(out winPlayer);
        else
            return RegularChessRuleCheck(out winPlayer);
    }

    private bool CheckIfPlayersHaveKing(out Player winPlayer)
    {
        winPlayer = null;

        for (int i = 0; i < 2; i++)
        {
            Player player = (i == 0 ? whitePlayer : blackPlayer);

            if (player.GetPiecesOfType<King>().FirstOrDefault() == null)
            {
                winState = WinState.KingTaken;
                winPlayer = player;
                return true;
            }
        }

        return false;
    }

    private bool RegularChessRuleCheck(out Player winPlayer)
    {
        for (int i = 0; i < 2; i++)
        {
            Player player = (i == 0 ? whitePlayer : blackPlayer);

            Piece[] piecesWithCheck = player.GetPiecesWithCheck();
            Player otherPlayer = GetOtherPlayer(player);
            if (piecesWithCheck.Length > 0)
            {
                if (!GameSettings.Instance.forceTakeKing)
                {
                    Piece king = otherPlayer.GetPiecesOfType<King>().FirstOrDefault();
                    otherPlayer.RemoveUnsafeMoves<King>(player, king, false);
                    otherPlayer.RemoveunsafeCastleMoves(player, king);
                }

                int availableKingMoves = otherPlayer.GetPiecesOfType<King>().FirstOrDefault().availableMoves.Count();

                if (availableKingMoves == 0)
                {
                    bool canCoverKing = otherPlayer.CanCoverKing(player);
                    if (!canCoverKing)
                    {
                        SetWinState(WinState.Checkmate);
                        winPlayer = player;
                        return true;
                    }
                }
            }
            else
            {
                int numActivePawns = player.GetPiecesOfType<Pawn>().Count();
                int numOtherPawns = otherPlayer.GetPiecesOfType<Pawn>().Count();
                if (numActivePawns == 0 && numOtherPawns == 0)
                {
                    if (otherPlayer.score <= 3 && player.score <= 3)
                    {
                        SetWinState(WinState.InsuficientMaterial);
                        winPlayer = player;
                        return true;
                    }
                }

                if (!GameSettings.Instance.forceTakeKing)
                {
                    Piece king = otherPlayer.GetPiecesOfType<King>().FirstOrDefault();
                    otherPlayer.RemoveUnsafeMoves<King>(player, king, false);
                    otherPlayer.RemoveunsafeCastleMoves(player, king);
                }

                int availableMoves = otherPlayer.GetAllAvailableMoves().Count();

                if (availableMoves == 0)
                {
                    SetWinState(WinState.Stalemate);
                    winPlayer = player;
                    return true;
                }
            }
        }
        winPlayer = null;
        return false;
    }

    private void EndGame(TeamColor teamColor)
    {
        Debug.Log(winState.ToString());
        SetGameState(GameState.Finished);
        gameOverMenu.gameObject.SetActive(true);
        gameOverMenu.SetWinText(teamColor);
    }

    private void ChangeActivePlayer()
    {
        canInteract = true;
        moveCount = 0;
        SetToPrevoiusBoard();
        activePlayer = activePlayer == whitePlayer ? blackPlayer : whitePlayer;

        if (activePlayer == whitePlayer)
            StartCoroutine(DoGameLoop());

        hud.SetMovesLeft(GameSettings.Instance.maxMoves);
    }

    public void PlayerReady()
    {
        ChangeActivePlayer();
    }

    private void SetToPrevoiusBoard()
    {
        board.ClearBoard();
        CreatePiecesFromLayout(savedGridState);
        GenerateAllPossiblePlayerMoves(activePlayer, true);
        GenerateAllPossiblePlayerMoves(GetOtherPlayer(activePlayer), true);
    }

    public void UndoMove()
    {
        RemoveMove();

        GenerateAllPossiblePlayerMoves(activePlayer, true);
        GenerateAllPossiblePlayerMoves(GetOtherPlayer(activePlayer), true);

        hud.DisableReadyButton();

        moveCount--;
        moveCount = Mathf.Max(0, moveCount);
        canInteract = true;

        hud.SetMovesLeft(GameSettings.Instance.maxMoves - moveCount);
    }

    private void RemoveMove()
    {
        List<Move> moves = activePlayer.teamColor == TeamColor.White ? whiteMoves : blackMoves;

        if (moves.Count <= 0)
            return;

        Move move = moves.Last();
        Piece piece = board.GetPieceOnSquare(move.newCoords);

        if(piece && piece.pieceData.promotionMove == piece.pieceData.numMoves)
        {
            PieceData pieceData = new PieceData();

            pieceData = piece.pieceData;
            pieceData.type = typeof(Pawn);
            pieceData.promotionMove = -1;

            board.TakePiece(piece);
            piece = InitializePiece(pieceData);
        }

        board.MovePiece(move.oldCoords, piece, true);
        MovesList.Instance.RemoveMove();

        piece.ResetMove();

        if (move.takenPiece.type != null)
            InitializePiece(move.takenPiece);

        bool doubleMove = move.doubleMove;

        moves.Remove(moves.Last());

        if (doubleMove)
            RemoveMove();
    }

    private Player GetOtherPlayer(Player player)
    {
       return player == whitePlayer ? blackPlayer : whitePlayer;
    }

    /// <summary>
    /// Removes all moves that would enable an attack on the king
    /// </summary>
    public void RemoveUnsafeMoves<T>(Piece piece) where T : Piece
    {
        activePlayer.RemoveUnsafeMoves<T>(GetOtherPlayer(activePlayer), piece);
    }

    public void RemoveUnsafeCastleMoves()
    {
        activePlayer.RemoveunsafeCastleMoves(GetOtherPlayer(activePlayer), activePlayer.GetPiecesOfType<King>().FirstOrDefault());
    }

    /// <summary>
    /// Removes a taken piece
    /// </summary>
    public void OnPieceRemoved(Piece piece)
    {
        Player pieceOwner = (piece.pieceData.teamColor == TeamColor.White ? whitePlayer : blackPlayer);
        pieceOwner.score -= piece.pieceData.value;
        pieceOwner.RemovePiece(piece);
        Destroy(piece.gameObject);
    }

    public bool SquareAttacked(Vector2Int square, TeamColor teamColor)
    {
        Player oppositePlayer = teamColor == TeamColor.White ? blackPlayer : whitePlayer;

        return oppositePlayer.CanAttackSquare(square);
    }
}
