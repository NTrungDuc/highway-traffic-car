using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HPS_MainMenuHandler : MonoBehaviour
{
    #region SINGLETON PATTERN
    private static HPS_MainMenuHandler instance;
    public static HPS_MainMenuHandler Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<HPS_MainMenuHandler>();

            return instance;
        }
    }
    #endregion

    [Header("Spawn Location Of The Cars")]
    public Transform carSpawnLocation;

    private GameObject[] createdCars;
    public RCC_CarControllerV3 currentCar;
    public HR_ModApplier currentApplier;

    internal int realCarIndex = 0;
    internal int carIndex = 0;

    [Header("UI Menus")]
    public GameObject playerInfoMenu;
    public GameObject statMenu;
    public GameObject eventMenu;
    public GameObject storesMenu;
    public GameObject rankMenu;
    public GameObject rouletteMenu;
    public GameObject carsMenu;
    public GameObject botMenu;
    public GameObject modsSelectionMenu;
    public GameObject sceneSelectionMenu;

    [Header("Buttons")]
    public GameObject backButton;
    public GameObject rouletteButton;
    public GameObject currencyButton;
    public GameObject buyCarButton;
    public GameObject selectCarButton;
    public GameObject goPlayButton;
    public GameObject modCarPanel;
    public GameObject testLight;

    [Header("Texts")]
    public TextMeshProUGUI currencyGold;
    public TextMeshProUGUI currencyDiamond;
    public TextMeshProUGUI vehicleNameText;        //	Current vehicle name text.
    public TextMeshProUGUI vehicleClass;        //	Current vehicle name text.
    public TextMeshProUGUI nitroCount;
    public TextMeshProUGUI bestScoreOneWay;        //	Best score one way text.
    public TextMeshProUGUI bestScoreTwoWay;        //	Best score two ways text.
    public TextMeshProUGUI levelMissionMode;

    [Header("Animator and Animations")]
    public Animator idleNotice;
    public RuntimeAnimatorController buttonScale;
    public RuntimeAnimatorController buttonScaling;

    [Space]
    public GameObject carRank;

    private float idleTime;
    private string nameScene;

    internal AudioSource mainMenuSoundtrack;

    [Header("Dev viet")]
    public GameObject loadingModeAI;
    public int priceModeDual;
    [HideInInspector] public int numberPooling;
    private string countryCode;

#if PHOTON_UNITY_NETWORKING && BCG_HR_PHOTON
    private GameObject multiplayerMenu;
#endif

    private void Awake()
    {
        if (!PlayerPrefs.HasKey("FirstRun"))
        {
            StartCoroutine(CheckCountry());

            PlayerPrefs.SetInt("FirstRun", 1);
            PlayerPrefs.Save();
        }
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            Vibration.Init();
            StartCoroutine(HPS_UILoading.Instance.LoadScene(1));
            return;
        }

        if (PlayerPrefs.HasKey("First Time") || PlayerPrefs.GetInt("First Time", 0) == 1)
            HPS_API.Load("Resources", "HPS_CarDatas");
        else
        {
            PlayerPrefs.SetInt("First Time", 1);
            JsonUtility.FromJsonOverwrite(Resources.Load<TextAsset>("HPS_CarDatas_Json").text, HPS_CarDatas.Instance);
            HPS_API.Save("Resources", "HPS_CarDatas");
        }

        // Setting time scale, volume, unpause, and target frame rate.
        Time.timeScale = 1f;
        AudioListener.volume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        AudioListener.pause = false;

        //	Creating soundtracks for the main menu.
        if (HR_HighwayRacerProperties.Instance.mainMenuClips != null && HR_HighwayRacerProperties.Instance.mainMenuClips.Length > 0)
        {

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
        nitroCount.text = PlayerPrefs.GetInt("NitroCount", 0).ToString();
        if (PlayerPrefs.GetInt("NitroCount", 0) == 0)
            nitroCount.transform.parent.parent.GetComponent<Animator>().runtimeAnimatorController = buttonScaling;
        else
            nitroCount.transform.parent.parent.GetComponent<Animator>().runtimeAnimatorController = buttonScale;

        carIndex = PlayerPrefs.GetInt("SelectedPlayerCarIndex", 0);
        PlayerPrefs.SetInt("numberPooling", 0);
        EnableMenu(statMenu);
        HPS_ModHandler.Instance.ChooseClass(null);
        CreateCars();   //	Creating all selectable cars at once.
        //SpawnCar();     //	Spawning only target car (carIndex).


#if PHOTON_UNITY_NETWORKING && BCG_HR_PHOTON

        HR_PhotonLobbyManager multiplayerManager = FindObjectOfType<HR_PhotonLobbyManager>(true);

        if (multiplayerManager)
            multiplayerMenu = multiplayerManager.gameObject;

#endif

    }

    private void Update()
    {

        //	Displaying currency.
        currencyGold.text = HR_API.GetCurrencyGold().ToString("F0");
        currencyDiamond.text = HR_API.GetCurrencyDiamond().ToString("F0");

        //testLight.SetActive(playerInfoMenu.activeSelf);
        if (idleTime < 10f)
            idleTime += Time.deltaTime;
        else
        {
            if (!HPS_ModHandler.Instance.AnyClassOpen())
            {
                if (goPlayButton.activeSelf || selectCarButton.activeSelf)
                    idleNotice.gameObject.SetActive(true);
                else if (!goPlayButton.activeSelf && !selectCarButton.activeSelf)
                    idleNotice.gameObject.SetActive(false);
            }
            else
                idleNotice.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Creating all spawnable cars at once.
    /// </summary>
    private void CreateCars()
    {

        //	Creating a new array.
        //createdCars = new GameObject[HR_PlayerCars.Instance.cars.Length];
        createdCars = new GameObject[HPS_CarDatas.Instance.cardatas.Length];

        GameObject garageTemplate = carsMenu.GetComponentInChildren<ScrollRect>().content.GetChild(0).gameObject;

        //	Setting array elements.
        for (int i = 0; i < createdCars.Length; i++)
        {

            //createdCars[i] = (RCC.SpawnRCC(HR_PlayerCars.Instance.cars[i].playerCar.GetComponent<RCC_CarControllerV3>(), carSpawnLocation.position, carSpawnLocation.rotation, false, false, false)).gameObject;
            createdCars[i] = (RCC.SpawnRCC(((GameObject)Resources.Load("Player Vehicles/" + HPS_CarDatas.Instance.cardatas[i].carName, typeof(GameObject))).GetComponent<RCC_CarControllerV3>(), carSpawnLocation.position, carSpawnLocation.rotation, false, false, false)).gameObject;
            createdCars[i].GetComponent<RCC_CarControllerV3>().lowBeamHeadLightsOn = true;
            createdCars[i].SetActive(false);

            GameObject car = Instantiate(garageTemplate, carsMenu.GetComponentInChildren<ScrollRect>().content);
            car.GetComponent<HPS_UICarSelection>().carIndex = i;
            car.SetActive(true);
            car.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load("Car Selection/" + HPS_CarDatas.Instance.cardatas[i].carName, typeof(Sprite)) as Sprite;
            car.transform.GetChild(0).GetComponent<Image>().color = Color.white;
            car.GetComponent<Button>().onClick.AddListener(() => SetCarIndex(car.GetComponent<HPS_UICarSelection>().carIndex));
        }

        StartCoroutine(PreventFallingCar());

    }

    private IEnumerator PreventFallingCar()
    {
        for (int i = 0; i < createdCars.Length; i++)
        {
            createdCars[i].SetActive(true);
            RCC.RegisterPlayerVehicle(createdCars[carIndex].GetComponent<RCC_CarControllerV3>(), false, false);
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            createdCars[i].SetActive(false);
        }

        SpawnCar();

        yield return null;
    }

    /// <summary>
    /// Spawns target car (carIndex).
    /// </summary>
    private void SpawnCar(bool skipUI = false)
    {
        //	If price of the car is 0, or unlocked, save it as owned car.
        //if (HR_PlayerCars.Instance.cars[carIndex].price <= 0 || HR_PlayerCars.Instance.cars[carIndex].unlocked)
        //    HR_API.UnlockVehice(carIndex);

        if (HPS_CarDatas.Instance.cardatas[carIndex].price <= 0 || HPS_CarDatas.Instance.cardatas[carIndex].unlocked)
            HPS_API.UnlockVehice(carIndex);

        //	If current spawned car is owned, enable buy button, disable select button. Do opposite otherwise.
        //if (HR_API.OwnedVehicle(carIndex)) {
        if (HPS_API.OwnedVehicle(carIndex))
        {

            //  Displaying price null.
            //if (buyCarButton.GetComponentInChildren<Text>())
            //    buyCarButton.GetComponentInChildren<Text>().text = "";

            // Enabling select button, disabling buy button.
            HPS_ModHandler.Instance.ChooseClass(HPS_ModHandler.Instance.mainsClass);
            buyCarButton.SetActive(false);
            selectCarButton.SetActive(true);

            if (skipUI)
                HPS_ModHandler.Instance.CheckPurchaseOfModifiers();
        }
        else
        {

            //  Displaying price.
            if (buyCarButton.GetComponentInChildren<TextMeshProUGUI>())
            {
                buyCarButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"{HPS_CarDatas.Instance.cardatas[carIndex].price}";
                buyCarButton.transform.GetChild(1).gameObject.SetActive(true);
            }

            //  Setting OnClick Event
            if (buyCarButton.GetComponent<Button>())
            {
                buyCarButton.GetComponent<Button>().onClick.RemoveAllListeners();
                buyCarButton.GetComponent<Button>().onClick.AddListener(() => BuyCar());
            }

            //  Enabling buy button, disabling select button.
            HPS_ModHandler.Instance.HideClass();
            buyCarButton.SetActive(true);
            selectCarButton.SetActive(false);

        }

        //	Disabling all cars at once. And then enabling only target car (carIndex). And make sure spawned cars are always at spawn point.
        for (int i = 0; i < createdCars.Length; i++)
        {

            if (createdCars[i].activeInHierarchy)
                createdCars[i].SetActive(false);

        }

        //	Enabling only target car (carIndex).
        SelectCar();
        createdCars[carIndex].SetActive(true);
        RCC.RegisterPlayerVehicle(createdCars[carIndex].GetComponent<RCC_CarControllerV3>(), false, false);

        //	Setting current car.
        currentCar = createdCars[carIndex].GetComponent<RCC_CarControllerV3>();
        currentApplier = currentCar.GetComponent<HR_ModApplier>();

        //	Displaying car name text.
        if (vehicleNameText)
            vehicleNameText.text = HPS_CarDatas.Instance.cardatas[carIndex].carName;

        if (vehicleClass)
            vehicleClass.text = "CLASS " + HPS_CarDatas.Instance.cardatas[carIndex].carClass;

        if (carRank)
        {
            if (HPS_CarDatas.Instance.cardatas[carIndex].stars % 2 == 1)
            {
                carRank.GetComponent<HorizontalLayoutGroup>().padding.left = 23;
                carRank.GetComponent<HorizontalLayoutGroup>().padding.right = 23;
                switch (HPS_CarDatas.Instance.cardatas[carIndex].stars)
                {
                    case 1:
                        for (int i = 0; i < carRank.transform.childCount; i++)
                        {
                            if (i == 2)
                                carRank.transform.GetChild(i).GetComponent<Image>().color = Color.white;
                            else
                                carRank.transform.GetChild(i).GetComponent<Image>().color = new Color(1, 1, 1, 0);
                        }
                        break;
                    case 3:
                        for (int i = 0; i < carRank.transform.childCount; i++)
                        {
                            if (i > 0 && i < 4)
                                carRank.transform.GetChild(i).GetComponent<Image>().color = Color.white;
                            else
                                carRank.transform.GetChild(i).GetComponent<Image>().color = new Color(1, 1, 1, 0);
                        }
                        break;
                    default:
                        for (int i = 0; i < carRank.transform.childCount; i++)
                            carRank.transform.GetChild(i).GetComponent<Image>().color = Color.white;
                        break;
                }
            }
            else
            {
                carRank.GetComponent<HorizontalLayoutGroup>().padding.left = 35;
                carRank.GetComponent<HorizontalLayoutGroup>().padding.right = 11;
                switch (HPS_CarDatas.Instance.cardatas[carIndex].stars)
                {
                    case 2:
                        for (int i = 0; i < carRank.transform.childCount; i++)
                        {
                            if (i > 0 && i < 3)
                                carRank.transform.GetChild(i).GetComponent<Image>().color = Color.white;
                            else
                                carRank.transform.GetChild(i).GetComponent<Image>().color = new Color(1, 1, 1, 0);
                        }
                        break;
                    case 4:
                        for (int i = 0; i < carRank.transform.childCount; i++)
                        {
                            if (i < carRank.transform.childCount - 1)
                                carRank.transform.GetChild(i).GetComponent<Image>().color = Color.white;
                            else
                                carRank.transform.GetChild(i).GetComponent<Image>().color = new Color(1, 1, 1, 0);
                        }
                        break;
                }
            }
        }

        if (!skipUI)
        {
            HPS_ModHandler.Instance.ChooseClass(null);
            EnableMenu(statMenu);
        }

    }

    /// <summary>
    /// Purchases current car.
    /// </summary>
    public void BuyCar()
    {

        // If we own the car, don't consume currency.
        //if (HR_API.OwnedVehicle(carIndex)) {
        if (HPS_API.OwnedVehicle(carIndex))
        {

            Debug.LogError("Car is already owned!");
            return;

        }

        //	If currency is enough, save it and consume currency. Otherwise display the informer.
        //if (HR_API.GetCurrency() >= HR_PlayerCars.Instance.cars[carIndex].price) {
        if (HR_API.GetCurrencyGold() >= HPS_CarDatas.Instance.cardatas[carIndex].price)
        {

            //HR_API.ConsumeCurrency(HR_PlayerCars.Instance.cars[carIndex].price);
            HR_API.ConsumeCurrencyGold(HPS_CarDatas.Instance.cardatas[carIndex].price);
            HPS_CarDatas.Instance.cardatas[carIndex].unlocked = true;
            HPS_API.Save("Resources", "HPS_CarDatas");

        }
        else
        {

            //HR_UIInfoDisplayer.Instance.ShowInfo("Not Enough Coins", "You have to earn " + (HR_PlayerCars.Instance.cars[carIndex].price - HR_API.GetCurrency()).ToString() + " more coins to buy this vehicle", HR_UIInfoDisplayer.InfoType.NotEnoughMoney);
            HR_UIInfoDisplayer.Instance.ShowInfo("Not Enough Coins", "You have to earn " + (HPS_CarDatas.Instance.cardatas[carIndex].price - HR_API.GetCurrencyGold()).ToString() + " more coins to buy this vehicle", HR_UIInfoDisplayer.InfoType.NotEnoughMoney);
            return;

        }

        //	Saving the car.
        //HR_API.UnlockVehice(carIndex);
        HPS_API.UnlockVehice(carIndex);
        buyCarButton.SetActive(false);
        selectCarButton.SetActive(true);
        HPS_ModHandler.Instance.ChooseClass(HPS_ModHandler.Instance.mainsClass);
    }

    /// <summary>
    /// Selects the current car with carIndex.
    /// </summary>
    public void SelectCar()
    {

        PlayerPrefs.SetInt("SelectedPlayerCarIndex", carIndex);


        //EventSystem.current.SetSelectedGameObject(null);
        //EventSystem.current.SetSelectedGameObject(carSelectionMenu.GetComponentsInChildren<Button>().Where(button => button.name.Contains("Car")).ToArray()[carIndex].gameObject);


    }

    /// <summary>
    /// Switch to car.
    /// </summary>
    public void SetCarIndex(int carIndex)
    {
        if (HPS_API.OwnedVehicle(carIndex) || !HPS_API.OwnedVehicle(carIndex) && HPS_API.OwnedVehicle(this.carIndex) || HPS_API.OwnedVehicle(carIndex) && !HPS_API.OwnedVehicle(this.carIndex))
            realCarIndex = this.carIndex;

        this.carIndex = carIndex;

        if (this.carIndex >= createdCars.Length)
            this.carIndex = 0;

        for (int i = 1; i < carsMenu.GetComponentInChildren<ScrollRect>().content.childCount; i++)
        {
            if (carIndex + 1 == i)
                carsMenu.GetComponentInChildren<ScrollRect>().content.GetChild(i).GetChild(1).GetChild(0).GetComponent<Image>().color = Color.white;
            else
                carsMenu.GetComponentInChildren<ScrollRect>().content.GetChild(i).GetChild(1).GetChild(0).GetComponent<Image>().color = new Color(1, 1, 1, 0);
        }

        if (realCarIndex != carIndex)
            SpawnCar(true);
    }

    /// <summary>
    /// Enables target menu and disables all other menus.
    /// </summary>
    /// <param name="activeMenu"></param>
    public void EnableMenu(GameObject activeMenu)
    {
        statMenu.SetActive(false);
        eventMenu.SetActive(false);
        carsMenu.SetActive(false);
        storesMenu.SetActive(false);
        rankMenu.SetActive(false);
        rouletteMenu.SetActive(false);
        botMenu.SetActive(false);
        modsSelectionMenu.SetActive(false);
        sceneSelectionMenu.SetActive(false);

        if (activeMenu == statMenu)
        {
            backButton.SetActive(false);
            modCarPanel.SetActive(false);
            goPlayButton.SetActive(false);
            botMenu.SetActive(true);
            eventMenu.SetActive(true);
            playerInfoMenu.SetActive(true);
            rouletteButton.SetActive(true);
            currencyButton.SetActive(true);
            selectCarButton.SetActive(true);
            statMenu.GetComponent<Button>().onClick.RemoveAllListeners();
            statMenu.GetComponent<Button>().onClick.AddListener(() =>
            {
                modCarPanel.SetActive(true);
                botMenu.SetActive(false);
                eventMenu.SetActive(false);
                goPlayButton.SetActive(false);
                playerInfoMenu.SetActive(false);
                selectCarButton.SetActive(false);
                backButton.GetComponentInChildren<TextMeshProUGUI>().text = "UPGRADE";
                HPS_ModHandler.Instance.ChooseClass(HPS_ModHandler.Instance.mainsClass);
                HPS_ModHandler.Instance.ChooseClass(HPS_ModHandler.Instance.upgradesClass);
                HPS_ModHandler.Instance.CheckButtonColors(HPS_ModHandler.Instance.upgradeButton);
                statMenu.GetComponent<Button>().interactable = false;
                FindObjectOfType<HPS_UIModInfo>().Hide();
                backButton.SetActive(true);
                backButton.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
                backButton.GetComponentInChildren<Button>().onClick.AddListener(() => SetBackButton(statMenu));
            });
            HPS_ModHandler.Instance.ChooseClass(null);
        }

        if (activeMenu == carsMenu)
        {
            idleTime = 0f;
            idleNotice.gameObject.SetActive(false);
            SetCarIndex(carIndex);
            FindObjectOfType<HR_ShowroomCamera>().SetFocus(0);
            playerInfoMenu.SetActive(false);
            goPlayButton.SetActive(false);
            rouletteButton.SetActive(false);
            if (HPS_API.OwnedVehicle(carIndex))
            {
                selectCarButton.SetActive(true);
                buyCarButton.SetActive(false);
            }
            else
            {
                selectCarButton.SetActive(false);
                buyCarButton.SetActive(true);
            }
            backButton.SetActive(true);
            backButton.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
            backButton.GetComponentInChildren<Button>().onClick.AddListener(() => SetBackButton(activeMenu));
        }

        if (activeMenu == modsSelectionMenu)
        {
            goPlayButton.SetActive(false);
            playerInfoMenu.SetActive(false);
            rouletteButton.SetActive(false);
            currencyButton.SetActive(false);
            backButton.SetActive(true);
            selectCarButton.SetActive(true);
            selectCarButton.GetComponent<Button>().onClick.RemoveAllListeners();
            selectCarButton.GetComponent<Button>().onClick.AddListener(() => SetSelectButton(sceneSelectionMenu));
            backButton.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
            backButton.GetComponentInChildren<Button>().onClick.AddListener(() => SetBackButton(carsMenu));
        }

        if (activeMenu == sceneSelectionMenu)
        {
            selectCarButton.SetActive(false);
            goPlayButton.SetActive(true);
            backButton.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
            backButton.GetComponentInChildren<Button>().onClick.AddListener(() => SetBackButton(modsSelectionMenu));
        }

        if (activeMenu == storesMenu)
        {
            playerInfoMenu.SetActive(false);
            selectCarButton.SetActive(false);
            goPlayButton.SetActive(false);
            rouletteButton.SetActive(false);
            backButton.SetActive(true);
            backButton.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
            if (modCarPanel.activeSelf && !HPS_ModHandler.Instance.AnyClassOpen())
                backButton.GetComponentInChildren<Button>().onClick.AddListener(() => EnableMenu(carsMenu));
            else if (HPS_ModHandler.Instance.AnyClassOpen())
            {
                switch (HPS_ModHandler.Instance.GetOpenClass().name)
                {
                    case "Upgrades":
                        backButton.GetComponentInChildren<Button>().onClick.AddListener(() => { storesMenu.SetActive(false); HPS_ModHandler.Instance.upgradeButton.onClick.Invoke(); });
                        break;
                    case "Colors":
                        backButton.GetComponentInChildren<Button>().onClick.AddListener(() => { storesMenu.SetActive(false); HPS_ModHandler.Instance.bodyPaintButton.onClick.Invoke(); });
                        break;
                    case "Plates":
                        backButton.GetComponentInChildren<Button>().onClick.AddListener(() => { storesMenu.SetActive(false); HPS_ModHandler.Instance.platesButton.onClick.Invoke(); });
                        break;
                    case "Tires":
                        backButton.GetComponentInChildren<Button>().onClick.AddListener(() => { storesMenu.SetActive(false); HPS_ModHandler.Instance.tireButton.onClick.Invoke(); });
                        break;
                    case "Rims":
                        backButton.GetComponentInChildren<Button>().onClick.AddListener(() => { storesMenu.SetActive(false); HPS_ModHandler.Instance.rimButton.onClick.Invoke(); });
                        break;
                    case "Spoilers":
                        backButton.GetComponentInChildren<Button>().onClick.AddListener(() => { storesMenu.SetActive(false); HPS_ModHandler.Instance.spoilersButton.onClick.Invoke(); });
                        break;
                }   
            }
            else
                backButton.GetComponentInChildren<Button>().onClick.AddListener(() => EnableMenu(statMenu));
        }

        if (activeMenu == rankMenu)
        {
            playerInfoMenu.SetActive(false);
            selectCarButton.SetActive(false);
            goPlayButton.SetActive(false);
            rouletteButton.SetActive(false);
            backButton.SetActive(true);
            backButton.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
            backButton.GetComponentInChildren<Button>().onClick.AddListener(() => EnableMenu(statMenu));
        }

        if (activeMenu == rouletteMenu)
        {
            playerInfoMenu.SetActive(false);
            selectCarButton.SetActive(false);
            goPlayButton.SetActive(false);
            rouletteButton.SetActive(false);
            backButton.SetActive(true);
            backButton.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
            backButton.GetComponentInChildren<Button>().onClick.AddListener(() => EnableMenu(statMenu));
        }

        activeMenu.SetActive(true);

        //if (activeMenu == modsSelectionMenu)
        //    BestScores();

    }

    /// <summary>
    /// Disables target menu.
    /// </summary>
    /// <param name="deactiveMenu"></param>
    public void DisableMenu(GameObject deactiveMenu)
    {
        if (deactiveMenu == carsMenu)
        {
            if (HPS_API.OwnedVehicle(carIndex))
                buyCarButton.SetActive(false);

            selectCarButton.SetActive(false);
            backButton.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
            backButton.GetComponentInChildren<Button>().onClick.AddListener(() => SetBackButton(deactiveMenu));

            if (HPS_ModHandler.Instance.GetOpenClass().name == "Upgrades")
            {
                statMenu.SetActive(true);
                nitroCount.text = PlayerPrefs.GetInt("NitroCount", 0).ToString();
                if (PlayerPrefs.GetInt("NitroCount", 0) == 0)
                    nitroCount.transform.parent.parent.GetComponent<Animator>().runtimeAnimatorController = buttonScaling;
                else
                    nitroCount.transform.parent.parent.GetComponent<Animator>().runtimeAnimatorController = buttonScale;
            }
        }

        idleTime = 0f;
        idleNotice.gameObject.SetActive(false);
        deactiveMenu.SetActive(false);
    }

    /// <summary>
    /// Enable target button.
    /// </summary>
    /// <param name="activeButton"></param>
    public void EnableButton(GameObject activeButton)
    {
        activeButton.SetActive(true);
    }

    /// <summary>
    /// Disable target button.
    /// </summary>
    /// <param name="deactiveButton"></param>
    public void DisableButton(GameObject deactiveButton)
    {
        deactiveButton.SetActive(false);
    }

    /// <summary>
    /// Selects the scene with int.
    /// </summary>
    /// <param name="levelIndex"></param>
    public void SelectScene(string levelName)
    {

        //PlayerPrefs.SetString("SelectedScene", levelName);
        nameScene = levelName;
    }

    public void SelcetSceneModeDual()
    {
        var numberScene = Random.Range(0, 3);
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
        var number = Random.Range(0, 10);
        if (number % 2 == 0)
            numberPooling = 0;
        else
            numberPooling = 1;
        PlayerPrefs.SetInt("SelectedModeIndex", 6);
        PlayerPrefs.SetInt("numberPooling", numberPooling);
        PlayerPrefs.SetString("SelectedScene", nameScene);
        selectCarButton.SetActive(false);
        goPlayButton.SetActive(true);
    }

    public void EndlessModeSelected()
    {
        PlayerPrefs.SetString("SelectedScene", nameScene = "HighwaySunny");
        SelectMode(0);
    }

    /// <summary>
    /// Selects the mode with int.
    /// </summary>
    /// <param name="_modeIndex"></param>
    public void SelectMode(int _modeIndex)
    {
        //	Saving the selected mode, and enabling scene selection menu.
        PlayerPrefs.SetInt("SelectedModeIndex", _modeIndex);

    }

    /// <summary>
    /// Highlight the target button.
    /// </summary>
    /// <param name="highlightButton"></param>
    public void HighlightButton(GameObject highlightButton)
    {

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(highlightButton);

    }

    /// <summary>
    /// Selects the scene with int.
    /// </summary>
    /// <param name="levelIndex"></param>
    public void StartRace()
    {
        if (nameScene == null)
        {
            return;
        }
        
        SelectCar();
        createdCars[carIndex].GetComponent<RCC_CarControllerV3>().KillOrStartEngine();

        if (PlayerPrefs.GetInt("SelectedModeIndex") == 6)
        {
            if (HR_API.GetCurrencyGold() >= priceModeDual)
            {
                PlayerPrefs.SetInt("priceModeDual", priceModeDual);
                EnableMenu(loadingModeAI);
                StartCoroutine(LoadingModeAI());
            }
        }
        else
        {
            mainMenuSoundtrack.Stop();
            StartCoroutine(HPS_UILoading.Instance.LoadScene(nameScene));
        }

    }
    
    private IEnumerator LoadingModeAI()
    {
        loadingModeAI.SetActive(true);
        loadingModeAI.transform.GetChild(1).gameObject.SetActive(false);
        yield return new WaitForSeconds(2);
        loadingModeAI.transform.GetChild(1).gameObject.SetActive(true);
        yield return new WaitForSeconds(1);
        mainMenuSoundtrack.Stop();
        StartCoroutine(HPS_UILoading.Instance.LoadScene(nameScene));
    }

    /// <summary>
    /// Displays best scores of all four modes.
    /// </summary>
    private void BestScores()
    {

        int[] scores = HR_API.GetHighScores();

        bestScoreOneWay.text = "BEST SCORE\n" + scores[0];
        bestScoreTwoWay.text = "BEST SCORE\n" + scores[1];
        levelMissionMode.text = "LEVEL\n" + PlayerPrefs.GetInt("levelMissionMode", 1).ToString();

    }

    public void MissionModeMap()
    {
        int selectMode = PlayerPrefs.GetInt("levelMissionMode", 1) - 1;
        nameScene = DataSelectMapMissionMode.Instance.dataMisionMode.selectModes[selectMode].nameMap;
        SelectMode(DataSelectMapMissionMode.Instance.dataMisionMode.selectModes[selectMode].mode);
        PlayerPrefs.SetString("SelectedScene", nameScene);
        selectCarButton.SetActive(false);
        goPlayButton.SetActive(true);
    }

    private void SetBackButton(GameObject activeMenu)
    {
        if (activeMenu == carsMenu)
        {
            if (HPS_ModHandler.Instance.AnyClassOpen())
                botMenu.GetComponentsInChildren<Button>()[0].onClick.Invoke();
            else
            {
                selectCarButton.GetComponent<Button>().onClick.RemoveAllListeners();
                selectCarButton.GetComponent<Button>().onClick.AddListener(() => SetSelectButton(modsSelectionMenu));
                if (!HPS_API.OwnedVehicle(carIndex))
                {
                    carIndex = realCarIndex;
                    SpawnCar(true);
                }
                statMenu.GetComponent<Button>().interactable = true;
                idleTime = 0f;
                idleNotice.gameObject.SetActive(false);
                EnableMenu(statMenu);
            }
        }

        if (activeMenu == modsSelectionMenu)
        {
            backButton.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
            backButton.GetComponentInChildren<Button>().onClick.AddListener(() => SetBackButton(carsMenu));
            EnableMenu(modsSelectionMenu);
        }

        if (activeMenu == statMenu)
        {
            statMenu.GetComponent<Button>().interactable = true;
            EnableMenu(statMenu);
        }

    }

    private void SetSelectButton(GameObject activeMenu)
    {
        if (activeMenu == sceneSelectionMenu)
        {
            nameScene = PlayerPrefs.GetString("SelectedScene");
            EnableMenu(sceneSelectionMenu);
        }

        if (activeMenu == modsSelectionMenu)
            EnableMenu(modsSelectionMenu);

    }

    IEnumerator CheckCountry()
    {
        string url = "http://ip-api.com/json/";
        UnityWebRequest request = UnityWebRequest.PostWwwForm(url, "");
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.ProtocolError || request.result == UnityWebRequest.Result.ConnectionError)
        {
            UnityEngine.Debug.LogError($"Check location country: {request.error}");
        }
        else
        {
            CountryLocation countryLocation = JsonUtility.FromJson<CountryLocation>(request.downloadHandler.text);
            countryCode = countryLocation.countryCode;
            PlayerPrefs.SetString("countryCode", countryCode);
        }
    }

    public void NitroAds()
    {
        int nitroCount = PlayerPrefs.GetInt("NitroCount", 0);
        nitroCount += 2;
        PlayerPrefs.SetInt("NitroCount", nitroCount);
        this.nitroCount.text = nitroCount.ToString();
        this.nitroCount.transform.parent.parent.GetComponent<Animator>().runtimeAnimatorController = buttonScale;
    }

    public void ShowOptions()
    {
        HR_UIOptionsHandler.Instance.Show();
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
    public void QuitGame()
    {

        Application.Quit();

    }


}
