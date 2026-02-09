using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    public TetrisManager tetrisManager;
    public TetronimoData[] tetronimos;

    public Piece piecePrefab;
    Piece activePiece;

    public Tilemap tilemap;
    public Vector2Int boardSize;
    public Vector2Int startPosition;

    public float dropInterval = 0.5f;

    float dropTime = 0.0f;

    Dictionary<Vector3Int, Piece> pieces = new Dictionary<Vector3Int, Piece>();

    int startPiece = 0;

    // Create the order in which tetronimos will be called upon
    List<Tetronimo> tetronimoOrder = new List<Tetronimo>
    { Tetronimo.O, Tetronimo.B, Tetronimo.O, Tetronimo.J, Tetronimo.I, Tetronimo.S, Tetronimo.Z, Tetronimo.L, Tetronimo.J, Tetronimo.L, Tetronimo.T, Tetronimo.L, Tetronimo.J, Tetronimo.B, Tetronimo.T};

    int left
    {
        get { return -boardSize.x / 2; }
    }

    int right
    {
        get { return boardSize.x / 2; }
    }

    int bottom
    {
        get { return -boardSize.y / 2; }
    }

    int top
    {
        get { return boardSize.y / 2; }
    }

    //private void Start()
    //{
    //    SpawnPiece();
    //}

    private void Update()
    {
        if (tetrisManager.gameOver) return;


        dropTime += Time.deltaTime;

        if (dropTime >= dropInterval)
        {
            dropTime = 0.0f;

            Clear(activePiece);
            bool moveResult = activePiece.Move(Vector2Int.down);
            Set(activePiece);

            if (!moveResult)
            {
                activePiece.freeze = true;

                CheckBoard();
                SpawnPiece();
            }
        }
    }

    public void SpawnPiece()
    {

        if (startPiece == tetronimoOrder.Count - 1) 
            tetrisManager.SetGameOver(true);

        activePiece = Instantiate(piecePrefab);

        //Tetronimo t = (Tetronimo)Random.Range(0, tetronimos.Length);
        
        // utilize created tetronimo spawn order
        Tetronimo t = tetronimoOrder[startPiece];

        activePiece.Initialize(this, t);

        CheckEndGame();

        Set(activePiece);
        startPiece++;
    }

    void CheckEndGame()
    {
        if (!IsPositionValid(activePiece, activePiece.position) || Time.deltaTime == 10.0f)
        {
            tetrisManager.SetGameOver(true);

        }
    }

    public void UpdateGameOver()
    {
        if (!tetrisManager.gameOver)
        {
            ResetBoard();
        }
    }

    public void ResetBoard()
    {
        Piece[] foundPieces = FindObjectsByType<Piece>(FindObjectsSortMode.None);

        foreach (Piece piece in foundPieces) Destroy(piece.gameObject);

        activePiece = null;

        //tilemap.ClearAllTiles();

        //pieces.Clear();

        SpawnPiece();
    }

    void SetTile(Vector3Int cellPosition, Piece piece)
    {
        if (piece == null)
        {
            tilemap.SetTile(cellPosition, null);

            pieces.Remove(cellPosition);
        }
        else
        {
            tilemap.SetTile(cellPosition, piece.data.tile);

            pieces[cellPosition] = piece;
        }
    }


    public void Set(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int cellPosition = (Vector3Int)(piece.cells[i] + piece.position);
            SetTile(cellPosition, piece);
        }
    }

    public void Clear(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int cellPosition = (Vector3Int)(piece.cells[i] + piece.position);
            SetTile(cellPosition, null);
        }
    }

    public bool IsPositionValid(Piece piece, Vector2Int position)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int cellPosition = (Vector3Int)(piece.cells[i] + position);

            if (cellPosition.x < left || cellPosition.x >= right ||
                cellPosition.y < bottom || cellPosition.y >= top) return false;

            if (tilemap.HasTile(cellPosition)) return false;
        }
        return true;
    }

    bool IsLineFull(int y)
    {
        for (int x = left; x < right; x++)
        {
            Vector3Int cellPosition = new Vector3Int(x, y);
            if (!tilemap.HasTile(cellPosition)) return false;
        }

        return true;
    }

    void DestroyLine(int y)
    {
        //Debug.Log($"Destroy line: {y}");
        for (int X = left; X < right; X++)
        {
            Vector3Int cellPosition = new Vector3Int(X, y);
            if (pieces.ContainsKey(cellPosition))
            {
                Piece piece = pieces[cellPosition];

                piece.ReduceActiveCount();

                SetTile(cellPosition, null);
            }

        }
    }

    void ShiftRowsDown(int clearedRow)
    {
        for (int y = clearedRow + 1; y < top; y++)
        {
            //Debug.Log($"Shift down {clearedRow + 1}");
            for (int x = left; x < right; x++)
            {
                Vector3Int cellPosition = new Vector3Int(x, y);

                if (pieces.ContainsKey(cellPosition))
                {
                    Piece currentPiece = pieces[cellPosition];

                    SetTile(cellPosition, null);

                    cellPosition.y -= 1;
                    SetTile(cellPosition, currentPiece);
                }
            }
        }
    }

    public void CheckBoard()
    {
        List<int> destroyedLines = new List<int>();
        for (int y = bottom; y < top; y++)
        {
            if (IsLineFull(y))
            {
                DestroyLine(y);
                destroyedLines.Add(y);
            }
        }

        //Debug.Log(destroyedLines);
        //We shift down
        int rowsShiftedDown = 0;
        foreach (int y in destroyedLines)
        {
            ShiftRowsDown(y - rowsShiftedDown);
            rowsShiftedDown++;
        }

        int score = tetrisManager.CalculateScore(destroyedLines.Count);

        tetrisManager.ChangeScore(score);


    }

}
