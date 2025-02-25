//----------------------------------------------
//           	   Highway Racer
//
// Copyright © 2014 - 2023 BoneCracker Games
// http://www.bonecrackergames.com
//
//----------------------------------------------

using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[AddComponentMenu("BoneCracker Games/Highway Racer/UI/HR UI Gameplay Options Handler")]
public class HR_UIOptionsHandler : MonoBehaviour {

    #region SINGLETON PATTERN
    private static HR_UIOptionsHandler instance;
    public static HR_UIOptionsHandler Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<HR_UIOptionsHandler>();
            }

            return instance;
        }
    }
    #endregion

    public GameObject pausedMenu;

    public GameObject resumeButton;
    public GameObject homeButton;

    void Start()
    {
        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        SceneManager.sceneUnloaded += SceneManager_sceneUnloaded;
    }

    private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        Hide();
        HR_GamePlayHandler.OnPaused += OnPaused;
        HR_GamePlayHandler.OnResumed += OnResumed;
    }

    private void SceneManager_sceneUnloaded(Scene arg0)
    {
        HR_GamePlayHandler.OnPaused -= OnPaused;
        HR_GamePlayHandler.OnResumed -= OnResumed;
    }

    public void ResumeGame() {

        HR_GamePlayHandler.Instance.Paused();

    }

    public void RestartGame() {

        Hide();
        HR_GamePlayHandler.Instance.RestartGame();

    }

    public void MainMenu() {

        Hide();
        HR_GamePlayHandler.Instance.MainMenu();

    }

    private void OnPaused() {

        Show();

        if (PlayerPrefs.GetInt("Multiplayer", 0) == 1)
            return;

        AudioListener.pause = true;
        Time.timeScale = 0;

    }

    public void OnResumed() {

        Hide();

        if (PlayerPrefs.GetInt("Multiplayer", 0) == 1)
            return;

        AudioListener.pause = false;
        Time.timeScale = 1;

    }

    public void ChangeCamera() {

        if (FindObjectOfType<HR_CarCamera>())
            FindObjectOfType<HR_CarCamera>().ChangeCamera();

    }

    private void SetupButtons()
    {
        switch (SceneManager.GetActiveScene().buildIndex)
        {
            case 1:
                resumeButton.gameObject.SetActive(false);
                homeButton.gameObject.SetActive(true);
                homeButton.GetComponent<Button>().onClick.AddListener(() => Hide());
                homeButton.GetComponent<Button>().onClick.AddListener(() => HPS_MainMenuHandler.Instance.EnableMenu(HPS_MainMenuHandler.Instance.statMenu));
                break;
            default:
                resumeButton.gameObject.SetActive(true);
                resumeButton.GetComponent<Button>().onClick.AddListener(() => ResumeGame());
                homeButton.gameObject.SetActive(true);
                homeButton.GetComponent<Button>().onClick.AddListener(() => MainMenu());
                homeButton.GetComponent<Button>().onClick.AddListener(() => HR_GamePlayHandler.Instance.player.CarController.KillOrStartEngine());
                break;
        }
    }

    public void Show()
    {
        pausedMenu.SetActive(true);
        SetupButtons();
    }

    public void Hide()
    {
        pausedMenu.SetActive(false);
        resumeButton.GetComponent<Button>().onClick.RemoveAllListeners();
        homeButton.GetComponent<Button>().onClick.RemoveAllListeners();
    }

}
