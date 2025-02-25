//----------------------------------------------
//           	   Highway Racer
//
// Copyright © 2014 - 2023 BoneCracker Games
// http://www.bonecrackergames.com
//
//----------------------------------------------

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;


[AddComponentMenu("BoneCracker Games/Highway Racer/UI/HR UI Gameplay Panel")]
public class HR_UIGameplayPanel : MonoBehaviour {

    private HR_PlayerHandler player;
    public GameObject content;

    public Text score;
    public Text timeLeft;

    public Text speed;
    public Text distance;
    public Text highSpeed;

    public RectTransform speedPointer;

    public Slider damageSlider;
    public Slider bombSlider;

    private Image highSpeedImage;
    private Vector2 highSpeedDefPos;

    private Image timeAttackImage;

    private RectTransform bombRect;
    private Vector2 bombDefPos;

    private GameObject multiplayerPanel;

    public GameObject loadingAI;

    private void Awake() {

        if (highSpeed) {

            highSpeedImage = highSpeed.GetComponentInParent<Image>();
            highSpeedDefPos = highSpeedImage.rectTransform.anchoredPosition;

        }

        if (timeLeft)
            timeAttackImage = timeLeft.GetComponentInParent<Image>();

        if (bombSlider) {

            bombRect = bombSlider.GetComponent<RectTransform>();
            bombDefPos = bombRect.anchoredPosition;

        }

#if PHOTON_UNITY_NETWORKING && BCG_HR_PHOTON

        HR_UI_GameplayPhoton multiplayerPanelScript = GetComponentInChildren<HR_UI_GameplayPhoton>(true);

        if (multiplayerPanelScript)
            multiplayerPanel = multiplayerPanelScript.gameObject;

#endif

    }

    private void OnEnable() {

        HR_GamePlayHandler.OnPlayerSpawned += HR_PlayerHandler_OnPlayerSpawned;
        HR_GamePlayHandler.OnPlayerDied += HR_PlayerHandler_OnPlayerDied;
        HR_GamePlayHandler.OnPlayerRevive += HR_PlayerHandler_OnPlayerSpawned;
    }

    private void Start() {

        if (multiplayerPanel) {

            if (PlayerPrefs.GetInt("Multiplayer", 0) == 1)
                multiplayerPanel.SetActive(true);
            else
                multiplayerPanel.SetActive(false);

        }

    }

    private void HR_PlayerHandler_OnPlayerSpawned(HR_PlayerHandler _player) {

        player = _player;
        content.SetActive(true);

    }

    private void HR_PlayerHandler_OnPlayerDied(HR_PlayerHandler _player, int[] scores) {

        if(HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.AIRacing)
        {
            StartCoroutine(DelayContentAtDie());
        }
        else
        {
            player = null;
            content.SetActive(false);

        }

    }
    IEnumerator DelayContentAtDie()
    {
        content.SetActive(false);
        yield return new WaitForSeconds(2f);
        content.SetActive(true);

    }

    private void Update() {

        if (!player)
            return;

        if (highSpeed) {

            if (player.highSpeedCurrent > .1f)
                highSpeedImage.rectTransform.anchoredPosition = Vector2.Lerp(highSpeedImage.rectTransform.anchoredPosition, highSpeedDefPos, Time.deltaTime * 5f);
            else
                highSpeedImage.rectTransform.anchoredPosition = Vector2.Lerp(highSpeedImage.rectTransform.anchoredPosition, new Vector2(highSpeedDefPos.x + 500, highSpeedDefPos.y), Time.deltaTime * 5f);

        }

        if (timeLeft) {

            if (HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.TimeAttack
                || HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.MissionMode_1
            || HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.MissionMode_2) {

                if (!timeLeft.gameObject.activeSelf)
                    timeAttackImage.gameObject.SetActive(true);

            } else {

                if (timeLeft.gameObject.activeSelf)
                    timeAttackImage.gameObject.SetActive(false);

            }

        }

        if (damageSlider) {

            damageSlider.value = player.damage;

        }

        if (bombSlider) {

            if (HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.Bomb) {

                if (!bombSlider.gameObject.activeSelf)
                    bombSlider.gameObject.SetActive(true);

            } else {

                if (bombSlider.gameObject.activeSelf)
                    bombSlider.gameObject.SetActive(false);

            }

            if (player.bombTriggered)
                bombRect.anchoredPosition = Vector2.Lerp(bombRect.anchoredPosition, bombDefPos, Time.deltaTime * 5f);
            else
                bombRect.anchoredPosition = Vector2.Lerp(bombRect.anchoredPosition, new Vector2(bombDefPos.x - 500, bombDefPos.y), Time.deltaTime * 5f);

        }

    }

    private void LateUpdate() {

        if (!player)
            return;

        if (score)
            score.text = player.score.ToString("F0");

        if (speed)
        {
            speedPointer.rotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(170f, -40f, player.speed / player.maxSpeed));
            speed.text = player.speed.ToString("F0");
        }

        if (distance)
        {
            if(HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.MissionMode_1 || HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.MissionMode_2)
                distance.text = (player.distanceMMText).ToString("F2") + " KM";
            else
                distance.text = (player.distance).ToString("F2") + " KM";
        }

        if (highSpeed)
            highSpeed.text = player.highSpeedCurrent.ToString("F1");

        if (timeLeft)
            timeLeft.text = player.showTimeLeft(true);

        if (bombSlider && HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.Bomb)
            bombSlider.value = player.bombHealth / 100f;
    }

    private void OnDisable() {

        HR_GamePlayHandler.OnPlayerSpawned -= HR_PlayerHandler_OnPlayerSpawned;
        HR_GamePlayHandler.OnPlayerDied -= HR_PlayerHandler_OnPlayerDied;
        HR_GamePlayHandler.OnPlayerRevive -= HR_PlayerHandler_OnPlayerSpawned;
    }

}
