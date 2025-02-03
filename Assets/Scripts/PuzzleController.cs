using System.CodeDom.Compiler;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleController : MonoBehaviour
{
    public static PuzzleController Instance { get; private set; }
    public GameObject puzzlePiecePrefab;

    private RectTransform rect;
    private GridLayoutGroup layout;
    private int[,] currentPuzzle;
    private int matrixSide;
    private int moves;
    public int Moves => moves;
    private int blankPiece;
    private Vector2Int blankPiecePosition;


    void Awake()
    {
        Init();
    }

    public void Init()
    {
        Instance = this;
        rect = GetComponent<RectTransform>();
        layout = GetComponent<GridLayoutGroup>();
    }

    public void SetPuzzlePhoto(Sprite sprite, int rowCount, List<int> puzzle)
    {
        // resize photo to center-cropped square
        Texture2D croppedTexture = PhotoSizeEqualizer(sprite.texture, out int baseSide);

        // generate pieces and resize panel to fit them
        SetPuzzlePieces(croppedTexture, baseSide, rowCount, puzzle);
    }

    public Sprite GetFullPhoto(int targetPhoto)
    {
        Texture2D croppedTexture = PhotoSizeEqualizer(GameManager.Instance.photoBank[targetPhoto].photo.texture, out int baseSide);
        return Sprite.Create(croppedTexture, new Rect(0, 0, baseSide, baseSide), new Vector2(0.5f, 0.5f));
    }

    private Texture2D PhotoSizeEqualizer(Texture2D originalTexture, out int smallerSide)
    {
        smallerSide = Mathf.Min(originalTexture.width, originalTexture.height);

        Color32[] originalPixels = originalTexture.GetPixels32();
        Color32[] croppedPixels = new Color32[smallerSide * smallerSide];

        int startX = (originalTexture.width - smallerSide) / 2;
        int startY = (originalTexture.height - smallerSide) / 2;

        for (int y = 0; y < smallerSide; y++)
        {
            for (int x = 0; x < smallerSide; x++)
            {
                int originalIndex = (startY + y) * originalTexture.width + startX + x;
                int croppedIndex = y * smallerSide + x;
                croppedPixels[croppedIndex] = originalPixels[originalIndex];
            }
        }

        Texture2D croppedTexture = new(smallerSide, smallerSide);
        croppedTexture.SetPixels32(croppedPixels);
        croppedTexture.Apply();

        return croppedTexture;
    }

    private Sprite[] PhotoSplice(Texture2D fullPhoto, int columnCount)
    {
        int pieceSide = fullPhoto.width / columnCount;
        Sprite[] pieces = new Sprite[columnCount * columnCount];

        Color32[] originalPhoto = fullPhoto.GetPixels32();
        Color32[] croppingFrame = new Color32[pieceSide * pieceSide];

        for (int i = 0; i < pieces.Length; i++)
        {
            int x = i % columnCount;
            int y = i / columnCount;

            int startY = (columnCount - 1 - y) * pieceSide;

            for (int j = 0; j < pieceSide; j++)
            {
                int originalRowStart = (startY + j) * fullPhoto.width + x * pieceSide;
                int croppedRowStart = j * pieceSide;

                for (int k = 0; k < pieceSide; k++)
                    croppingFrame[croppedRowStart + k] = originalPhoto[originalRowStart + k];
            }

            Texture2D pieceTexture = new Texture2D(pieceSide, pieceSide);
            pieceTexture.SetPixels32(croppingFrame);
            pieceTexture.Apply();

            pieces[i] = Sprite.Create(pieceTexture, new Rect(0, 0, pieceSide, pieceSide), new Vector2(0.5f, 0.5f));
        }

        return pieces;
    }

    private void SetPuzzlePieces(Texture2D photo, int side, int columnCount, List<int> puzzle)
    {
        // Resize panel
        rect.sizeDelta = new Vector2(872, 872);

        // Calculate piece size
        float pieceSize = side / columnCount;
        layout.cellSize = new Vector2(pieceSize, pieceSize);
        layout.constraintCount = columnCount;

        // Obtain pieces textures
        Sprite[] pieces = PhotoSplice(photo, columnCount);

        int totalPieces = columnCount * columnCount;

        int emptyPieceIndex = totalPieces - 1;

        for (int i = 0; i < totalPieces; i++)
        {
            int pieceIndex = puzzle[i];

            PuzzlePiece p = Instantiate(puzzlePiecePrefab, transform).GetComponent<PuzzlePiece>();

            if (pieceIndex == emptyPieceIndex)
                p.SetUpPiece(null, pieceIndex, false);
            else
                p.SetUpPiece(pieces[pieceIndex], pieceIndex, true);
        }

        //  init level
        moves = 0;
        InitializeMatrix(puzzle, columnCount);
    }

    private void InitializeMatrix(List<int> puzzle, int columnCount)
    {
        currentPuzzle = new int[columnCount, columnCount];
        blankPiece = columnCount * columnCount - 1;
        matrixSide = columnCount;

        for (int i = 0; i < puzzle.Count; i++)
        {
            int row = i / columnCount;
            int col = i % columnCount;
            currentPuzzle[row, col] = puzzle[i];

            if (puzzle[i] == blankPiece)
                blankPiecePosition = new Vector2Int(row, col);
        }
    }

    public void Abort()
    {
        foreach (Transform t in transform)
            Destroy(t.gameObject);
        rect.sizeDelta = Vector2.zero;
    }

    public void TryToMovePiece(int pieceIndex)
    {
        if (OnPieceClicked(pieceIndex))
        {
            moves++;
            UiManager.Instance.SetMoves(moves);
            if (CheckPuzzle())
                GameManager.Instance.WinLevel();
        }
    }

    public bool OnPieceClicked(int pieceIndex)
    {
        Vector2Int piecePosition = FindPiecePosition(pieceIndex);

        if (IsNeighbor(piecePosition, blankPiecePosition))
        {
            SwapPieces(piecePosition, blankPiecePosition);
            blankPiecePosition = piecePosition;
            return true;
        }

        return false;
    }

    private Vector2Int FindPiecePosition(int pieceIndex)
    {
        for (int row = 0; row < matrixSide; row++)
        {
            for (int col = 0; col < matrixSide; col++)
            {
                if (currentPuzzle[row, col] == pieceIndex)
                {
                    return new Vector2Int(row, col);
                }
            }
        }
        return new Vector2Int(-1, -1); // Not found
    }

    // Check if two positions are neighbors (up, down, left, right)
    private bool IsNeighbor(Vector2Int pos1, Vector2Int pos2)
    {
        int rowDiff = Mathf.Abs(pos1.x - pos2.x);
        int colDiff = Mathf.Abs(pos1.y - pos2.y);

        // Neighbors are adjacent horizontally or vertically, but not diagonally
        return (rowDiff == 1 && colDiff == 0) || (rowDiff == 0 && colDiff == 1);
    }

    // Swap two pieces in the matrix
    private void SwapPieces(Vector2Int pos1, Vector2Int pos2)
    {
        AudioManager.Instance.PlaySfx("Piece");
        layout.enabled = false;

        // store 2D pos to swap
        int temp = currentPuzzle[pos1.x, pos1.y];
        // store 1D positions to swap
        int temp1DPos = pos1.x * matrixSide + pos1.y;
        int temp2DPos = pos2.x * matrixSide + pos2.y;
        Transform t1, t2;
        t1 = transform.GetChild(temp1DPos);
        t2 = transform.GetChild(temp2DPos);
        currentPuzzle[pos1.x, pos1.y] = currentPuzzle[pos2.x, pos2.y];
        currentPuzzle[pos2.x, pos2.y] = temp;
        t2.SetSiblingIndex(temp1DPos);
        t1.SetSiblingIndex(temp2DPos);

        layout.enabled = true;
    }

    private bool CheckPuzzle()
    {
        for (int i = 0; i < matrixSide * matrixSide; i++)
            if (transform.GetChild(i).GetComponent<PuzzlePiece>().PieceNumber != i)
                return false;
        // win condition
        return true;
    }
}
