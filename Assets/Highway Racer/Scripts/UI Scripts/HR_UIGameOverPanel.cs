//----------------------------------------------
//           	   Highway Racer
//
// Copyright © 2014 - 2023 BoneCracker Games
// http://www.bonecrackergames.com
//
//----------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
#if PHOTON_UNITY_NETWORKING
using Photon;
using Photon.Pun;
#endif

[AddComponentMenu("BoneCracker Games/Highway Racer/UI/HR UI Gameover Panel")]
public class HR_UIGameOverPanel : MonoBehaviour {

    public GameObject content;

    [Header("UI Texts On Scoreboard")]
    public Text totalScore;
    public Text subTotalMoney;
    public Text totalMoney;

    public Text totalDistance;
    public Text totalNearMiss;
    public Text totalOverspeed;
    public Text totalOppositeDirection;

    public Text totalDistanceMoney;
    public Text totalNearMissMoney;
    public Text totalOverspeedMoney;
    public Text totalOppositeDirectionMoney;
    public Text winOrLoseText;

    public GameObject restartButton;

    private void OnEnable() {

        HR_GamePlayHandler.OnPlayerDied += HR_PlayerHandler_OnPlayerDied;
        HR_GamePlayHandler.OnPlayerRevive += HR_PlayerHandler_OnPlayerRevive;

#if PHOTON_UNITY_NETWORKING && BCG_HR_PHOTON
        HR_PhotonHandler.OnNetworkPlayerWon += HR_GamePlayHandler_OnNetworkPlayerWon;
#endif

    }

    private void HR_PlayerHandler_OnPlayerDied(HR_PlayerHandler player, int[] scores) {

        StartCoroutine(DisplayResults(player, scores));

    }

    private void HR_PlayerHandler_OnPlayerRevive(HR_PlayerHandler player)
    {
        content.SetActive(false);
    }

    public IEnumerator DisplayResults(HR_PlayerHandler player, int[] scores) {

        yield return new WaitForSecondsRealtime(1f);

        content.SetActive(true);

        if (totalScore)
            totalScore.text = Mathf.Floor(player.score).ToString("F0");

        if (totalDistance)
            totalDistance.text = (player.distance).ToString("F2");

        if (totalNearMiss)
            totalNearMiss.text = (player.score).ToString("F0");

        if (totalOverspeed)
            totalOverspeed.text = (player.highSpeedTotal).ToString("F1");

        if (totalOppositeDirection)
            totalOppositeDirection.text = (player.opposideDirectionTotal).ToString("F1");

        if (totalDistanceMoney)
            totalDistanceMoney.text = scores[0].ToString("F0");

        if (totalNearMissMoney)
            totalNearMissMoney.text = scores[1].ToString("F0");

        if (totalOverspeedMoney)
            totalOverspeedMoney.text = scores[2].ToString("F0");

        if (totalOppositeDirectionMoney)
            totalOppositeDirectionMoney.text = scores[3].ToString("F0");

        if (totalMoney)
            totalMoney.text = (scores[0] + scores[1] + scores[2] + scores[3]).ToString();
        if (winOrLoseText)
            winOrLoseText.text = PlayerPrefs.GetString("checkWinOrLost");
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

    private void OnDisable() {

        HR_GamePlayHandler.OnPlayerDied -= HR_PlayerHandler_OnPlayerDied;

#if PHOTON_UNITY_NETWORKING && BCG_HR_PHOTON
        HR_PhotonHandler.OnNetworkPlayerWon -= HR_GamePlayHandler_OnNetworkPlayerWon;
#endif

    }

}
