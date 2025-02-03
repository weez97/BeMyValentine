using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class UiResultsPanel : MonoBehaviour
{
    [Header("References")]
    public Transform panel;
    public float panelHiddenPos;
    public TextMeshProUGUI subtitle;
    public TextMeshProUGUI finalTimer;
    public TextMeshProUGUI finalMoves;
    public Transform newRecord;
    public Vector3 recordInitialPos;
    public Vector3 recordFinalPos;
    public Transform playAgainButton;
    public GameObject inviteButton;

    private CanvasGroup canvasGroup;
    private CanvasGroup recordCg;
    private const string LEVEL_RECORD = "RECORD_";
    private const string LEVEL_BEATEN = "BEATEN_";

    public void ShowPanel(float time, int moves, string diff)
    {
        canvasGroup = GetComponent<CanvasGroup>();
        recordCg = newRecord.GetComponent<CanvasGroup>();

        canvasGroup.interactable = canvasGroup.blocksRaycasts = true;
        finalTimer.text = "TOTAL TIME: 00:00";
        finalMoves.text = "TOTAL MOVES: 0";
        PlayerPrefs.SetInt(LEVEL_BEATEN + diff, 1);

        StartCoroutine(ResultSequence(time, moves, diff));
    }

    private IEnumerator ResultSequence(float time, int moves, string diff)
    {
        bool recordFlag = false;
        panel.DOLocalMoveY(0, 2.22f).SetEase(Ease.OutElastic);
        yield return new WaitForSecondsRealtime(3f);

        subtitle.text = $"YOU BEAT THE {diff} LEVEL";
        subtitle.GetComponent<CanvasGroup>().DOFade(1, .39f);

        yield return new WaitForSecondsRealtime(2.1f);

        finalTimer.GetComponent<CanvasGroup>().DOFade(1, 0);

        yield return new WaitForSecondsRealtime(.6f);

        float temp = 0;
        DOTween.To(
            () => temp,
            x => temp = x,
            time,
            .4f
        ).OnUpdate(() =>
        {
            TimeSpan ts = TimeSpan.FromSeconds(temp);
            string formattedTime = string.Format("{0}:{1:D2}", (int)ts.TotalMinutes, ts.Seconds);
            finalTimer.text = $"TOTAL TIME: {formattedTime}";
        });

        yield return new WaitForSecondsRealtime(.5f);

        finalMoves.GetComponent<CanvasGroup>().DOFade(1, 0);

        yield return new WaitForSecondsRealtime(.6f);

        int itemp = 0;
        DOTween.To(
            () => itemp,
            x => itemp = Mathf.RoundToInt(x),
            moves,
            .4f
        ).OnUpdate(() =>
        {
            finalMoves.text = $"TOTAL MOVES: {itemp}";
        });

        yield return new WaitForSecondsRealtime(.5f);

        int tempRec = PlayerPrefs.GetInt(LEVEL_RECORD + diff, -1);

        if (tempRec == -1 || moves < tempRec)
            recordFlag = true;

        if (recordFlag)
        {
            PlayerPrefs.SetInt(LEVEL_RECORD + diff, moves);

            yield return new WaitForSecondsRealtime(.15f);

            recordCg.DOFade(1, .17f);
            newRecord.DOPunchScale(new Vector3(0.1f, 0.1f, 0.1f), .4f);
        }

        yield return new WaitForSecondsRealtime(1.15f);

        int i1, i2, i3;
        i1 = PlayerPrefs.GetInt(LEVEL_BEATEN + "Easy", 0);
        i2 = PlayerPrefs.GetInt(LEVEL_BEATEN + "Medium", 0);
        i3 = PlayerPrefs.GetInt(LEVEL_BEATEN + "Hard", 0);

        // if all levels beat
        if (i1 + i2 + i3 >= 2)
        {
            inviteButton.SetActive(true);
            PlayerPrefs.SetInt("INVITE", 1);
        }

        playAgainButton.gameObject.SetActive(true);
    }

    public void HidePanel()
    {
        UiManager.Instance.CloseCurtains(() =>
        {
            canvasGroup.interactable = canvasGroup.blocksRaycasts = false;
            panel.DOLocalMoveY(panelHiddenPos, 0);
            subtitle.GetComponent<CanvasGroup>().DOFade(0, 0);
            finalTimer.GetComponent<CanvasGroup>().DOFade(0, 0);
            finalMoves.GetComponent<CanvasGroup>().DOFade(0, 0);
            recordCg.DOFade(0, 0);
            playAgainButton.gameObject.SetActive(false);

            GameManager.Instance.CancelGame();
            UiManager.Instance.MainFromWin();
        });
    }

    public void DoShowInvite()
    {
        UiManager.Instance.ShowInvite(true);
    }
}
