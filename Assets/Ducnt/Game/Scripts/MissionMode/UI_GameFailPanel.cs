using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_GameFailPanel : MonoBehaviour
{
    public GameObject content;

    [Header("UI Texts On Scoreboard")]
    public Text totalScore;
    public Text subTotalMoney;
    public Text totalMoney;

    public Text totalGold;
    public Text totalDiamond;

    public Text totalGoldMoney;
    public Text totalDiamondMoney;
    public Text winOrLoseText;

    public GameObject restartButton;

    private void OnEnable()
    {

        HR_GamePlayHandler.OnPlayerFailed += HR_PlayerHandler_OnPlayerFailed;
        HR_GamePlayHandler.OnPlayerFailedTime += HR_PlayerHandler_OnPlayerFailed;
        HR_GamePlayHandler.OnPlayerRevive += HR_PlayerHandler_OnPlayerRevive;
#if PHOTON_UNITY_NETWORKING && BCG_HR_PHOTON
        HR_PhotonHandler.OnNetworkPlayerWon += HR_GamePlayHandler_OnNetworkPlayerWon;
#endif

    }

    private void HR_PlayerHandler_OnPlayerFailed(HR_PlayerHandler player, int[] scores)
    {

        StartCoroutine(DisplayResults(player, scores));

    }
    private void HR_PlayerHandler_OnPlayerRevive(HR_PlayerHandler player)
    {
        content.SetActive(false);
    }
    public IEnumerator DisplayResults(HR_PlayerHandler player, int[] scores)
    {
        ProgressMissionMode.Instance.disableProgressBar();
        yield return new WaitForSecondsRealtime(1f);

        content.SetActive(true);

        if (player.timeLeft <= 0)
        {
            //het thoi gian
            winOrLoseText.text = "Time Out!";
        }
        else
        {
            //tong xe
            winOrLoseText.text = "Crash!";
        }

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

        HR_GamePlayHandler.OnPlayerFailed -= HR_PlayerHandler_OnPlayerFailed;
        HR_GamePlayHandler.OnPlayerFailedTime -= HR_PlayerHandler_OnPlayerFailed;
        HR_GamePlayHandler.OnPlayerRevive -= HR_PlayerHandler_OnPlayerRevive;
#if PHOTON_UNITY_NETWORKING && BCG_HR_PHOTON
        HR_PhotonHandler.OnNetworkPlayerWon -= HR_GamePlayHandler_OnNetworkPlayerWon;
#endif

    }
}
