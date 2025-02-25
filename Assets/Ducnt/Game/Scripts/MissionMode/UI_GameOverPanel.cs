using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_GameOverPanel : MonoBehaviour
{
    public GameObject content;
    public RankUp rankUp;
    [Header("UI Texts On Scoreboard")]
    public Text rankName;
    public Text rankPoint;
    public Text subTotalMoney;
    public Text totalMoney;

    public Text totalDistance;
    public Text totalNearMiss;
    public Text totalTimeRemaining;
    public Text totalGold;
    public Text totalDiamond;

    public Text totalDistanceMoney;
    public Text totalNearMissMoney;
    public Text totalTimeRemainingMoney;
    public Text totalGoldMoney;
    public Text totalDiamondMoney;
    public Text winOrLoseText;
    public Text bonusText;

    public GameObject restartButton;

    private void OnEnable()
    {

        HR_GamePlayHandler.OnPlayerDied += HR_PlayerHandler_OnPlayerDied;
        HR_GamePlayHandler.OnPlayerRevive += HR_PlayerHandler_OnPlayerRevive;

#if PHOTON_UNITY_NETWORKING && BCG_HR_PHOTON
        HR_PhotonHandler.OnNetworkPlayerWon += HR_GamePlayHandler_OnNetworkPlayerWon;
#endif

    }

    private void HR_PlayerHandler_OnPlayerDied(HR_PlayerHandler player, int[] scores)
    {
        if(HR_GamePlayHandler.Instance.mode != HR_GamePlayHandler.Mode.AIRacing)
            StartCoroutine(DisplayResults(player, scores));
        else if(HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.AIRacing && HR_GamePlayHandler.Instance.player.isCheckLostOrWin)
        {
            Debug.Log("viet_gameOversau");
            StartCoroutine(DisplayResults(player, scores));
        }
    }
    private void HR_PlayerHandler_OnPlayerRevive(HR_PlayerHandler player)
    {
        gameObject.BroadcastMessage("ResetValueUI");
        gameObject.BroadcastMessage("ResetValueAnimation");
        content.SetActive(false);
    }
    
    public IEnumerator DisplayResults(HR_PlayerHandler player, int[] scores)
    {

        yield return new WaitForSecondsRealtime(1f);

        content.SetActive(true);

        float currentRankPoint = PlayerPrefs.GetFloat("CurrentRankPoint", 0) + (scores[4] * 0.1f);

        rankUp.GetCurrentRank(rankName, currentRankPoint);

        if (rankPoint)
            rankPoint.text = (currentRankPoint).ToString();

        if (totalDistance)
            totalDistance.text = (player.distance * 1000).ToString("F2");

        if (totalNearMiss)
            totalNearMiss.text = (player.score).ToString("F0");

        if (totalTimeRemaining)
            totalTimeRemaining.text = (player.showTimeLeft(false)).ToString();

        if (totalGold)
            totalGold.text = (player.gold).ToString("F1");

        if (totalDiamond)
            totalDiamond.text = (player.diamond).ToString("F1");

        if (totalDistanceMoney)
            totalDistanceMoney.text = scores[0].ToString("F0");

        if (totalNearMissMoney)
            totalNearMissMoney.text = scores[1].ToString("F0");

        if (totalTimeRemainingMoney)
            totalTimeRemainingMoney.text = "0";

        if (totalGoldMoney)
            totalGoldMoney.text = scores[2].ToString("F0");

        if (totalDiamondMoney)
            totalDiamondMoney.text = scores[3].ToString("F0");

        if (totalMoney)
            totalMoney.text = (scores[0] + scores[1] + scores[2] + scores[3]).ToString();

        if (HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.AIRacing)
        {
            if (winOrLoseText)
                winOrLoseText.text = PlayerPrefs.GetInt("checkWinOrLost").ToString();

            if (bonusText)
                bonusText.text = "0";
        }
        else
        {
            winOrLoseText.gameObject.transform.parent.gameObject.SetActive(false);
            bonusText.gameObject.transform.parent.gameObject.SetActive(false);

        }

        PlayerPrefs.SetFloat("CurrentRankPoint", currentRankPoint);

        gameObject.BroadcastMessage("Animate");
        gameObject.BroadcastMessage("GetNumber");

        if (PlayerPrefs.GetInt("Multiplayer", 0) == 1)
            restartButton.SetActive(false);
        else
            restartButton.SetActive(true);

    }

#if PHOTON_UNITY_NETWORKING && BCG_HR_PHOTON

    private void HR_GamePlayHandler_OnNetworkPlayerWon(int viewID) {

        if (PhotonView.Find(viewID).GetComponent<HR_PlayerHandler>() == HR_GamePlayHandler.Instance.player)
            winOrLoseText.text = "Win!";
        else
            winOrLoseText.text = "Lose!";

    }

#endif

    private void OnDisable()
    {

        HR_GamePlayHandler.OnPlayerDied -= HR_PlayerHandler_OnPlayerDied;
        HR_GamePlayHandler.OnPlayerRevive -= HR_PlayerHandler_OnPlayerRevive;
#if PHOTON_UNITY_NETWORKING && BCG_HR_PHOTON
        HR_PhotonHandler.OnNetworkPlayerWon -= HR_GamePlayHandler_OnNetworkPlayerWon;
#endif

    }

}
