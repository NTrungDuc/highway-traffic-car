//----------------------------------------------
//           	   Highway Racer
//
// Copyright © 2014 - 2023 BoneCracker Games
// http://www.bonecrackergames.com
//
//----------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using UnityEditor;
using System.Net.Sockets;
using GoogleMobileAds.Sample;
#if PHOTON_UNITY_NETWORKING
using Photon;
using Photon.Pun;
using Photon.Realtime;
#endif

/// <summary>
/// Gameplay management. Spawns player vehicle, sets volume, set mods, listens player events.
/// </summary>
[AddComponentMenu("BoneCracker Games/Highway Racer/Gameplay/HR Gameplay Handler")]
public class HR_GamePlayHandler : MonoBehaviour {

    #region SINGLETON PATTERN
    private static HR_GamePlayHandler instance;
    public static HR_GamePlayHandler Instance {
        get {
            //if (instance == null) {
            //    instance = FindObjectOfType<HR_GamePlayHandler>();
            //}

            return instance;
        }
    }
    #endregion

    [Header("Time Of The Scene")]
    public DayOrNight dayOrNight = DayOrNight.Day;
    public enum DayOrNight { Day, Night }

    [Header("Current Mode")]
    public Mode mode = Mode.OneWay;
    public enum Mode { OneWay, TwoWay, TimeAttack, Bomb, MissionMode_1, MissionMode_2, AIRacing }
    public MeshRenderer roadRenderer;
    public Material[] roadMaterials;

    [Header("Spawn Location Of The Cars")]
    public Transform spawnLocation;

    public HR_PlayerHandler player;
    public HR_PlayerHandler player2;

    public float overtakingDistance = 0f;
    public float maxDistance = 0.5f;

    private int selectedCarIndex = 0;
    private int selectedModeIndex = 0;

    public bool gameStarted = false;
    private bool paused = false;
    private readonly float minimumSpeed = 20f;

    public delegate void onCountDownStarted();
    public static event onCountDownStarted OnCountDownStarted;

    public delegate void onRaceStarted();
    public static event onRaceStarted OnRaceStarted;

    public delegate void onPlayerSpawned(HR_PlayerHandler player);
    public static event onPlayerSpawned OnPlayerSpawned;

    public delegate void onPlayerDied(HR_PlayerHandler player, int[] scores);
    public static event onPlayerDied OnPlayerDied;

    public delegate void onPlayerWon(HR_PlayerHandler player, int[] scores);
    public static event onPlayerWon OnPlayerWon;

    public delegate void onPlayerFailed(HR_PlayerHandler player, int[] scores);
    public static event onPlayerFailed OnPlayerFailed;
    public static event onPlayerFailed OnPlayerFailedTime;

    public delegate void onPlayerRevive(HR_PlayerHandler player);
    public static event onPlayerRevive OnPlayerRevive;

    public delegate void onPaused();
    public static event onPaused OnPaused;

    public delegate void onResumed();
    public static event onResumed OnResumed;

    public delegate void onPauseModeDual();
    public static event onPauseModeDual OnPauseModeDual;

    public delegate void onGetCoin();
    public static event onGetCoin OnGetCoin;

    public delegate void onGetDiamond();
    public static event onGetDiamond OnGetDiamond;

    public delegate void onGetTimeBonus();
    public static event onGetTimeBonus OnGetTimeBonus;
    private AudioSource gameplaySoundtrack;
    // dev viet
    public Transform AITranform;
    [HideInInspector] public bool isCheckTimeAI;
    [HideInInspector] public float distancePlayerBetweenAI;
    private GameObject carAIGameObject;
    [HideInInspector] public HR_AICarNew carAIGameHR_AICar;
    public GameObject AINew;
    [HideInInspector] public bool isCheckNitro;
    [HideInInspector] public bool isCheckAIBefore;
    [HideInInspector] public bool isCheckAIAfter;
    private string nameScene;

    private void Awake() {

        if (instance == null)
        {
            instance = this;
        }
        //  Make sure time scale is 1. We are setting volume to 0, we'll be increase it smoothly in the update method.
        Time.timeScale = 1f;
        AudioListener.volume = 0f;
        AudioListener.pause = false;
        gameStarted = false;
        //  Creating soundtrack.
        if (HR_HighwayRacerProperties.Instance.gameplayClips != null && HR_HighwayRacerProperties.Instance.gameplayClips.Length > 0) {

            gameplaySoundtrack = HR_CreateAudioSource.NewAudioSource(gameObject, "GamePlay Soundtrack", 0f, 0f, .35f, HR_HighwayRacerProperties.Instance.gameplayClips[UnityEngine.Random.Range(0, HR_HighwayRacerProperties.Instance.gameplayClips.Length)], true, true, false);
            gameplaySoundtrack.volume = PlayerPrefs.GetFloat("MusicVolume", .35f);
            gameplaySoundtrack.ignoreListenerPause = true;

        }
        
        //  Getting selected player car index and mode index.
        selectedCarIndex = PlayerPrefs.GetInt("SelectedPlayerCarIndex");
        selectedModeIndex = PlayerPrefs.GetInt("SelectedModeIndex");

        //  Setting proper mode.
        switch (selectedModeIndex) {

            case 0:
                mode = Mode.OneWay;
                break;
            case 1:
                mode = Mode.TwoWay;
                break;
            case 2:
                mode = Mode.TimeAttack;
                break;
            case 3:
                mode = Mode.Bomb;
                break; 
	        case 4:          
                mode = Mode.MissionMode_1;
                break;
            case 5:
                mode = Mode.MissionMode_2;
                break;
            case 6:
                mode = Mode.AIRacing;
                break;
        }

        ChangeRoad();

#if PHOTON_UNITY_NETWORKING && BCG_HR_PHOTON

        if (PlayerPrefs.GetInt("Multiplayer", 0) == 1) {

            if (!PhotonNetwork.IsConnectedAndReady || !PhotonNetwork.InRoom) {

                Debug.LogError("Not connected to the server, or not in a room yet. Returning to the main menu...");
                SceneManager.LoadScene(0);
                return;

            }

            HR_FixFloatingOrigin fixFloatingOrigin = FindObjectOfType<HR_FixFloatingOrigin>();

            if (fixFloatingOrigin)
                Destroy(fixFloatingOrigin);

        }

#endif

    }

    private void Start() {

        SpawnCar();     //  Spawning the player vehicle.
        SpawnCarAI();

        if (PlayerPrefs.GetInt("Multiplayer", 0) == 0)
            StartCoroutine(StartRaceDelayed());

        //StartCoroutine(PrewarmBeforePlay());
        
    }

    private void Update() {
        //	If loading, set value of the loading slider.
        //if(Input.GetKeyDown(KeyCode.Space))
        //    player.GetComponent<Rigidbody>().velocity = new Vector3(0f, 0f, 0);

        //  Adjusting volume smoothly.
        float targetVolume = 1f;

        if (AudioListener.volume < targetVolume && !paused && Time.timeSinceLevelLoad > .5f) {

            if (AudioListener.volume < targetVolume) {

                targetVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
                AudioListener.volume = Mathf.MoveTowards(AudioListener.volume, targetVolume, Time.deltaTime);

            }

        }

        if(mode == Mode.AIRacing)
        {
            distancePlayerBetweenAI = Vector3.Distance(player.transform.position, carAIGameObject.transform.position);
        }
#if PHOTON_UNITY_NETWORKING && BCG_HR_PHOTON
        if (PlayerPrefs.GetInt("Multiplayer", 0) == 1)
            CheckNetworkPlayers();
#endif

    }

    /// <summary>
    /// Change the road according to the selected mode.
    /// </summary>
    private void ChangeRoad()
    {
        switch (mode)
        {
            case Mode.OneWay:
            case Mode.MissionMode_1:
            case Mode.AIRacing when PlayerPrefs.GetInt("numberPolling", 0) == 0:
                roadRenderer.material = roadMaterials[0];
                break;
            case Mode.TwoWay:
            case Mode.MissionMode_2:
            case Mode.AIRacing when PlayerPrefs.GetInt("numberPolling", 0) == 1:
                roadRenderer.material = roadMaterials[1];
                break;
        }
    }

    /// <summary>
    /// Spawning player car.
    /// </summary>
    private void SpawnCar() {

        bool isMultiplayer = PlayerPrefs.GetInt("Multiplayer", 0) == 0 ? false : true;
        if (!isMultiplayer) {

            //player = (RCC.SpawnRCC(HR_PlayerCars.Instance.cars[selectedCarIndex].playerCar.GetComponent<RCC_CarControllerV3>(), spawnLocation.position, spawnLocation.rotation, true, false, true)).GetComponent<HR_PlayerHandler>();
            player = (RCC.SpawnRCC(((GameObject)Resources.Load("Player Vehicles/" + HPS_CarDatas.Instance.cardatas[selectedCarIndex].carName, typeof(GameObject))).GetComponent<RCC_CarControllerV3>(), spawnLocation.position, spawnLocation.rotation, true, false, true)).GetComponent<HR_PlayerHandler>();
            BannerViewController.Instance.ShowAd();
            player.canCrash = true;
            player.Rigid.isKinematic = false;
            player.Rigid.velocity = new Vector3(0f, 0f, minimumSpeed / 1.75f);
            player.CarController.canNitro = true;
            RCC_Customization.LoadStats(player.CarController);
            StartCoroutine(CheckDayTime());

        } else {

#if PHOTON_UNITY_NETWORKING && BCG_HR_PHOTON

            if (PhotonNetwork.LocalPlayer.ActorNumber == 1)
                spawnLocation.transform.position += Vector3.right * 1.25f;
            else
                spawnLocation.transform.position -= Vector3.right * 1.25f;

            player = PhotonNetwork.Instantiate("Photon Player Vehicles/" + HR_PlayerCars.Instance.cars[selectedCarIndex].playerCar.name, spawnLocation.position, spawnLocation.rotation).GetComponent<HR_PlayerHandler>();
            player.canCrash = false;
            RCC.RegisterPlayerVehicle(player.GetComponent<RCC_CarControllerV3>(), false, true);
            player.GetComponent<Rigidbody>().velocity = new Vector3(0f, 0f, minimumSpeed / 1.75f);
            RCC_Customization.LoadStats(player.GetComponent<RCC_CarControllerV3>());
            StartCoroutine(CheckDayTime());
            HR_PhotonHandler.Instance.NetworkPlayerSpawned(player.gameObject.GetPhotonView());

#endif

        }

        if (OnPlayerSpawned != null)
            OnPlayerSpawned(player);
    }

    private IEnumerator PrewarmBeforePlay()
    {
        yield return new WaitUntil(() => !HPS_UILoading.Instance.isLoading);

        BannerViewController.Instance.ShowAd();
        player.canCrash = true;
        player.Rigid.isKinematic = false;
        player.Rigid.velocity = new Vector3(0f, 0f, minimumSpeed / 1.75f);
        player.CarController.canNitro = true;
        RCC_Customization.LoadStats(player.CarController);
        StartCoroutine(CheckDayTime());

        if (OnPlayerSpawned != null)
            OnPlayerSpawned(player);

        StartCoroutine(StartRaceDelayed());
    }

    private void SpawnCarAI()
    {
        if (mode == Mode.AIRacing)
        {
            GameObject gameObject = Instantiate(AINew, AITranform.position, Quaternion.identity);
            carAIGameObject = gameObject;
            carAIGameHR_AICar = gameObject.GetComponent<HR_AICarNew>();
            gameObject.GetComponent<RCC_CarControllerV3>().canNitro = true;
            carAIGameHR_AICar.canCrash = true;
            carAIGameHR_AICar.GetComponent<Rigidbody>().velocity = new Vector3(0f, 0f, minimumSpeed / 1.75f);
        }
    }

    /// <summary>
    /// Countdown before the game.
    /// </summary>
    /// <returns></returns>
    public IEnumerator StartRaceDelayed()
    {

        if (OnCountDownStarted != null)
            OnCountDownStarted();

        yield return new WaitForSeconds(4);

        gameStarted = true;
        RCC.SetControl(player.GetComponent<RCC_CarControllerV3>(), true);

        if (OnRaceStarted != null)
            OnRaceStarted();

    }

    /// <summary>
    /// Checking time.
    /// </summary>
    /// <returns></returns>
    private IEnumerator CheckDayTime() {

        yield return new WaitForFixedUpdate();

        if (dayOrNight == DayOrNight.Night)
            player.GetComponent<RCC_CarControllerV3>().lowBeamHeadLightsOn = true;
        else
            player.GetComponent<RCC_CarControllerV3>().lowBeamHeadLightsOn = false;

    }

    /// <summary>
    /// When player crashed.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="scores"></param>
    public void CrashedPlayerModDual()
    {
        gameStarted = false;
        if (OnPauseModeDual != null)
            OnPauseModeDual();
    }
    public void CrashedPlayer(HR_PlayerHandler player, int[] scores) {

        gameStarted = false;
       
            if (OnPlayerDied != null)
                OnPlayerDied(player, scores);        

        StartCoroutine(FinishRaceDelayed(1f));

    }
    public void CrashedPlayer(HR_AICarNew AI, int[] scores) {

        //gameStarted = false;
        //StartCoroutine(FinishRaceDelayed(1f));

    }
    public void PLayerFailed(HR_PlayerHandler playerHandler, int[] scores)
    {
        gameStarted = false;

        if(OnPlayerFailed != null)
            OnPlayerFailed(playerHandler, scores);
    }
    public void PLayerFailed(HR_AICarNew AIHandler, int[] scores)
    {
        //gameStarted = false;

        //if(OnPlayerFailed != null)
        //    OnPlayerFailed(AIHandler, scores);
    }
    public void PLayerFailedTime(HR_PlayerHandler playerHandler, int[] scores)
    {
        gameStarted = false;

        if (OnPlayerFailedTime != null)
            OnPlayerFailedTime(playerHandler, scores);
    }
    public void PLayerFailedTime(HR_AICarNew AIHandler, int[] scores)
    {
        //gameStarted = false;

        //if (OnPlayerFailedTime != null)
        //    OnPlayerFailedTime(playerHandler, scores);
    }
    public void PLayerRevive(HR_PlayerHandler playerHandler)
    {
        gameStarted = true;

        if (OnPlayerRevive != null)
            OnPlayerRevive(playerHandler);
    }
    public void PLayerRevive(HR_AICarNew AIHandler)
    {
        //gameStarted = true;

        //if (OnPlayerRevive != null)
        //    OnPlayerRevive(playerHandler);
    }
    /// <summary>
    /// Finished the game after the crash and saves the highscore.
    /// </summary>
    /// <param name="delayTime"></param>
    /// <returns></returns>
    public IEnumerator FinishRaceDelayed(float delayTime) {

        yield return new WaitForSecondsRealtime(delayTime);
        FinishRace();

    }
    public void PlayerWon(HR_PlayerHandler player, int[] scores)
    {

        gameStarted = false;

        if (OnPlayerWon != null)
            OnPlayerWon(player, scores);

        StartCoroutine(FinishRaceWon(1f));

    }
    public void PlayerWon(HR_AICarNew AI, int[] scores)
    {

        //gameStarted = false;

        //if (OnPlayerWon != null)
        //    OnPlayerWon(player, scores);

        //StartCoroutine(FinishRaceWon(1f));

    }
    public IEnumerator FinishRaceWon(float delayTime)
    {

        yield return new WaitForSecondsRealtime(delayTime);
        GameWin();

    }
    public void GameWin()
    {
        switch (mode)
        {

            case Mode.MissionMode_1:
                PlayerPrefs.SetInt("levelMissionMode", (int)player.level);
                break;
            case Mode.MissionMode_2:
                PlayerPrefs.SetInt("levelMissionMode", (int)player.level);
                break;
        }
    }
    /// <summary>
    /// Finishes the game after the crash and saves the highscore instantly.
    /// </summary>
    /// <param name="delayTime"></param>
    /// <returns></returns>
    public void FinishRace() {

        switch (mode) {

            case Mode.OneWay:
                PlayerPrefs.SetInt("bestScoreOneWay", (int)player.score); 
                break;
            case Mode.TwoWay:
                PlayerPrefs.SetInt("bestScoreTwoWay", (int)player.score);
                break;
            case Mode.TimeAttack:
                PlayerPrefs.SetInt("bestScoreTimeAttack", (int)player.score);
                break;
            case Mode.Bomb:
                PlayerPrefs.SetInt("bestScoreBomb", (int)player.score);
                break;
        }

    }

    /// <summary>
    /// Main menu.
    /// </summary>
    public void MainMenu() {

        if (PlayerPrefs.GetInt("Multiplayer", 0) == 0) 
        {
            player.CarController.Rigid.isKinematic = true;
            StartCoroutine(HPS_UILoading.Instance.LoadScene(1));
            BannerViewController.Instance.HideAd();
        } else {

#if PHOTON_UNITY_NETWORKING && BCG_HR_PHOTON
            PhotonNetwork.LeaveRoom();
#endif

        }

    }

    /// <summary>
    /// Restart the game.
    /// </summary>
    public void RestartGame() {

        if (PlayerPrefs.GetInt("Multiplayer", 0) == 1)
            return;

        player.CarController.Rigid.isKinematic = true;

        if (mode == Mode.MissionMode_1 || mode == Mode.MissionMode_2)
        {
            int selectMode = PlayerPrefs.GetInt("levelMissionMode", 1) - 1;
            PlayerPrefs.SetInt("SelectedModeIndex", DataSelectMapMissionMode.Instance.dataMisionMode.selectModes[selectMode].mode);
            StartCoroutine(HPS_UILoading.Instance.LoadScene(DataSelectMapMissionMode.Instance.dataMisionMode.selectModes[selectMode].nameMap));
        }
        else
            StartCoroutine(HPS_UILoading.Instance.LoadScene(SceneManager.GetActiveScene().name));
    }

    public void next()
    {
        var numberScene = UnityEngine.Random.Range(0, 3);
        switch (numberScene)
        {
            case 0:
                nameScene = "HighwaySunny";
                break;
            case 1:
                nameScene = "HighwayNight";
                break;
            case 2:
                nameScene = "HighwayRainy";
                break;
            case 3:
                nameScene = "HighwayAfternoon";
                break;
        }
        var number = UnityEngine.Random.Range(0, 10);
        int numberPooling;
        if (number % 2 == 0)
            numberPooling = 0;
        else
            numberPooling = 1;
        PlayerPrefs.SetInt("SelectedModeIndex", 6);
        PlayerPrefs.SetInt("numberPooling", numberPooling);
        StartCoroutine(HPS_UILoading.Instance.LoadScene(nameScene));
    }
    /// <summary>
    /// Pause or resume the game.
    /// </summary>
    public void Paused() {

        paused = !paused;

        if (paused)
        {
            if (HR_GamePlayHandler.instance.mode == Mode.MissionMode_1 || HR_GamePlayHandler.instance.mode == Mode.MissionMode_2)
                ProgressMissionMode.Instance.disableProgressBar();
            else if (HR_GamePlayHandler.instance.mode == Mode.AIRacing)
                ShowLocation.Instance.activeProgressDualAI(false);
            OnPaused();
        }
        else
        {
            if(HR_GamePlayHandler.instance.mode==Mode.MissionMode_1 || HR_GamePlayHandler.instance.mode == Mode.MissionMode_2)
                ProgressMissionMode.Instance.content.SetActive(true);
            else if(HR_GamePlayHandler.instance.mode == Mode.AIRacing)
                ShowLocation.Instance.activeProgressDualAI(true);
            OnResumed();
        }

    }
    public void GetItems(int index)
    {
        switch (index)
        {
            case 0:
                OnGetCoin();
                break;
            case 1:
                OnGetDiamond(); 
                break;
            case 2:
                OnGetTimeBonus();
                break;
        }
    }
}
