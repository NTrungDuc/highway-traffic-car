using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HPS_UISummarizePanel : MonoBehaviour
{
    public GameObject content;
    public GameObject infos;
    public GameObject buttons;
    public GameObject unlockBox;
    public GameObject modeTitles;
    public GameObject homeButton;
    public GameObject reviveButton;
    public GameObject doubleCoinButton;
    public GameObject restartButton;
    public GameObject nextButton;
    public GameObject BGComplete;
    public GameObject Ranking;

    public RankUp rankUp;
    [Header("UI Texts On Scoreboard")]
    public Text rankName;
    public Text rankPoint;
    public Text subTotalMoney;
    public Text totalMoney;

    public Text totalDistance;
    public Text totalPoint;
    public Text totalTimeRemaining;
    public Text totalGold;
    public Text totalDiamond;

    public Text totalDistanceMoney;
    public Text totalPointMoney;
    public Text totalTimeRemainingMoney;
    public Text totalGoldMoney;
    public Text totalDiamondMoney;
    public Text winOrLoseText;
    public Text bonus;
    public Text failedText;
    public Text completeText;
    public Text nextGame;
    private void OnEnable()
    {

        HR_GamePlayHandler.OnPlayerDied += OnGameEnd;
        HR_GamePlayHandler.OnPlayerWon += OnGameEnd;
        HR_GamePlayHandler.OnPlayerFailed += OnGameFail;
        HR_GamePlayHandler.OnPlayerRevive += HR_PlayerHandler_OnPlayerRevive;

#if PHOTON_UNITY_NETWORKING && BCG_HR_PHOTON
        HR_PhotonHandler.OnNetworkPlayerWon += HR_GamePlayHandler_OnNetworkPlayerWon;
#endif

    }

    void Start()
    {
        for (int i = 0; i < modeTitles.transform.childCount; i++)
            modeTitles.transform.GetChild(i).gameObject.SetActive(false);

        //if (HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.MissionMode_1 || HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.MissionMode_2)
        //{
        //    nextButton.GetComponent<Button>().onClick.RemoveAllListeners();
        //    nextButton.GetComponent<Button>().onClick.AddListener(() => HR_GamePlayHandler.Instance.RestartGame());
        //}
        //if (HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.AIRacing)
        //{
        //    nextButton.GetComponent<Button>().onClick.RemoveAllListeners();
        //    nextButton.GetComponent<Button>().onClick.AddListener(() => HR_GamePlayHandler.Instance.next());
        //}
    }

    private void OnGameEnd(HR_PlayerHandler player, int[] scores)
    {
        StartCoroutine(showUnlockBox(player, scores));
        reviveButton.SetActive(false);
        if (HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.OneWay || HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.TwoWay
            || HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.MissionMode_1 || HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.MissionMode_2)
        {
            restartButton.SetActive(true);
            nextButton.SetActive(false);
        }
        else
        {
            restartButton.SetActive(false);
            nextButton.SetActive(true);
        }
        doubleCoinButton.SetActive(true);
        failedText.text = "";
        nextGame.text = "NEXT";
    }

    public void OnGameFail(HR_PlayerHandler player, int[] scores)
    {
        StartCoroutine(ShowUIGameFail(player, scores));
        reviveButton.SetActive(true);
        doubleCoinButton.SetActive(false);
        restartButton.SetActive(true);
        nextButton.SetActive(false);
        if (player.timeLeft <= 0)
            failedText.text = "Timed out";
        else
            failedText.text = "Crashed!";
        nextGame.text = "RESTART";
    }
    public void HR_PlayerHandler_OnPlayerRevive(HR_PlayerHandler player)
    {
        content.SetActive(false);
        buttons.SetActive(false);
        ProgressMissionMode.Instance.content.SetActive(true);
    }
    public IEnumerator ShowUIGameFail(HR_PlayerHandler player, int[] scores)
    {
        yield return new WaitForSecondsRealtime(1f);

        content.SetActive(true);
        StartCoroutine(DisplayResults(player, scores, false, 0));
        AudioListener.pause = true;
    }

    public IEnumerator showUnlockBox(HR_PlayerHandler player, int[] scores)
    {
        if (HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.MissionMode_1 || HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.MissionMode_2)
        {
            BGComplete.SetActive(true);
            completeText.text = "Mission Complete";
        }
        else if (HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.AIRacing)
        {
            BGComplete.SetActive(true);
            if (!HR_GamePlayHandler.Instance.isCheckTimeAI)
                completeText.text = "Dual Complete";
            else
                completeText.text = "Dual Fail";
        }
        else
        {
            BGComplete.SetActive(false);
        }
        yield return new WaitForSecondsRealtime(0.5f);

        content.SetActive(true);
        if (HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.MissionMode_1 || HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.MissionMode_2)
        {
            ProgressMissionMode.Instance.disableProgressBar();
            yield return new WaitForSecondsRealtime(1f);
            unlockBox.gameObject.SetActive(true);
            if (PlayerPrefs.GetFloat("ProgressBox") != 0.9f)
            {
                StartCoroutine(DisplayRanking(player, scores, 3));
            }
            else
            {
                unlockBox.GetComponent<UnlockBox>().BtnClaim.GetComponent<Button>().onClick.AddListener(() => { StartCoroutine(DisplayResults(player, scores, true, 0)); });
                unlockBox.GetComponent<UnlockBox>().BtnNoThanks.GetComponent<Button>().onClick.AddListener(() => { StartCoroutine(DisplayResults(player, scores, true, 0)); });
            }
        }
        else
            StartCoroutine(DisplayRanking(player, scores, 0));
        AudioListener.pause = true;
    }

    public IEnumerator DisplayRanking(HR_PlayerHandler player, int[] scores, int timeShow)
    {
        yield return new WaitForSeconds(timeShow);
        Ranking.SetActive(true);
        float targetRankPoint = PlayerPrefs.GetFloat("CurrentRankPoint", 0) + (scores[4] * 0.1f);
        
        //if (rankPoint)
        //    rankPoint.text = (targetRankPoint).ToString();

        rankUp.GetCurrentRank(rankName, targetRankPoint);

        PlayerPrefs.SetFloat("CurrentRankPoint", targetRankPoint);
        
        yield return new WaitUntil(() => rankUp.isFinished);
        StartCoroutine(DisplayResults(player, scores, true, 2));
    }

    public IEnumerator DisplayResults(HR_PlayerHandler player, int[] scores, bool isActive, int timeShow)
    {
        yield return new WaitForSeconds(timeShow);
        Ranking.SetActive(false);
        HPS_Ads.Instance.leftBanner.SetActive(true);
        if (isActive)
            infos.gameObject.SetActive(true);

        buttons.SetActive(true);

        switch (PlayerPrefs.GetInt("SelectedModeIndex"))
        {
            case 0:
            case 1:
                modeTitles.transform.GetChild(3).gameObject.SetActive(true);
                break;
            case 4:
            case 5:
                modeTitles.transform.GetChild(0).gameObject.SetActive(true);
                break;
            case 6:
                modeTitles.transform.GetChild(1).gameObject.SetActive(true);
                break;
        }

        if (totalPoint)
            totalPoint.text = (player.score).ToString();

        if (totalTimeRemaining)
            totalTimeRemaining.text = (player.showTimeLeft(false)).ToString();

        if (totalGold)
            totalGold.text = (player.gold).ToString();

        if (totalDiamond)
            totalDiamond.text = (player.diamond).ToString();

        if (totalDistanceMoney)
            totalDistanceMoney.text = scores[0].ToString();

        if (totalPointMoney)
            totalPointMoney.text = scores[1].ToString();

        if (totalTimeRemainingMoney)
            totalTimeRemainingMoney.text = "0";

        if (totalGoldMoney)
            totalGoldMoney.text = scores[2].ToString();

        if (totalDiamondMoney)
            totalDiamondMoney.text = scores[3].ToString();



        gameObject.BroadcastMessage("Animate");
        if (isActive)
            gameObject.BroadcastMessage("GetNumber");
        if (HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.AIRacing)
        {
            if (winOrLoseText)
                winOrLoseText.text = PlayerPrefs.GetInt("checkWinOrLost").ToString();


            if (totalDistance)
                totalDistance.text = (player.distance * 1000).ToString("F1") + " M";
            if (PlayerPrefs.GetInt("checkWinOrLost") == 1)
            {
                if (bonus)
                    bonus.text = (PlayerPrefs.GetInt("priceModeDual") * 2).ToString();
                restartButton.SetActive(false);
                nextButton.SetActive(true);
                if (totalMoney)
                    totalMoney.text = (scores[0] + scores[1] + scores[2] + scores[3] + (PlayerPrefs.GetInt("priceModeDual") * 2)).ToString();
            }
            else
            {
                if (bonus)
                    bonus.text = "0";
                restartButton.SetActive(true);
                nextButton.SetActive(false);
                if (totalMoney)
                    totalMoney.text = (scores[0] + scores[1] + scores[2] + scores[3]).ToString();
            }

        }
        else if (HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.MissionMode_1
            || HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.MissionMode_2)
        {
            winOrLoseText.gameObject.transform.parent.parent.gameObject.SetActive(false);
            bonus.gameObject.transform.parent.parent.gameObject.SetActive(false);
            if (totalDistance)
                totalDistance.text = (player.distanceWinMM * 1000).ToString("F1") + " M";
            if (totalMoney)
                totalMoney.text = (scores[0] + scores[1] + scores[2] + scores[3]).ToString();
        }
        else
        {
            winOrLoseText.gameObject.transform.parent.parent.gameObject.SetActive(false);
            bonus.gameObject.transform.parent.parent.gameObject.SetActive(false);
            if (totalDistance)
                totalDistance.text = (player.distance * 1000).ToString("F1") + " M";
            if (totalMoney)
                totalMoney.text = (scores[0] + scores[1] + scores[2] + scores[3]).ToString();
        }


        //if (PlayerPrefs.GetInt("Multiplayer", 0) == 1)
        //    restartButton.SetActive(false);
        //else
        //    restartButton.SetActive(true);
        
    }

    private void OnDisable()
    {
        HR_GamePlayHandler.OnPlayerDied -= OnGameEnd;
        HR_GamePlayHandler.OnPlayerWon -= OnGameEnd;
        HR_GamePlayHandler.OnPlayerFailed -= OnGameFail;
        HR_GamePlayHandler.OnPlayerRevive -= HR_PlayerHandler_OnPlayerRevive;
    }
}
