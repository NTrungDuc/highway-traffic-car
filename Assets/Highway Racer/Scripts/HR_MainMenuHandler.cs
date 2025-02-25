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
using System;
using UnityEngine.SceneManagement;
using UnityEditor;

/// <summary>
/// Management of the main menu events. Creates and spawns vehicles, switches them, enables/disables menus.
/// </summary>
[AddComponentMenu("BoneCracker Games/Highway Racer/Main Menu/HR Main Menu Handler")]
public class HR_MainMenuHandler : MonoBehaviour {

    #region SINGLETON PATTERN
    private static HR_MainMenuHandler instance;
    public static HR_MainMenuHandler Instance {
        get {
            if (instance == null) {
                instance = FindObjectOfType<HR_MainMenuHandler>();
            }

            return instance;
        }
    }
    #endregion

    [Header("Spawn Location Of The Cars")]
    public Transform carSpawnLocation;      //  Spawn location.

    private GameObject[] createdCars;       //	All created cars will be stored.
    public RCC_CarControllerV3 currentCar;      //	Current selected car.
    public HR_ModApplier currentApplier;        //  Current mod applier of the selected car.

    internal int carIndex = 0;      //	Current car index.
    [Header("UI Menus")]
    public GameObject optionsMenu;
    public GameObject carSelectionMenu;
    public GameObject modsSelectionMenu;
    public GameObject sceneSelectionMenu;
    public GameObject creditsMenu;

    [Header("UI Loading Section")]
    public GameObject loadingScreen;
    public Slider loadingBar;
    private AsyncOperation async;

    [Header("Buttons")]
    public GameObject buyCarButton;
    public GameObject selectCarButton;
    public GameObject modCarPanel;
    public GameObject missionModeSelectWay;
    public GameObject endlessModeSelectWay;

    [Header("Texts")]
    public Text currency;

    [Header("Best Score Texts")]
    public Text vehicleNameText;        //	Current vehicle name text.
    public Text bestScoreOneWay;        //	Best score one way text.
    public Text bestScoreTwoWay;        //	Best score two ways text.
    public Text bestScoreTimeLeft;      //	Best score time left text.
    public Text bestScoreBomb;      //	Best score bomb text.
    public Text levelMissionMode;

    private string nameScene;

    internal AudioSource mainMenuSoundtrack;
    // dev viet

    public GameObject laodingMode;
    public Sprite[] imageCarPlay;
    public Image imageUI;
    public Sprite[] imageCarAI;
    public Image imageUIAI;

#if PHOTON_UNITY_NETWORKING && BCG_HR_PHOTON
    private GameObject multiplayerMenu;
#endif

    private void Awake() {

        HPS_API.Save("Resources", "HPS_CarDatas");

        // Setting time scale, volume, unpause, and target frame rate.
        Time.timeScale = 1f;
        AudioListener.volume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        AudioListener.pause = false;

        //	Creating soundtracks for the main menu.
        if (HR_HighwayRacerProperties.Instance.mainMenuClips != null && HR_HighwayRacerProperties.Instance.mainMenuClips.Length > 0) {

            mainMenuSoundtrack = HR_CreateAudioSource.NewAudioSource(gameObject, "Main Menu Soundtrack", 0f, 0f, PlayerPrefs.GetFloat("MusicVolume", .35f), HR_HighwayRacerProperties.Instance.mainMenuClips[UnityEngine.Random.Range(0, HR_HighwayRacerProperties.Instance.mainMenuClips.Length)], true, true, false);
            mainMenuSoundtrack.ignoreListenerPause = true;

        }

        //	If test mode enabled, add 1000000 coins to the balance.
        if (HR_HighwayRacerProperties.Instance._1MMoneyForTesting)
        {
            PlayerPrefs.SetInt("CurrencyGold", 1000000);
            PlayerPrefs.SetInt("CurrencyDiamond", 10000);
        }

        //	Getting last selected car index.
        carIndex = PlayerPrefs.GetInt("SelectedPlayerCarIndex", 0);

        CreateCars();   //	Creating all selectable cars at once.
        SpawnCar();     //	Spawning only target car (carIndex).
        EnableMenu(carSelectionMenu);       //  Enabling car selection menu.

#if PHOTON_UNITY_NETWORKING && BCG_HR_PHOTON

        HR_PhotonLobbyManager multiplayerManager = FindObjectOfType<HR_PhotonLobbyManager>(true);

        if (multiplayerManager)
            multiplayerMenu = multiplayerManager.gameObject;

#endif

    }

    private void Update() {

        //	Displaying currency.
        currency.text = HR_API.GetCurrencyGold().ToString("F0");

        //	If loading, set value of the loading slider.
        if (async != null && !async.isDone)
            loadingBar.value = async.progress;

    }

    /// <summary>
    /// Creating all spawnable cars at once.
    /// </summary>
    private void CreateCars() {

        //	Creating a new array.
        //createdCars = new GameObject[HR_PlayerCars.Instance.cars.Length];
        createdCars = new GameObject[HPS_CarDatas.Instance.cardatas.Length];

        //	Setting array elements.
        for (int i = 0; i < createdCars.Length; i++) {

            //createdCars[i] = (RCC.SpawnRCC(HR_PlayerCars.Instance.cars[i].playerCar.GetComponent<RCC_CarControllerV3>(), carSpawnLocation.position, carSpawnLocation.rotation, false, false, false)).gameObject;
            createdCars[i] = (RCC.SpawnRCC(((GameObject)Resources.Load("Player Vehicles/" + HPS_CarDatas.Instance.cardatas[i].carName, typeof(GameObject))).GetComponent<RCC_CarControllerV3>(), carSpawnLocation.position, carSpawnLocation.rotation, false, false, false)).gameObject;
            createdCars[i].GetComponent<RCC_CarControllerV3>().lowBeamHeadLightsOn = true;
            createdCars[i].SetActive(false);

        }

    }

    /// <summary>
    /// Spawns target car (carIndex).
    /// </summary>
    private void SpawnCar() {

        //	If price of the car is 0, or unlocked, save it as owned car.
        //if (HR_PlayerCars.Instance.cars[carIndex].price <= 0 || HR_PlayerCars.Instance.cars[carIndex].unlocked)
        //    HR_API.UnlockVehice(carIndex);

        if (HPS_CarDatas.Instance.cardatas[carIndex].price <= 0 || HPS_CarDatas.Instance.cardatas[carIndex].unlocked)
            HPS_API.UnlockVehice(carIndex);

        //	If current spawned car is owned, enable buy button, disable select button. Do opposite otherwise.
        //if (HR_API.OwnedVehicle(carIndex)) {
        if (HPS_API.OwnedVehicle(carIndex)) {

            //  Displaying price null.
            if (buyCarButton.GetComponentInChildren<Text>())
                buyCarButton.GetComponentInChildren<Text>().text = "";

            // Enabling select button, disabling buy button.
            buyCarButton.SetActive(false);
            selectCarButton.SetActive(true);
            modCarPanel.SetActive(true);

        } else {

            //  Displaying price.
            if (buyCarButton.GetComponentInChildren<Text>())
            {
                //buyCarButton.GetComponentInChildren<Text>().text = "BUY FOR\n" + HR_PlayerCars.Instance.cars[carIndex].price.ToString("F0");
                buyCarButton.GetComponentInChildren<Text>().text = "BUY FOR\n" + HPS_CarDatas.Instance.cardatas[carIndex].price.ToString("F0");
            }

            //  Enabling buy button, disabling select button.
            selectCarButton.SetActive(false);
            buyCarButton.SetActive(true);
            modCarPanel.SetActive(false);

        }

        //	Disabling all cars at once. And then enabling only target car (carIndex). And make sure spawned cars are always at spawn point.
        for (int i = 0; i < createdCars.Length; i++) {

            if (createdCars[i].activeInHierarchy)
                createdCars[i].SetActive(false);

        }

        //	Enabling only target car (carIndex).
        createdCars[carIndex].SetActive(true);
        RCC.RegisterPlayerVehicle(createdCars[carIndex].GetComponent<RCC_CarControllerV3>(), false, false);

        //	Setting current car.
        currentCar = createdCars[carIndex].GetComponent<RCC_CarControllerV3>();
        currentApplier = currentCar.GetComponent<HR_ModApplier>();

        //	Displaying car name text.
        if (vehicleNameText)
        {
            //vehicleNameText.text = HR_PlayerCars.Instance.cars[carIndex].vehicleName;
            vehicleNameText.text = HPS_CarDatas.Instance.cardatas[carIndex].carName;
        }

        HR_ModHandler.Instance.ChooseClass(null);

    }

    /// <summary>
    /// Purchases current car.
    /// </summary>
    public void BuyCar() {

        // If we own the car, don't consume currency.
        //if (HR_API.OwnedVehicle(carIndex)) {
        if (HPS_API.OwnedVehicle(carIndex)) {

            Debug.LogError("Car is already owned!");
            return;

        }

        //	If currency is enough, save it and consume currency. Otherwise display the informer.
        //if (HR_API.GetCurrency() >= HR_PlayerCars.Instance.cars[carIndex].price) {
        if (HR_API.GetCurrencyGold() >= HPS_CarDatas.Instance.cardatas[carIndex].price) {

            //HR_API.ConsumeCurrency(HR_PlayerCars.Instance.cars[carIndex].price);
            HR_API.ConsumeCurrencyGold(HPS_CarDatas.Instance.cardatas[carIndex].price);
            HPS_CarDatas.Instance.cardatas[carIndex].unlocked = true;
            HPS_API.Save("Resources", "HPS_CarDatas");

        } else {

            //HR_UIInfoDisplayer.Instance.ShowInfo("Not Enough Coins", "You have to earn " + (HR_PlayerCars.Instance.cars[carIndex].price - HR_API.GetCurrency()).ToString() + " more coins to buy this vehicle", HR_UIInfoDisplayer.InfoType.NotEnoughMoney);
            HR_UIInfoDisplayer.Instance.ShowInfo("Not Enough Coins", "You have to earn " + (HPS_CarDatas.Instance.cardatas[carIndex].price - HR_API.GetCurrencyGold()).ToString() + " more coins to buy this vehicle", HR_UIInfoDisplayer.InfoType.NotEnoughMoney);
            return;

        }

        //	Saving the car.
        //HR_API.UnlockVehice(carIndex);
        HPS_API.UnlockVehice(carIndex);

        //	And spawning again to check modders of the car.
        SpawnCar();

    }

    /// <summary>
    /// Selects the current car with carIndex.
    /// </summary>
    public void SelectCar() {

        PlayerPrefs.SetInt("SelectedPlayerCarIndex", carIndex);

    }

    /// <summary>
    /// Switch to next car.
    /// </summary>
    public void PositiveCarIndex() {

        carIndex++;

        if (carIndex >= createdCars.Length)
            carIndex = 0;

        SpawnCar();

    }

    /// <summary>
    /// Switch to previous car.
    /// </summary>
    public void NegativeCarIndex() {

        carIndex--;

        if (carIndex < 0)
            carIndex = createdCars.Length - 1;

        SpawnCar();

    }

    /// <summary>
    /// Enables target menu and disables all other menus.
    /// </summary>
    /// <param name="activeMenu"></param>
    public void EnableMenu(GameObject activeMenu) {

        optionsMenu.SetActive(false);
        carSelectionMenu.SetActive(false);
        modsSelectionMenu.SetActive(false);
        sceneSelectionMenu.SetActive(false);
        creditsMenu.SetActive(false);
        loadingScreen.SetActive(false);
        missionModeSelectWay.SetActive(false);
        endlessModeSelectWay.SetActive(false);

        activeMenu.SetActive(true);

        if (activeMenu == modsSelectionMenu)
            BestScores();

    }

    /// <summary>
    /// Selects the scene with int.
    /// </summary>
    /// <param name="levelIndex"></param>
    public void SelectScene(string levelName) {

        if(HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.AIRacing && HR_GamePlayHandler.Instance != null)
        {
            laodingMode.SetActive(true);
            imageUI.sprite = imageCarPlay[PlayerPrefs.GetInt("SelectedPlayerCarIndex")];
            var number = UnityEngine.Random.Range(0, imageCarAI.Length);
            PlayerPrefs.SetInt("CarAINumber", number);
            imageUIAI.sprite = imageCarAI[number];
            StartCoroutine(delayMode(3, levelName));
        }
        else
        {
            StartCoroutine(delayMode(0, levelName));
        }

        //PlayerPrefs.SetString("SelectedScene", levelName);
        nameScene = levelName;



    }
    IEnumerator delayMode(float time, string levelName)
    {
        yield return new WaitForSeconds(time);
        PlayerPrefs.SetString("SelectedScene", levelName);
        laodingMode.SetActive(false);
    }

    /// <summary>
    /// Selects the mode with int.
    /// </summary>
    /// <param name="_modeIndex"></param>
    public void SelectMode(int _modeIndex) {

        //	Saving the selected mode, and enabling scene selection menu.
        PlayerPrefs.SetInt("SelectedModeIndex", _modeIndex);

    }

    /// <summary>
    /// Selects the scene with int.
    /// </summary>
    /// <param name="levelIndex"></param>
    public void StartRace() {
        if(nameScene == null)
        {
            return;
        }
        SelectCar();
        EnableMenu(loadingScreen);
        //async = SceneManager.LoadSceneAsync(PlayerPrefs.GetString("SelectedScene", ""));
        async = SceneManager.LoadSceneAsync(nameScene);

    }

    /// <summary>
    /// Displays best scores of all four modes.
    /// </summary>
    private void BestScores() {

        int[] scores = HR_API.GetHighScores();

        bestScoreOneWay.text = "BEST SCORE\n" + scores[0];
        bestScoreTwoWay.text = "BEST SCORE\n" + scores[1];
        bestScoreTimeLeft.text = "BEST SCORE\n" + scores[2];
        bestScoreBomb.text = "BEST SCORE\n" + scores[3];
        levelMissionMode.text = "LEVEL\n" + PlayerPrefs.GetInt("levelMissionMode",1).ToString();

    }

#if PHOTON_UNITY_NETWORKING && BCG_HR_PHOTON

    public void SetMultiplayer(bool state) {

        PlayerPrefs.SetInt("Multiplayer", state ? 1 : 0);

        if (state) {

            HR_PhotonHandler photonHandler = HR_PhotonHandler.Instance;

            if (!photonHandler)
                Instantiate(Resources.Load("HR_Photon Handler", typeof(GameObject)));

            if (multiplayerMenu)
                EnableMenu(multiplayerMenu);

        } else {

            HR_PhotonHandler photonHandler = HR_PhotonHandler.Instance;

            if (photonHandler)
                Destroy(photonHandler.gameObject);

        }

    }

#endif

    /// <summary>
    /// Quits the game.
    /// </summary>
    public void QuitGame() {

        Application.Quit();

    }

}
