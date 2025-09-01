using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum GameState
{
    Init,
    Active,
    Finished
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

    private GameState state;

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

        board.SetDependencies(this);

        //Create pieces from set layout
        CreatePiecesFromLayout(startingBoardLayout);

        //Set the active player to white and generate their possible moves
        activePlayer = whitePlayer;
        GenerateAllPossiblePlayerMoves(activePlayer);

        SetGameState(GameState.Active);
    }

    //Sets the current state of the game
    private void SetGameState(GameState state)
    {
        this.state = state;
    }

    /// <summary>
    /// Checks if the current game is active
    /// </summary>
    public bool IsGameInProgress()
    {
        return state == GameState.Active;
    }

    private void CreatePiecesFromLayout(BoardLayout layout)
    {
        //Initialize all pieces defined in the layout
        for (int i = 0; i < layout.GetPiecesCount(); i++)
        {
            Vector2Int coords = layout.GetSquareCoordsAtIndex(i);
            TeamColor teamColor = layout.GetPieceTeamColorAtIndex(i);
            string pieceName = layout.GetPieceNameAtIndex(i);
            
            Type type = Type.GetType(pieceName);
            InitializePiece(coords, teamColor, type);
        }
    }

    private void InitializePiece(Vector2Int coords, TeamColor teamColor, Type type)
    {
        //Creatse a new piece of the given type
        Piece newPiece = pieceInitializer.InitializePiece(type).GetComponent<Piece>();
        newPiece.SetData(coords, teamColor, board);

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
        GenerateAllPossiblePlayerMoves(activePlayer);
        GenerateAllPossiblePlayerMoves(GetOtherPlayer(activePlayer));
        if (GameFinished())
            EndGame();
        else
            ChangeActivePlayer();
    }

    private bool GameFinished()
    {
        Piece[] piecesWithCheck = activePlayer.GetPiecesWithCheck();
        if(piecesWithCheck.Length > 0)
        {
            Player oppositePlayer = GetOtherPlayer(activePlayer);
            Piece king = oppositePlayer.GetPiecesOfType<King>().FirstOrDefault();
            oppositePlayer.RemoveUnsafeMoves<King>(activePlayer, king);

            int availableKingMoves = oppositePlayer.GetPiecesOfType<King>().FirstOrDefault().availableMoves.Count();

            if(availableKingMoves == 0)
            {
                bool canCoverKing = oppositePlayer.CanCoverKing(activePlayer);
                if(!canCoverKing)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void EndGame()
    {
        Debug.Log("Check Mate");
        SetGameState(GameState.Finished);
    }

    private void ChangeActivePlayer()
    {
        activePlayer = activePlayer == whitePlayer ? blackPlayer : whitePlayer;
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
        pieceOwner.RemovePiece(piece);
        Destroy(piece.gameObject);
    }
}
