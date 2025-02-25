//----------------------------------------------
//           	   Highway Racer
//
// Copyright © 2014 - 2023 BoneCracker Games
// http://www.bonecrackergames.com
//
//----------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("BoneCracker Games/Highway Racer/UI/HR UI Dynamic Score Displayer")]
public class HR_UIDynamicScoreDisplayer : MonoBehaviour {

    #region SINGLETON PATTERN
    private static HR_UIDynamicScoreDisplayer instance;
    public static HR_UIDynamicScoreDisplayer Instance {
        get {
            if (instance == null) {
                instance = FindObjectOfType<HR_UIDynamicScoreDisplayer>();
            }

            return instance;
        }
    }
    #endregion

    private GameObject scoreText;
    private GameObject[] scoreTexts;
    private int index = 0;

    private float lifeTime = .5f;
    private float timer = 0f;
    private Vector3 defPos;

    public enum Side { Left, Right, Center }

    private AudioSource nearMissSound;
    private AudioSource destroySound;

    private void Start() {

        scoreText = transform.GetChild(0).gameObject;
        scoreText.SetActive(false);

        scoreTexts = new GameObject[10];

        for (int i = 0; i < 10; i++) {

            GameObject instantiatedText = Instantiate(scoreText, transform);
            scoreTexts[i] = instantiatedText;
            scoreTexts[i].gameObject.SetActive(true);

        }

        timer = 0f;
        defPos = scoreTexts[0].transform.position - new Vector3(0f, 508f, 0f);

    }

    private void OnEnable() {

        HR_PlayerHandler.OnNearMiss += HR_PlayerHandler_OnNearMiss;
        HR_PlayerHandler.OnDestroyTrafficCar += HR_PlayerHandler_OnDestroyTrafficCar;

    }

    private void HR_PlayerHandler_OnNearMiss(HR_PlayerHandler player, int score, Side side) {

        switch (side) {

            case Side.Left:
                DisplayScore(score, new Vector3(-450f, -354f, 0f));
                break;

            case Side.Right:
                DisplayScore(score, new Vector3(450f, -354f, 0f));
                break;

            case Side.Center:
                DisplayScore(score, new Vector3(0f, -818f, 0f));
                break;

        }

    }


    private void HR_PlayerHandler_OnDestroyTrafficCar(HR_PlayerHandler player, int score, Side side) {

        switch (side)
        {

            case Side.Left:
                DisplayScore(score, new Vector3(-404f, -271f, 0f), true);
                break;

            case Side.Right:
                DisplayScore(score, new Vector3(404f, -271f, 0f), true);
                break;

            case Side.Center:
                DisplayScore(score, new Vector3(0f, -818f, 0f), true);
                break;

        }

    }

    public void DisplayScore(int score, Vector3 offset, bool destroy = false) {

        if (index < scoreTexts.Length - 1)
            index++;
        else
            index = 0;

        offset.y = 0;
        scoreTexts[index].GetComponentInChildren<Text>().text = (destroy == true ? "DESTROY " : "NEAR MISS ") + "+" + score.ToString();
        scoreTexts[index].transform.position = new Vector3(defPos.x , (Screen.height / Random.Range(1.5f, 2f)), defPos.z) + offset;
        timer = lifeTime;
        if (!destroy)
        {
            nearMissSound = HR_CreateAudioSource.NewAudioSource(gameObject, HR_HighwayRacerProperties.Instance.nearMissAudioClip.name, 0f, 0f, 1f, HR_HighwayRacerProperties.Instance.nearMissAudioClip, false, true, true);
            nearMissSound.ignoreListenerPause = true;
        }
        else
        {
            destroySound = HR_CreateAudioSource.NewAudioSource(gameObject, "Destroy Car Sound", 0f, 0f, 1f, SoundCarManager.Instance.destroySound, false, true, true);
            destroySound.ignoreListenerPause = true;
        }

    }
    private void Update() {

        if (timer > 0)
            timer -= Time.deltaTime;

        timer = Mathf.Clamp(timer, 0f, lifeTime);

        for (int i = 0; i < scoreTexts.Length; i++) {
            //			scoreTexts [i].transform.Translate (Vector3.up * Time.deltaTime * 75f, Space.World);
            scoreTexts[i].GetComponentInChildren<Image>().color = Color.Lerp(scoreTexts[i].GetComponentInChildren<Image>().color, new Color(scoreTexts[i].GetComponentInChildren<Image>().color.r, scoreTexts[i].GetComponentInChildren<Image>().color.g, scoreTexts[i].GetComponentInChildren<Image>().color.b, 0f), Time.deltaTime * 10f);
            scoreTexts[i].GetComponentInChildren<Text>().color = Color.Lerp(scoreTexts[i].GetComponentInChildren<Text>().color, new Color(scoreTexts[i].GetComponentInChildren<Text>().color.r, scoreTexts[i].GetComponentInChildren<Text>().color.g, scoreTexts[i].GetComponentInChildren<Text>().color.b, 0f), Time.deltaTime * 10f);
        }

        if (timer > 0) {

            scoreTexts[index].GetComponentInChildren<Image>().color = new Color(scoreTexts[index].GetComponentInChildren<Image>().color.r, scoreTexts[index].GetComponentInChildren<Image>().color.g, scoreTexts[index].GetComponentInChildren<Image>().color.b, 1f);
            scoreTexts[index].GetComponentInChildren<Text>().color = new Color(scoreTexts[index].GetComponentInChildren<Text>().color.r, scoreTexts[index].GetComponentInChildren<Text>().color.g, scoreTexts[index].GetComponentInChildren<Text>().color.b, 1f);

        }

    }

    private void OnDisable() {

        HR_PlayerHandler.OnNearMiss -= HR_PlayerHandler_OnNearMiss;
        HR_PlayerHandler.OnDestroyTrafficCar -= HR_PlayerHandler_OnDestroyTrafficCar;
    }

}
