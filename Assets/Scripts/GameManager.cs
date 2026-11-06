using DeveloperTools;
using System;
using Unity.Netcode;
using UnityEngine;
using System.Collections;
public class GameManager : NetworkBehaviour
{
    public enum playerColor
    {
        Red = 1,
        Yellow = 2
    }

    public bool myTurn { get; private set; } = false;
    private playerColor myColor;

    private NetworkManager netManager;
    private GameObject PiecesContainer;

    private GameObject redWon;
    private GameObject yellowWon;

    [SerializeField]
    private GameObject redPiece;
    [SerializeField]
    private GameObject yellowPiece;

    public float cooldown { get; private set; }
    public float lastPlayed { get; private set; }

    public const int lenghtOfBoard = 7;
    public const int heightOfBoard = 6;
    private int[,] BoardArray = new int[lenghtOfBoard, heightOfBoard];
    public bool IsComputerGame { get; private set; }
    public bool IsLocalGame { get; private set; }
    private bool isOnlineGame;
    private bool localModeSelected;
    private int localCurrentColor = 1;
    private float ignoreMovesUntil;
    public bool GameOver { get; private set; }
    public bool IsBusy { get; private set; }
    public int WinnerColor { get; private set; }
    public int MoveCount { get; private set; }
    private AiGameHud computerHud;
    private Transform boardFront;
    // Measured from the 42 transparent apertures in Front Part.png, bottom row first.
    private static readonly Vector2[] HoleCenters = {
        new Vector2(-7.479f,-5.415f),new Vector2(-5.018f,-5.414f),new Vector2(-2.561f,-5.413f),new Vector2(-0.102f,-5.415f),new Vector2(2.407f,-5.418f),new Vector2(4.858f,-5.414f),new Vector2(7.338f,-5.415f),
        new Vector2(-7.458f,-3.308f),new Vector2(-4.999f,-3.307f),new Vector2(-2.540f,-3.308f),new Vector2(-0.081f,-3.308f),new Vector2(2.420f,-3.307f),new Vector2(4.879f,-3.307f),new Vector2(7.359f,-3.304f),
        new Vector2(-7.458f,-1.198f),new Vector2(-4.999f,-1.198f),new Vector2(-2.540f,-1.198f),new Vector2(-0.081f,-1.198f),new Vector2(2.420f,-1.198f),new Vector2(4.879f,-1.198f),new Vector2(7.359f,-1.198f),
        new Vector2(-7.458f,0.909f),new Vector2(-4.999f,0.910f),new Vector2(-2.540f,0.910f),new Vector2(-0.081f,0.910f),new Vector2(2.420f,0.910f),new Vector2(4.879f,0.910f),new Vector2(7.359f,0.910f),
        new Vector2(-7.458f,3.019f),new Vector2(-4.999f,3.019f),new Vector2(-2.540f,3.019f),new Vector2(-0.081f,3.019f),new Vector2(2.420f,3.019f),new Vector2(4.879f,3.019f),new Vector2(7.355f,3.024f),
        new Vector2(-7.460f,5.124f),new Vector2(-4.999f,5.127f),new Vector2(-2.540f,5.127f),new Vector2(-0.081f,5.127f),new Vector2(2.420f,5.127f),new Vector2(4.878f,5.130f),new Vector2(7.358f,5.127f)
    };
    public Vector3 GetCellPosition(int column, int row)
    {
        Vector2 center = HoleCenters[row * lenghtOfBoard + column];
        return boardFront.TransformPoint(new Vector3(center.x, center.y, 2.1f));
    }
    public int GetCell(int column, int row) { return BoardArray[column, row]; }
    /*
    {
        {0, 0, 0, 0, 0, 0 },
        {0, 0, 0, 0, 0, 0 },  Turn 90 Degrees counter clockwise
        {0, 0, 0, 0, 0, 0 },  This is 7X6 Board
        {0, 0, 0, 0, 0, 0 },
        {0, 0, 0, 0, 0, 0 },
        {0, 0, 0, 0, 0, 0 },
        {0, 0, 0, 0, 0, 0 }
    };
    */
    
    private void Awake()
    {
        var canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        redWon = canvas.transform.Find("RedWon").gameObject;
        yellowWon = canvas.transform.Find("YellowWon").gameObject;
        PiecesContainer = DevTools.FindGameObject("Board/Pieces");
        boardFront = GameObject.Find("Board/Front Part").transform;
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
        gameObject.AddComponent<BoardInput>().Initialize(this);
#if UNITY_WEBGL && !UNITY_EDITOR
        UnityEngine.WebGLInput.mobileKeyboardSupport = false;
#endif
        redWon.SetActive(false);
        yellowWon.SetActive(false);

        netManager = Unity.Netcode.NetworkManager.Singleton;
        isOnlineGame = netManager != null && netManager.IsListening;
        if (!isOnlineGame)
        {
            myColor = playerColor.Red;
            computerHud = gameObject.AddComponent<AiGameHud>();
            computerHud.Initialize(this, canvas);
            computerHud.ShowModeSelection();
            IsLocalGame = true;
            return;
        }
        if (netManager.IsHost)
        {
            myColor = playerColor.Red;
            myTurn = true;
            Debug.Log("Hosting");
        }
        else
        {
            myColor = playerColor.Yellow;
            myTurn = false;
        }
    }
    public bool TryPlayColumn(int column)
    {
        if (!myTurn || Time.unscaledTime < ignoreMovesUntil || Time.time - lastPlayed < cooldown) return false;
        if (isOnlineGame)
        {
            if (column < 0 || column >= lenghtOfBoard) return false;
            registerPieceServerRpc(column);
            return true;
        }
        if (!localModeSelected || GameOver || IsBusy || ConnectFourRules.AvailableRow(BoardArray, column) < 0) return false;
        IsBusy = true;
        myTurn = false;
        if (IsComputerGame) StartCoroutine(PlayComputerTurn(column));
        else StartCoroutine(PlayLocalTwoPlayerTurn(column));
        return true;
    }

    public void StartLocalGame(bool againstComputer)
    {
        if (isOnlineGame) return;
        localModeSelected = true;
        IsLocalGame = true;
        IsComputerGame = againstComputer;
        localCurrentColor = 1;
        ignoreMovesUntil = Time.unscaledTime + 0.25f;
        computerHud.HideModeSelection();
        RestartGame();
    }

    public void RestartGame()
    {
        if (isOnlineGame || computerHud == null || !localModeSelected) return;
        StopAllCoroutines();
        foreach (Transform child in PiecesContainer.transform)
        {
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }
        Array.Clear(BoardArray, 0, BoardArray.Length);
        redWon.SetActive(false); yellowWon.SetActive(false);
        GameOver = false; IsBusy = false; WinnerColor = 0; MoveCount = 0;
        localCurrentColor = 1;
        myTurn = true; cooldown = 0; lastPlayed = -10;
        computerHud.SetStatus(IsComputerGame ? "Your turn • Red — tap or click a column" : "Player 1 (Red) — choose a column");
    }

    private IEnumerator PlayComputerTurn(int column)
    {
        yield return DropLocalPiece(column, 1);
        if (FinishLocalGame()) yield break;
        computerHud.SetStatus("Computer is thinking • Yellow");
        yield return new WaitForSeconds(0.3f);
        int computerColumn = ConnectFourRules.ChooseComputerMove(BoardArray);
        if (computerColumn >= 0) yield return DropLocalPiece(computerColumn, 2);
        if (FinishLocalGame()) yield break;
        IsBusy = false; myTurn = true;
        computerHud.SetStatus("Your turn • Red — tap or click a column");
    }

    private IEnumerator PlayLocalTwoPlayerTurn(int column)
    {
        int color = localCurrentColor;
        yield return DropLocalPiece(column, color);
        if (FinishLocalGame()) yield break;
        localCurrentColor = color == 1 ? 2 : 1;
        IsBusy = false;
        myTurn = true;
        computerHud.SetStatus(localCurrentColor == 1 ? "Player 1 (Red) — choose a column" : "Player 2 (Yellow) — choose a column");
    }
    private IEnumerator DropLocalPiece(int column, int color)
    {
        int row = ConnectFourRules.AvailableRow(BoardArray, column);
        if (row < 0) yield break;
        BoardArray[column, row] = color;
        MoveCount++;
        Vector3 landingPosition = GetCellPosition(column, row);
        // Place chips behind the perforated front, in front of the stand.
        var piece = Instantiate(color == 1 ? redPiece : yellowPiece,
            new Vector3(landingPosition.x, 8.5f, landingPosition.z), Quaternion.identity, PiecesContainer.transform);
        // Local chips use an exact landing position; physics cannot desync the grid.
        var body = piece.GetComponent<Rigidbody2D>();
        if (body != null) body.simulated = false;
        foreach (var collider in piece.GetComponents<Collider2D>()) collider.enabled = false;
        Vector3 start = piece.transform.position, end = landingPosition;
        const float duration = 0.42f;
        for (float elapsed = 0; elapsed < duration; elapsed += Time.deltaTime)
        {
            float t = Mathf.Clamp01(elapsed / duration);
            piece.transform.position = Vector3.Lerp(start, end, t * t);
            yield return null;
        }
        piece.transform.position = end;
    }

    private bool FinishLocalGame()
    {
        WinnerColor = ConnectFourRules.Winner(BoardArray);
        GameOver = WinnerColor != 0 || ConnectFourRules.IsFull(BoardArray);
        if (!GameOver) return false;
        myTurn = false; IsBusy = false;
        computerHud.SetStatus(WinnerColor == 1 ? "Red wins! Four in a row!" :
            WinnerColor == 2 ? (IsComputerGame ? "Computer wins! Play again?" : "Yellow wins! Four in a row!") : "Draw! The board is full.");
        return true;
    }
    public void resetCooldown()
    {
        lastPlayed = Time.time;
    }

    void switchTurn() {
        if (myTurn) { myTurn = false; }
        else { myTurn = true; }
        resetCooldown();
    }
        

    [ServerRpc]
    public void testServerRpc()
    {
        testServerClientRpc();
    }
    [ClientRpc]
    public void testServerClientRpc() 
    {
        Debug.Log("private works");
    }

    
    private void redWins() { redWon.SetActive(true); }
    private void yellowWins() { yellowWon.SetActive(true); }


    [ServerRpc(RequireOwnership = false)]
    public void registerPieceServerRpc(int column)
    {
        for (int row = 0; row < heightOfBoard; row++)
        {

            if (BoardArray[column, row] == 0)
            {
                if (myTurn)
                {
                    
                    BoardArray[column, row] = (int)myColor;
                    spawnPiece(myColor, (float)(column - 3) * 2.5f, 8.5f);
                    registerPieceClientRpc();
                    WinCheck(myColor);

                }
                else
                {
                    BoardArray[column, row] = (int)myColor+1;
                    spawnPiece(myColor+1, (float)(column - 3) * 2.5f, 8.5f);
                    registerPieceClientRpc();
                    WinCheck(myColor+1);
                }
                break;
            }
        }
        
    }
    [ClientRpc]
    public void registerPieceClientRpc()
    {
        
        resetCooldown();
        switchTurn();
    }

    [ServerRpc]
    private void winServerRpc(playerColor color)
    {
        winClientRpc(color);
    }
    [ClientRpc]
    private void winClientRpc(playerColor color)
    {
        if (color == playerColor.Red)
        {
            redWins();
        }
        else
        {
            yellowWins();
        }
    }
    private void WinCheck(playerColor player_color)
    {
        

        //horizontal
        for (int y = 0; y < heightOfBoard; y++)
        {
            for (int x = 0; x < lenghtOfBoard - 3; x++)
            {
                
                if (BoardArray[x, y] == (int)player_color && BoardArray[x + 1, y] == (int)player_color && BoardArray[x + 2, y] == (int)player_color && BoardArray[x + 3, y] == (int)player_color)
                {
                    winServerRpc(player_color); 
                }
            }
        }

        //vertical
        for (int x = 0; x < lenghtOfBoard; x++)
        {
            for (int y = 0; y < heightOfBoard - 3; y++)
            {
                if (BoardArray[x, y] == (int)player_color && BoardArray[x, y + 1] == (int)player_color && BoardArray[x, y + 2] == (int)player_color && BoardArray[x, y + 3] == (int)player_color)
                {
                    winServerRpc(player_color);
                }
            }
        }

        //y=x
        for (int y = 0; y < heightOfBoard - 3; y++)
        {
            for (int x = 0; x < lenghtOfBoard - 3; x++)
            {
                if (BoardArray[x, y] == (int)player_color && BoardArray[x + 1, y + 1] == (int)player_color && BoardArray[x + 2, y + 2] == (int)player_color && BoardArray[x + 3, y + 3] == (int)player_color)
                {
                    winServerRpc(player_color);
                }
            }
        }

        //y=-x
        for (int y = 0; y < heightOfBoard - 3; y++)
        {
            for (int x = 0; x < lenghtOfBoard - 3; x++)
            {
                if (BoardArray[x, y + 3] == (int)player_color && BoardArray[x + 1, y + 2] == (int)player_color && BoardArray[x + 2, y + 1] == (int)player_color && BoardArray[x + 3, y] == (int)player_color)
                {
                    winServerRpc(player_color);
                }
            }
        }
    }

    
    private void spawnPiece(playerColor color, float x, float y)
    {
        GameObject piece;
        if(color == playerColor.Yellow)
        {
            piece = Instantiate(yellowPiece, new Vector3(x,y,0), Quaternion.identity, PiecesContainer.transform);
        }
        else
        {
            piece = Instantiate(redPiece, new Vector3(x, y, 0), Quaternion.identity, PiecesContainer.transform);

        }
        piece.GetComponent<NetworkObject>().Spawn();
    }
   
}




