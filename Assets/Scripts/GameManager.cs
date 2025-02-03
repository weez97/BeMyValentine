using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public enum Difficulty { Easy = 3, Medium = 4, Hard = 5, Special = 100 }

public static class ListExtensions
{
    public static void Shuffle<T>(this List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            (list[k], list[n]) = (list[n], list[k]); // Swap
        }
    }
}

public class GameManager : MonoBehaviour
{
    [System.Serializable]
    public class PhotoPuzzle
    {
        public Difficulty difficulty;
        public Sprite photo;
    }

    public static GameManager Instance { get; private set; }

    [Header("Resources")]
    public PhotoPuzzle[] photoBank;

    private Dictionary<Difficulty, Sprite> sortedPhotos = new();
    private Difficulty selectedDifficulty;
    private readonly Dictionary<string, Difficulty> difficultyMap = new(){
    { "Easy", Difficulty.Easy },
    { "Medium", Difficulty.Medium },
    { "Hard", Difficulty.Hard }
};
    private bool playing = false;
    private bool gameRunning = false;
    private float timer = 0;

    public int CurrentLevel()
    {
        switch (selectedDifficulty)
        {
            case Difficulty.Easy:
                return 0;
            case Difficulty.Medium:
                return 1;
            case Difficulty.Hard:
                return 2;
        }
        return -1;
    }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        foreach (PhotoPuzzle pp in photoBank)
            sortedPhotos.TryAdd(pp.difficulty, pp.photo);

        AudioManager.Instance.Init();
        UiManager.Instance.Init();
    }

    public void CancelGame()
    {
        playing = gameRunning = false;
        PuzzleController.Instance.Abort();
        UiManager.Instance.ToggleGameMode(playing);
    }

    public void OnDifficultySelected(string diff)
    {
        if (difficultyMap.TryGetValue(diff, out var difficulty))
            selectedDifficulty = difficulty;

        StartCoroutine(GenerateLevel());
    }

    private IEnumerator GenerateLevel()
    {
        if (!sortedPhotos.TryGetValue(selectedDifficulty, out Sprite targetPhoto)) yield return null;

        UiManager.Instance.CloseCurtains(() => DoGenerateLevel(targetPhoto, (int)selectedDifficulty));

        yield return new WaitForSecondsRealtime(.75f);
        playing = true;
        UiManager.Instance.ToggleGameMode(playing);
        yield return new WaitForSecondsRealtime(.35f);

        UiManager.Instance.OpenCurtains(() => UiManager.Instance.ShowBackButton(playing));
        GameStart();
    }

    private void DoGenerateLevel(Sprite photo, int rowCount)
    {
        List<int> initialPuzzle = new();
        for (int i = 0; i < rowCount * rowCount; i++)
            initialPuzzle.Add(i);

        initialPuzzle.Shuffle();

        PuzzleController.Instance.SetPuzzlePhoto(photo, rowCount, initialPuzzle);
    }

    private void GameStart()
    {
        timer = 0;
        gameRunning = true;
    }

    public void WinLevel()
    {
        gameRunning = false;
        UiManager.Instance.ShowResults(timer, PuzzleController.Instance.Moves, selectedDifficulty.ToString());
    }

    void Update()
    {
        if (gameRunning)
        {
            timer += Time.deltaTime;
            UiManager.Instance.SetTimer(timer);
        }
    }
}
