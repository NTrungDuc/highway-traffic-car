//----------------------------------------------
//           	   Highway Racer
//
// Copyright © 2014 - 2023 BoneCracker Games
// http://www.bonecrackergames.com
//
//----------------------------------------------

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HR_UICountDown : MonoBehaviour {

    [SerializeField] private TextMeshProUGUI level;
    [SerializeField] private TextMeshProUGUI time;
    [SerializeField] private TextMeshProUGUI distance;

    private void OnEnable() {

        HR_GamePlayHandler.OnCountDownStarted += HR_GamePlayHandler_OnCountDownStarted;

    }

    private void HR_GamePlayHandler_OnCountDownStarted() {

        if (HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.MissionMode_1 || HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.MissionMode_2)
        {
            level.text = $"LEVEL: {PlayerPrefs.GetInt("levelMissionMode", 1)}";
            time.text = $"{DataLevelMissionMode.Instance.levelData.levelGames[PlayerPrefs.GetInt("levelMissionMode", 1) - 1].time}s";
            distance.text = DataLevelMissionMode.Instance.levelData.levelGames[PlayerPrefs.GetInt("levelMissionMode", 1) - 1].distanceWin.ToString("F2") + " KM";
            GetComponent<Animator>().SetTrigger("Count");
        }
        else
            GetComponent<Animator>().SetTrigger("CountNoInfo");
    
    }

    private void OnDisable() {

        HR_GamePlayHandler.OnCountDownStarted -= HR_GamePlayHandler_OnCountDownStarted;

    }

}
