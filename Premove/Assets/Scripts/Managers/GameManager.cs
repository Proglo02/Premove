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
    InsuficientMaterial
}
public struct Move
{
    public Vector2Int oldCoords;
    public Vector2Int newCoords;
    public int id;
}

[RequireComponent(typeof(PieceInitializer))]
public class GameManager : MonoBehaviour
{
    [SerializeField] private BoardLayout startingBoardLayout;
    [SerializeField] private Board board;

    private PieceInitializer pieceInitializer;

    private Player whitePlayer;
    private Player blackPlayer;
    private Player activePlayer;

    [HideInInspector] public int moveCount = 0;

    [HideInInspector] public GameState gameState;
    private WinState winState;

    private BoardLayout savedGridState;

    public List<Move> whiteMoves = new List<Move>();
    public List<Move> blackMoves = new List<Move>();

    private void Awake()
    {
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

        board.SetDependencies(this);

        //Create pieces from set layout
        CreatePiecesFromLayout(startingBoardLayout);

        //Set the active player to white and generate their possible moves
        activePlayer = whitePlayer;
        GenerateAllPossiblePlayerMoves(activePlayer);

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
            Vector2Int coords = layout.GetSquareCoordsAtIndex(i);
            TeamColor teamColor = layout.GetPieceTeamColorAtIndex(i);
            string pieceName = layout.GetPieceNameAtIndex(i);
            int id = layout.GetPieceId(i);
            
            Type type = Type.GetType(pieceName);
            InitializePiece(coords, teamColor, type, id);
        }
    }

    /// <summary>
    /// Initializes a piece at the given corrds of the type and color
    /// </summary>
    public void InitializePiece(Vector2Int coords, TeamColor teamColor, Type type, int id)
    {
        //Creatse a new piece of the given type
        Piece newPiece = pieceInitializer.InitializePiece(type).GetComponent<Piece>();
        newPiece.SetData(coords, teamColor, board, id);

        //Set the material of the piece based on its team color
        Material teamMaterial = pieceInitializer.GetTeamMaterial(teamColor);
        newPiece.SetMaterial(teamMaterial);

        board.PlacePieceOnBoard(coords, newPiece);

        Player currentPlayer = teamColor == TeamColor.Black ? blackPlayer : whitePlayer;
        currentPlayer.AddPiece(newPiece);
    }

    private void GenerateAllPossiblePlayerMoves(Player player)
    {
        player.GenerateAllPossibleMoves();
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
        GenerateAllPossiblePlayerMoves(activePlayer);
        GenerateAllPossiblePlayerMoves(GetOtherPlayer(activePlayer));
        ClearEnPassant();
        if (moveCount >= 5)
            ChangeActivePlayer();
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

            GenerateAllPossiblePlayerMoves(activePlayer);
            GenerateAllPossiblePlayerMoves(GetOtherPlayer(activePlayer));

            List<Move> moveList = whiteToPlay ? whiteMoves : blackMoves;

            Vector2Int newCoords;
            Vector2Int oldCoords;

            newCoords = moveList.First().newCoords;
            oldCoords = moveList.First().oldCoords;

            Piece piece = board.GetPieceOnSquare(oldCoords);

            if(piece && piece.id == moveList.First().id && piece.CanMoveTo(newCoords))
                board.MovePiece(newCoords, piece);

            moveList.RemoveAt(0);

            ClearEnPassant();

            whiteToPlay = !whiteToPlay;

            yield return new WaitForSeconds(.5f);
        }

        savedGridState.SetLayoutFromGrid(board.grid);
        GenerateAllPossiblePlayerMoves(activePlayer);
        GenerateAllPossiblePlayerMoves(GetOtherPlayer(activePlayer));
        gameState = GameState.Active;
        if (CheckGameFinished())
            EndGame();
    }

    private void ClearEnPassant()
    {
        foreach(var piece in GetOtherPlayer(activePlayer).GetPiecesOfType<Pawn>())
        {
            Pawn pawn = (Pawn)piece;
            pawn.hasJumped = false;
        }
    }

    private bool CheckGameFinished()
    {
        for (int i = 0; i < 2; i++)
        {
            Player player = (i == 0 ? whitePlayer : blackPlayer);

            Piece[] piecesWithCheck = player.GetPiecesWithCheck();
            Player otherPlayer = GetOtherPlayer(player);
            if (piecesWithCheck.Length > 0)
            {
                Piece king = otherPlayer.GetPiecesOfType<King>().FirstOrDefault();
                otherPlayer.RemoveUnsafeMoves<King>(player, king);

                int availableKingMoves = otherPlayer.GetPiecesOfType<King>().FirstOrDefault().availableMoves.Count();

                if (availableKingMoves == 0)
                {
                    bool canCoverKing = otherPlayer.CanCoverKing(player);
                    if (!canCoverKing)
                    {
                        SetWinState(WinState.Checkmate);
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
                        return true;
                    }
                }

                Piece king = otherPlayer.GetPiecesOfType<King>().FirstOrDefault();
                otherPlayer.RemoveUnsafeMoves<King>(player, king);

                int availableMoves = otherPlayer.GetAllAvailableMoves().Count();

                if (availableMoves == 0)
                {
                    SetWinState(WinState.Stalemate);
                    return true;
                }
            }
        }

        return false;
    }

    private void EndGame()
    {
        Debug.Log(winState.ToString());
        SetGameState(GameState.Finished);
    }

    private void ChangeActivePlayer()
    {
        moveCount = 0;
        SetToPrevoiusBoard();
        activePlayer = activePlayer == whitePlayer ? blackPlayer : whitePlayer;

        if (activePlayer == whitePlayer)
            StartCoroutine(DoGameLoop());
    }

    private void SetToPrevoiusBoard()
    {
        board.ClearBoard();
        CreatePiecesFromLayout(savedGridState);
        GenerateAllPossiblePlayerMoves(activePlayer);
        GenerateAllPossiblePlayerMoves(GetOtherPlayer(activePlayer));
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

    /// <summary>
    /// Removes a taken piece
    /// </summary>
    public void OnPieceRemoved(Piece piece)
    {
        Player pieceOwner = (piece.teamColor == TeamColor.White ? whitePlayer : blackPlayer);
        pieceOwner.score -= piece.value;
        pieceOwner.RemovePiece(piece);
        Destroy(piece.gameObject);
    }
}
