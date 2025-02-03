using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

public class UiManager : MonoBehaviour
{
    public static UiManager Instance { get; private set; }

    [Header("Curtains")]
    public RectTransform[] curtains;
    public float curtainsClosedAngle;
    public float curtainsOpenAngle;

    [Header("Buttons")]
    public Transform playButton;
    public float playButtonVisiblePos;
    public float playButtonHiddenPos;
    public Transform difficultyButtons;
    public float difficultyButtonsVisiblePos;
    public float difficultyButtonsHiddenPos;
    public GameObject inviteButton;

    [Header("Records")]
    public TextMeshProUGUI recEasy;
    public TextMeshProUGUI recMedium;
    public TextMeshProUGUI recHard;

    [Header("Audio Switch")]
    public Transform audioSwitch;
    public TextMeshProUGUI audioText;
    public float audioSwitchOnPos;
    public float audioSwitchOffPos;

    [Header("Back Button")]
    public Transform backButton;
    public float backButtonVisiblePos;
    public float backButtonHiddenPos;

    [Header("Background")]
    public Image background;
    public Sprite homeBack;
    public Sprite gameBack;

    [Header("Title")]
    public Transform title;
    public float titleVisiblePos;
    public float titleHiddenPos;

    [Header("Example")]
    public Image example;

    private bool isAudioOn = true;

    [Header("HUD")]
    public TextMeshProUGUI movesText;
    public TextMeshProUGUI timerText;

    [Header("Results Panel")]
    public UiResultsPanel results;

    [Header("Invite")]
    public CanvasGroup inviteCg;
    public Image invitePhoto;

    private const string LEVEL_RECORD = "RECORD_";
    private const string LEVEL_BEATEN = "BEATEN_";
    void Awake()
    {
        Instance = this;
    }

    public void Init()
    {
        // cheat to view button
        int i1, i2, i3;
        i1 = PlayerPrefs.GetInt(LEVEL_BEATEN + "Easy", 0);
        i2 = PlayerPrefs.GetInt(LEVEL_BEATEN + "Medium", 0);
        i3 = PlayerPrefs.GetInt(LEVEL_BEATEN + "Hard", 0);

        // if all levels beat
        if (i1 + i2 + i3 >= 2)
            PlayerPrefs.SetInt("INVITE", 1);

        OpenCurtains();
        ShowPlayButton(true);
        SetAudioToggle();
        invitePhoto.sprite = PuzzleController.Instance.GetFullPhoto(3);
        invitePhoto.rectTransform.sizeDelta = new Vector2(165, 165);
        CheckInvite();
    }

    private void CheckInvite()
    {
        int i = PlayerPrefs.GetInt("INVITE", 0);
        inviteButton.SetActive(i > 0);
    }

    public void OpenCurtains(Action onCompleteAction = null)
    {
        curtains[0].DOLocalRotate(new Vector3(0, 0, curtainsOpenAngle), .75f).SetEase(Ease.InBack);
        curtains[1].DOLocalRotate(new Vector3(0, 0, curtainsOpenAngle), .75f).SetEase(Ease.InBack).OnComplete(() => onCompleteAction?.Invoke());

    }

    public void CloseCurtains(Action onCompleteAction = null)
    {
        curtains[0].DOLocalRotate(new Vector3(0, 0, curtainsClosedAngle), .75f).SetEase(Ease.InExpo);
        curtains[1].DOLocalRotate(new Vector3(0, 0, curtainsClosedAngle), .75f).SetEase(Ease.InExpo).OnComplete(() => onCompleteAction?.Invoke());
    }

    public void PlayGame()
    {
        AudioManager.Instance.PlaySfx("Button");
        ShowPlayButton(false);
        ShowDifficulties(true);
    }

    public void SelectedDifficulty(string str)
    {
        AudioManager.Instance.PlaySfx(str);
        GameManager.Instance.OnDifficultySelected(str);
        ShowDifficulties(false);
    }

    public void BackToMain()
    {
        AudioManager.Instance.PlaySfx("Button");
        ShowDifficulties(false);
        ShowPlayButton(true);
    }

    public void BackToMainFromGame()
    {
        AudioManager.Instance.PlaySfx("Button");
        backButton.GetComponent<Button>().interactable = false;
        StartCoroutine(GoToMain());
    }

    private IEnumerator GoToMain()
    {
        CloseCurtains();
        yield return new WaitForSecondsRealtime(.8f);
        GameManager.Instance.CancelGame();
        OpenCurtains();
        ShowPlayButton(true);
        backButton.GetComponent<Button>().interactable = true;
    }

    public void MainFromWin()
    {
        CheckInvite();
        OpenCurtains();
        ShowDifficulties(false);
        ShowPlayButton(true);

    }

    public void ToggleAudio()
    {
        AudioManager.Instance.PlaySfx("Button");
        Button bAudio = audioSwitch.GetComponent<Button>();
        isAudioOn = !isAudioOn;

        bAudio.interactable = false;
        audioSwitch.DOLocalMoveX(isAudioOn ? audioSwitchOnPos : audioSwitchOffPos, .35f).OnComplete(() => bAudio.interactable = true);
        audioSwitch.GetComponent<Image>().DOColor(isAudioOn ? Color.white : Color.gray, .35f);
        audioText.text = isAudioOn ? "MUSIC ON" : "MUSIC OFF";

        AudioManager.Instance.ToggleAudio(isAudioOn);
    }

    public void ToggleGameMode(bool isPlaying)
    {
        GameObject exampleGO = example.transform.parent.gameObject;
        background.sprite = isPlaying ? gameBack : homeBack;
        title.DOLocalMoveY(isPlaying ? titleHiddenPos : titleVisiblePos, isPlaying ? .35f : .82f).SetEase(Ease.InOutBack);
        movesText.gameObject.SetActive(isPlaying);
        timerText.gameObject.SetActive(isPlaying);

        if (!isPlaying)
        {
            exampleGO.SetActive(false);
            ShowBackButton(false);
        }
        else
        {
            SetTimer(0);
            SetMoves(0);
            exampleGO.SetActive(true);
            example.sprite = PuzzleController.Instance.GetFullPhoto(GameManager.Instance.CurrentLevel());
        }
    }

    public void ShowBackButton(bool isPlaying)
    {
        backButton.DOLocalMoveX(isPlaying ? backButtonVisiblePos : backButtonHiddenPos, .37f);
    }

    public void SetMoves(int moves)
    {
        movesText.text = $"MOVES: {moves}";
    }

    public void SetTimer(float time)
    {
        TimeSpan ts = TimeSpan.FromSeconds(time);
        string formattedTime = string.Format("{0}:{1:D2}", (int)ts.TotalMinutes, ts.Seconds);
        timerText.text = $"TIME: {formattedTime}";
    }

    public void ShowResults(float finalTime, int finalMoves, string diff)
    {
        results.ShowPanel(finalTime, finalMoves, diff);
    }

    public void ShowPlayButton(bool visible)
    {
        Button bPlay = playButton.GetChild(0).GetComponent<Button>();

        if (visible)
            playButton.DOLocalMoveY(playButtonVisiblePos, .65f).SetEase(Ease.OutCubic).OnComplete(() => bPlay.interactable = true);
        else
        {
            bPlay.interactable = false;
            playButton.DOLocalMoveY(playButtonHiddenPos, .7f).SetEase(Ease.InOutBack);
        }
    }

    public void ShowInvite(bool isOn)
    {
        AudioManager.Instance.PlaySfx(isOn ? "Invite" : "Button");
        inviteCg.interactable = inviteCg.blocksRaycasts = isOn;
        inviteCg.DOFade(isOn ? 1 : 0, isOn ? 1.15f : .45f);
    }

    private void ShowDifficulties(bool visible)
    {
        CanvasGroup diffGroup = difficultyButtons.GetComponent<CanvasGroup>();

        int temp;
        temp = PlayerPrefs.GetInt(LEVEL_BEATEN + "Easy", 0);
        difficultyButtons.GetChild(1).GetComponent<Button>().interactable = temp > 0;
        temp = PlayerPrefs.GetInt(LEVEL_BEATEN + "Medium", 0);
        difficultyButtons.GetChild(2).GetComponent<Button>().interactable = temp > 0;

        // show records
        temp = PlayerPrefs.GetInt(LEVEL_RECORD + "Easy", -1);
        if (temp != -1) recEasy.text = $"BEST: {temp} MOVES";
        temp = PlayerPrefs.GetInt(LEVEL_RECORD + "Medium", -1);
        if (temp != -1) recMedium.text = $"BEST: {temp} MOVES";
        temp = PlayerPrefs.GetInt(LEVEL_RECORD + "Hard", -1);
        if (temp != -1) recHard.text = $"BEST: {temp} MOVES";

        if (visible)
            difficultyButtons.DOLocalMoveY(difficultyButtonsVisiblePos, .65f).SetEase(Ease.OutCubic).OnComplete(() => diffGroup.interactable = true);
        else
        {
            diffGroup.interactable = false;
            difficultyButtons.DOLocalMoveY(difficultyButtonsHiddenPos, .7f).SetEase(Ease.InOutCubic);
        }
    }

    private void SetAudioToggle()
    {
        isAudioOn = AudioManager.Instance.IsMusicOn;

        audioText.text = isAudioOn ? "MUSIC ON" : "MUSIC OFF";
        audioSwitch.DOLocalMoveX(isAudioOn ? audioSwitchOnPos : audioSwitchOffPos, 0);
        audioSwitch.GetComponent<Image>().DOColor(isAudioOn ? Color.white : Color.gray, 0);
    }
}
