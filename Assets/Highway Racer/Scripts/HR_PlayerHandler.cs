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
using UnityEditor;
using UnityEngine.InputSystem.XR;
using Unity.VisualScripting;
using TMPro;
using System;
#if PHOTON_UNITY_NETWORKING
using Photon;
using Photon.Pun;
#endif

/// <summary>
/// Player manager that containts current score, near misses.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(RCC_CarControllerV3))]
[RequireComponent(typeof(HR_ModApplier))]
[AddComponentMenu("BoneCracker Games/Highway Racer/Player/HR Player Handler")]
public class HR_PlayerHandler : MonoBehaviour
{

    private RCC_CarControllerV3 carController;
    public RCC_CarControllerV3 CarController
    {

        get
        {

            if (!carController)
                carController = GetComponent<RCC_CarControllerV3>();

            return carController;

        }

    }

    private Rigidbody rigid;
    public Rigidbody Rigid
    {

        get
        {

            if (!rigid)
                rigid = GetComponent<Rigidbody>();

            return rigid;

        }

    }

    public bool canCrash = true;
    public string Class;
    public int star;
    [Space()]
    [Range(200f, 1000f)] public float currentEngineTorque = 300f;        //	Current engine torque.
    [Range(2000f, 6000f)] public float currentBrakeTorque = 2000f;        //	Current brake torque.
    [Range(.1f, .5f)] public float currentHandlingStrength = .1f;     //	Current handling strength.
    [Range(200f, 400f)] public float currentSpeed = 360f;     //	Current speed.
    [Range(3f, 10f)] public float currentNOS = 3f;      //    Current NOS.
    [Space()]
    [Range(200f, 1000f)] public float maxEngineTorque = 300f;        //	Maximum upgradable engine torque.
    [Range(2000f, 6000f)] public float maxBrakeTorque = 2000f;        //	Maximum upgradable brake torque.
    [Range(.1f, .5f)] public float maxHandlingStrength = .1f;     //	Maximum upgradable handling strength.
    [Range(200f, 400f)] public float maxSpeed = 360f;     //	Maximum upgradable speed.
    [Range(3f, 10f)] public float maxNOS = 10f;      //    Maximun upgradable NOS.
    [Space()]
    public bool unlocked = false;
    public int price = 25000;

    public float damage = 0f;       //  Current damage.
    internal bool crashed = false;      //	Game is over now?

    internal float score = 0;       //  Score
    internal float timeLeft = 0;     //	Time left.
    internal int combo;     //	Current near miss combo.
    internal int maxCombo;      //	Highest combo count.
    internal float distanceToNextPlayer = -9999f;     //	Time left.

    internal float speed = 0f;      //  Current speed.
    internal float distance = 0f;       //  Total distance traveled.
    internal float highSpeedCurrent = 0f;       //  Current high speed time.
    internal float highSpeedTotal = 0f;     //  Total high speed time.
    internal float opposideDirectionCurrent = 0f;       //  Current opposite direction time.
    internal float opposideDirectionTotal = 0f;     //  Total opposite direction time.
    internal int rams;        //  Total rams.
    private float comboTime;        //  Combo time for near misses.
    public Vector3 previousPosition;       //  Previous position used to calculate total traveled distance.

    //mission mode
    [SerializeField] private GameObject checkPointMissionMode;
    private Vector3 distanceCrashed;
    internal float distanceWinMM = 0.1f;
    internal float distanceMMText = 0;
    internal int level = 1;
    internal bool gameWin = false;
    internal bool gameOver = false;
    private AudioSource winSound;


    private int MinimumSpeedToScore
    {
        get
        {
            return HR_HighwayRacerProperties.Instance._minimumSpeedForGainScore;
        }
    }
    private int MinimumSpeedToHighSpeed
    {
        get
        {
            return HR_HighwayRacerProperties.Instance._minimumSpeedForHighSpeed;
        }
    }

    public int TotalDistanceMoneyMP
    {
        get
        {
            return HR_HighwayRacerProperties.Instance._totalDistanceMoneyMP;
        }
    }
    public int TotalNearMissMoneyMP
    {
        get
        {
            return HR_HighwayRacerProperties.Instance._totalNearMissMoneyMP;
        }
    }
    public int TotalOverspeedMoneyMP
    {
        get
        {
            return HR_HighwayRacerProperties.Instance._totalOverspeedMoneyMP;
        }
    }
    public int TotalOppositeDirectionMP
    {
        get
        {
            return HR_HighwayRacerProperties.Instance._totalOppositeDirectionMP;
        }
    }

    public string currentTrafficCarNameLeft = null;
    public string currentTrafficCarNameRight = null;

    public string currentTrafficCarNameBack = null;

    internal bool bombTriggered = false;
    internal float bombHealth = 100f;

    private AudioSource hornSource;

    public delegate void onNearMiss(HR_PlayerHandler player, int score, HR_UIDynamicScoreDisplayer.Side side);
    public static event onNearMiss OnNearMiss;

    public delegate void onDestroyTrafficCar(HR_PlayerHandler player, int score, HR_UIDynamicScoreDisplayer.Side side);
    public static event onDestroyTrafficCar OnDestroyTrafficCar;
    bool isCollide = false;
    [SerializeField] GameObject[] fxNitro;
    [SerializeField] GameObject[] fxTrailLight;
    private float resetTime = 0f;
    //dev viet
    float timeRun = 0;
    public bool isCheckLostOrWin;
    internal int gold;
    internal int diamond;
    public Transform frontPointBefore;
   //public Transform frontPointAfter;
    public LayerMask layerMaskBefore;
    public LayerMask layerMaskAfter;
    public LayerMask layerMask;
    public bool isCheckNitro;
    public Collider[] colider;
    public HPS_TrafficCar trafficCar;
    private HR_AICarNew AICar;
    public Transform checkNearMiss;
#if PHOTON_UNITY_NETWORKING
    private PhotonView photonView;
#endif

    private void OnEnable()
    {

        //	If engine is not running, start the engine.
        if (CarController && !CarController.engineRunning)
            CarController.StartEngine();

#if PHOTON_UNITY_NETWORKING && BCG_HR_PHOTON

        photonView = GetComponent<PhotonView>();

        if (photonView && !photonView.IsMine)
            return;

#endif

        //	Creating horn audio source.
        hornSource = HR_CreateAudioSource.NewAudioSource(gameObject, "Horn", 10f, 100f, 1f, HR_HighwayRacerProperties.Instance.hornClip, true, false, false);

        CheckGroundGap();

        //WheelCollider[] wheelColliders = GetComponentsInChildren<WheelCollider>(true);

        //foreach (WheelCollider item in wheelColliders)
        //    item.forceAppPointDistance = .15f;

        //Mission Mode
        //HR_GamePlayHandler.Instance.mode = HR_GamePlayHandler.Mode.OneWay;
        if (!HR_GamePlayHandler.Instance)
            return;
        //ShowEffectTrailLight(true);
        GetDataMode();
        StartCoroutine(GetPosAfter3s());
    }
    private void Awake()
    {
        CarController.isCheckAi = false;
    }
    private void Start()
    {
       
        
    }
    private void Update()
    {
        //	If scene doesn't include gameplay manager, return.
        if (!HR_GamePlayHandler.Instance)
            return;

        CarController.isCheckNitroAI = isCheckNitro;
#if PHOTON_UNITY_NETWORKING && BCG_HR_PHOTON

        if (photonView && !photonView.IsMine)
            return;

#endif
        if (Physics.Raycast(CarController.COM.position, -frontPointBefore.right, out RaycastHit hitRinghtBefore1, 10f, layerMask) && AICar == null)
        {

            AICar = hitRinghtBefore1.collider.GetComponentInParent<HR_AICarNew>();
            for (int i = 0; i < colider.Length; i++)
            {
                for (int j = 0; j < AICar.colider.Length; j++)
                {
                    Physics.IgnoreCollision(colider[i], AICar.colider[j]);

                }
            }

        }

        if (PlayerPrefs.GetInt("SelectedModeIndex") == 6 && HR_GamePlayHandler.Instance != null)
        {

            #region Raycast is Check AI

            if (Physics.Raycast(frontPointBefore.position, frontPointBefore.right, out RaycastHit hitRinghtBefore, 10f, layerMaskBefore))
            {
                HR_GamePlayHandler.Instance.isCheckAIBefore = true;
                HR_GamePlayHandler.Instance.isCheckAIAfter = false;
                Debug.Log("Có xe bên phải truoc!");
            }
            if (Physics.Raycast(frontPointBefore.position, -frontPointBefore.right, out RaycastHit hitLeftBefore, 10f, layerMaskBefore))
            {
                HR_GamePlayHandler.Instance.isCheckAIBefore = true;
                HR_GamePlayHandler.Instance.isCheckAIAfter = false;
                Debug.Log("Có xe bên trái truoc!");
            }
            if (Physics.Raycast(frontPointBefore.position, frontPointBefore.right, out RaycastHit hitRinghtAfter, 10f, layerMaskAfter))
            {
                HR_GamePlayHandler.Instance.isCheckAIBefore = false;
                HR_GamePlayHandler.Instance.isCheckAIAfter = true;
                Debug.Log("Có xe bên phải sau!");
            }
            if (Physics.Raycast(frontPointBefore.position, -frontPointBefore.right, out RaycastHit hitLeftAfter, 10f, layerMaskAfter))
            {
                HR_GamePlayHandler.Instance.isCheckAIBefore = false;
                HR_GamePlayHandler.Instance.isCheckAIAfter = true;
                Debug.Log("Có xe bên trái sau!");
            }
            Debug.DrawRay(frontPointBefore.position, frontPointBefore.right * 10f, Color.green);
            Debug.DrawRay(frontPointBefore.position, -frontPointBefore.right * 10f, Color.green);
            Debug.DrawRay(frontPointBefore.position, frontPointBefore.right * 10f, Color.green);
            Debug.DrawRay(frontPointBefore.position, -frontPointBefore.right * 10f, Color.green);
            #endregion

        }

        speed = CarController.speed;
        //	If game is not started yet, return.
        if (crashed || !HR_GamePlayHandler.Instance.gameStarted)
            return;

        //	Speed of the car.
        

        // Total distance traveled.
        distance += Vector3.Distance(previousPosition, transform.position) / 1000f;
        previousPosition = transform.position;
        if (HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.MissionMode_1 || HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.MissionMode_2)
            distanceMMText = distanceWinMM - distance;
        //	Is speed is high enough, gain score.
        //if (speed >= MinimumSpeedToScore)
        //    score += CarController.speed * (Time.deltaTime * .05f);

        //	If speed is higher than high speed, gain score.
        if (speed >= MinimumSpeedToHighSpeed)
        {

            highSpeedCurrent += Time.deltaTime;
            highSpeedTotal += Time.deltaTime;

        }
        else
        {

            highSpeedCurrent = 0f;

        }

        // If car is at opposite direction, gain score.
        if (speed >= (MinimumSpeedToHighSpeed / 2f) && transform.position.x <= 0f && HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.TwoWay || HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.MissionMode_2)
        {

            opposideDirectionCurrent += Time.deltaTime;
            opposideDirectionTotal += Time.deltaTime;

        }
        else
        {

            opposideDirectionCurrent = 0f;

        }
        if (HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.MissionMode_1 || HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.MissionMode_2)
        {
            ProgressMissionMode.Instance.ProgressBar(distance, DataLevelMissionMode.Instance.levelData.levelGames[level - 1].distanceWin);
        }
        //	If mode is Mission Mode, reduce the timer.
        if (HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.MissionMode_1
            || HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.MissionMode_2)
        {
            if (!gameWin && !crashed)
            {
                if(timeLeft > 0)
                    timeLeft -= Time.deltaTime;

                // If timer hits 0, game over.
                if (timeLeft < 0)
                {

                    timeLeft = 0;
                    //GameOver();
                    GameFail();

                }
            }
        }

        comboTime += Time.deltaTime;

        //	If game mode is bomb...
        if (HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.Bomb)
        {

            //	Bomb will be triggered below 80 km/h.
            if (speed > 80f)
            {

                if (!bombTriggered)
                    bombTriggered = true;
                else
                    bombHealth += Time.deltaTime * 5f;

            }
            else if (bombTriggered)
            {

                bombHealth -= Time.deltaTime * 10f;

            }

            bombHealth = Mathf.Clamp(bombHealth, 0f, 100f);
            if (!RCC_SceneManager.Instance.activePlayerVehicle.useNitro)
            {
                //	If bomb health hits 0, blow and game over.
                if (bombHealth <= 0f)
                {

                    GameObject explosion = Instantiate(HR_HighwayRacerProperties.Instance.explosionEffect, transform.position, transform.rotation);
                    explosion.transform.SetParent(null);
                    Rigid.AddForce(Vector3.up * 10000f, ForceMode.Impulse);
                    Rigid.AddTorque(Vector3.up * 10000f, ForceMode.Impulse);
                    Rigid.AddTorque(Vector3.forward * 10000f, ForceMode.Impulse);

                    GameOver();

                }
            }
        }

        if (comboTime >= 2)
            combo = 0;
            CheckStatus();
    }

    IEnumerator delayWinOrLost()
    {
        if (!HR_GamePlayHandler.Instance.isCheckTimeAI)
        {
            PlayerPrefs.SetInt("checkWinOrLost", 1);
            yield return new WaitForSeconds(1f);
            GameWin();
            Debug.Log("win");
            HR_API.AddCurrencyGold(PlayerPrefs.GetInt("priceModeDual") * 2);
            var numberWin = PlayerPrefs.GetInt("numberWin");
            numberWin++;
            PlayerPrefs.SetInt("numberWin", numberWin);
        }
        else
        {
            PlayerPrefs.SetInt("checkWinOrLost", 2);
            yield return new WaitForSeconds(1f);
            GameOver();
            Debug.Log("lost");
            var numberLost = PlayerPrefs.GetInt("numberLost");
            numberLost++;
            PlayerPrefs.SetInt("numberLost", numberLost);
        }

    }

    private void FixedUpdate()
    {


#if PHOTON_UNITY_NETWORKING && BCG_HR_PHOTON

        if (photonView && !photonView.IsMine)
            return;

#endif

        //	If scene doesn't include gameplay manager, return.
        if (!HR_GamePlayHandler.Instance)
            return;

        //	If game is started, check near misses with raycasts.
        if (!crashed && HR_GamePlayHandler.Instance.gameStarted)
        {

            if (CarController.useNitro)
                CheckFreeRam(CarController.COM.position + (transform.up * 0.5f), 3.5f, 35, 3);
            else
                CheckNearMiss();

            CheckGameOver();
        }

    }

    IEnumerator GetPosAfter3s()
    {
        yield return new WaitForSeconds(3f);
        previousPosition = transform.position;
        if (HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.MissionMode_1 || HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.MissionMode_2)
        {
            Instantiate(checkPointMissionMode, new Vector3(checkPointMissionMode.transform.position.x
                , checkPointMissionMode.transform.position.y, (DataLevelMissionMode.Instance.levelData.levelGames[level - 1].distanceWin + (previousPosition.z / 1000)) * 1000), Quaternion.identity);
        }
        else if (HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.AIRacing)
            Instantiate(checkPointMissionMode, new Vector3(checkPointMissionMode.transform.position.x
                , checkPointMissionMode.transform.position.y, (HR_GamePlayHandler.Instance.maxDistance + (previousPosition.z / 1000)) * 1000), Quaternion.identity);

    }
    void GetDataMode()
    {
        if (HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.OneWay || HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.TwoWay)
        {
            timeLeft = 0;
        }
        if (HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.MissionMode_1 || HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.MissionMode_2)
        {
            if (PlayerPrefs.GetInt("levelMissionMode") != 0)
            {
                level = PlayerPrefs.GetInt("levelMissionMode");
            }
            timeLeft = DataLevelMissionMode.Instance.levelData.levelGames[level - 1].time;
            distanceWinMM = DataLevelMissionMode.Instance.levelData.levelGames[level - 1].distanceWin;
            distanceMMText = distanceWinMM;
            Debug.Log(level);
            Debug.Log(DataLevelMissionMode.Instance.levelData.levelGames[level - 1].distanceWin);
        }
    }
    /// <summary>
    /// Checks near vehicles by drawing raycasts to the left and right sides.
    /// </summary>
    private void CheckNearMiss()
    {

        RaycastHit hit;
        Debug.DrawRay(checkNearMiss.position, (Quaternion.Euler(0, 30f, 0) * -transform.right * 2f), Color.white);
        Debug.DrawRay(checkNearMiss.position, (Quaternion.Euler(0, -30f, 0) * transform.right * 2f), Color.white);
        // Raycasting to the left side.
        if (Physics.Raycast(checkNearMiss.position, (Quaternion.Euler(0, 30f, 0) * -transform.right), out hit, 2f, HR_HighwayRacerProperties.Instance.trafficCarsLayer) && !hit.collider.isTrigger)
        {

            //	If hits, get it's name.
            currentTrafficCarNameLeft = hit.transform.name;
        }
        else
        {

            if (currentTrafficCarNameLeft != null && speed > HR_HighwayRacerProperties.Instance._minimumSpeedForGainScore)
            {
                combo++;
                comboTime = 0;

                if (maxCombo <= combo)
                    maxCombo = combo;

                if (isCollide)
                {
                    Vibration.VibratePop();
                    if (CarController.useNitro && !CarController.hasUseNitro)
                    {
                        score += 5;
                        OnDestroyTrafficCar(this, 5, HR_UIDynamicScoreDisplayer.Side.Left);
                    }
                    else
                    {
                        score += 5;
                        OnDestroyTrafficCar(this, 5, HR_UIDynamicScoreDisplayer.Side.Left);
                    }
                    isCollide = false;
                }
                else
                {
                    if (CarController.useNitro && !CarController.hasUseNitro)
                    {
                        score++;
                        OnNearMiss(this, 1, HR_UIDynamicScoreDisplayer.Side.Left);
                    }
                    else
                    {
                        score++;
                        OnNearMiss(this, 1, HR_UIDynamicScoreDisplayer.Side.Left);
                    }
                }

                currentTrafficCarNameLeft = null;
            }
            else
                currentTrafficCarNameLeft = null;

        }

        // Raycasting to the right side.
        if (Physics.Raycast(checkNearMiss.position, (Quaternion.Euler(0, -30f, 0) * transform.right), out hit, 2f, HR_HighwayRacerProperties.Instance.trafficCarsLayer) && !hit.collider.isTrigger)
        {

            //	If hits, get it's name.
            currentTrafficCarNameRight = hit.transform.name;

        }
        else
        {

            if (currentTrafficCarNameRight != null && speed > HR_HighwayRacerProperties.Instance._minimumSpeedForGainScore)
            {
                combo++;
                comboTime = 0;

                if (maxCombo <= combo)
                    maxCombo = combo;

                if (isCollide)
                {
                    Vibration.VibratePop();
                    if (CarController.useNitro && !CarController.hasUseNitro)
                    {
                        score += 5;
                        OnDestroyTrafficCar(this, 5, HR_UIDynamicScoreDisplayer.Side.Right);
                    }
                    else
                    {
                        score += 5;
                        OnDestroyTrafficCar(this, 5, HR_UIDynamicScoreDisplayer.Side.Right);
                    }
                    isCollide = false;
                }
                else
                {
                    if (CarController.useNitro && !CarController.hasUseNitro)
                    {
                        score++;
                        OnNearMiss(this, 1, HR_UIDynamicScoreDisplayer.Side.Right);
                    }
                    else
                    {
                        score++;
                        OnNearMiss(this, 1, HR_UIDynamicScoreDisplayer.Side.Right);
                    }
                }

                currentTrafficCarNameRight = null;
            }
            else
                currentTrafficCarNameRight = null;

        }

        // Raycasting to the front side. Used for taking down the lane.
        if (Physics.Raycast(checkNearMiss.position, (transform.forward), out hit, 40f, HR_HighwayRacerProperties.Instance.trafficCarsLayer) && !hit.collider.isTrigger)
        {
            Debug.DrawRay(checkNearMiss.position, (transform.forward * 20f), Color.red);

            if (CarController.highBeamHeadLightsOn)
                hit.transform.SendMessage("ChangeLines");
        }

        // Horn and siren.
        if (hornSource)
        {

            hornSource.volume = Mathf.Lerp(hornSource.volume, CarController.highBeamHeadLightsOn ? 1f : 0f, Time.deltaTime * 25f);

            if (CarController.highBeamHeadLightsOn)
            {
                HR_VehicleUpgrade_Siren upgradeSiren = GetComponentInChildren<HR_VehicleUpgrade_Siren>();

                if (upgradeSiren && upgradeSiren.isActiveAndEnabled)
                    hornSource.clip = HR_HighwayRacerProperties.Instance.sirenAudioClip;

                if (!hornSource.isPlaying)
                    hornSource.Play();
            }
            else
                hornSource.Stop();
        }

    }

    void CheckFreeRam(Vector3 origin, float distance, float angle, int numOfRays)
    {
        float angleStep = angle / numOfRays;

        for (int i = 0; i <= numOfRays; i++)
        {

            float currentAngle = -angle / 2 + angleStep * i;
            float adjustedDistance = distance / Mathf.Cos(currentAngle * Mathf.Deg2Rad);
            Vector3 rayDirection = Quaternion.Euler(0, currentAngle, 0) * transform.forward;

            Debug.DrawRay(origin, (rayDirection * adjustedDistance), Color.red);
            if (Physics.Raycast(origin, rayDirection, out RaycastHit hit, adjustedDistance, HR_HighwayRacerProperties.Instance.trafficCarsLayer) && !hit.collider.isTrigger)
            {

                if (hit.transform.TryGetComponent(out HPS_TrafficCar trafficCar))
                {
                    trafficCar.Rammed(currentTrafficCarNameLeft != null, currentTrafficCarNameRight != null, opposideDirectionCurrent > .1f);
                    HR_CarCamera.Instance.CameraBack();

                    if (!CarController.hasUseNitro)
                    {
                        Vibration.VibratePop();
                        score += 5;

                        switch (currentAngle)
                        {
                            case var v when currentAngle < 0:
                                OnDestroyTrafficCar(this, 5, HR_UIDynamicScoreDisplayer.Side.Left);
                                break;
                            case var v when currentAngle > 0:
                                OnDestroyTrafficCar(this, 5, HR_UIDynamicScoreDisplayer.Side.Right);
                                break;
                            default:
                                OnDestroyTrafficCar(this, 5, HR_UIDynamicScoreDisplayer.Side.Center);
                                break;
                        }
                    }
                }
            }
        }
    }

    public void CheckGameOver()
    {
        RaycastHit hit;

        Debug.DrawRay(checkNearMiss.position, (transform.forward * 10f), Color.white);
        if (Physics.Raycast(checkNearMiss.position, (transform.forward), out hit, 10f, HR_HighwayRacerProperties.Instance.trafficCarsLayer) && !hit.collider.isTrigger)
        {

            //	If hits, get it's name.
            currentTrafficCarNameBack = hit.transform.name;

        }
        else
        {
            currentTrafficCarNameBack = null;
        }
    }
    public void ShowEffectNitro(bool useNitro)
    {
        foreach (GameObject i in fxNitro)
        {
            i.SetActive(useNitro);
        }
    }
    public void ShowEffectTrailLight(bool useTrailCar)
    {
        foreach (GameObject i in fxTrailLight)
        {
            i.SetActive(useTrailCar);
        }
    }
    IEnumerator DelayCarDie()
    {
        //GamePause(false);
        yield return new WaitForSeconds(1f);
        GameCoroutine();
        Array.ForEach(gameObject.transform.GetComponentsInChildren<MeshRenderer>(), meshRenderer => meshRenderer.enabled = false);
        yield return new WaitForSeconds(0.2f);
        Array.ForEach(gameObject.transform.GetComponentsInChildren<MeshRenderer>(), meshRenderer => meshRenderer.enabled = true);
        yield return new WaitForSeconds(0.2f);
        Array.ForEach(gameObject.transform.GetComponentsInChildren<MeshRenderer>(), meshRenderer => meshRenderer.enabled = false);
        yield return new WaitForSeconds(0.2f);
        Array.ForEach(gameObject.transform.GetComponentsInChildren<MeshRenderer>(), meshRenderer => meshRenderer.enabled = true);
        yield return new WaitForSeconds(0.2f);
        Array.ForEach(gameObject.transform.GetComponentsInChildren<MeshRenderer>(), meshRenderer => meshRenderer.enabled = false);
        yield return new WaitForSeconds(0.2f);
        Array.ForEach(gameObject.transform.GetComponentsInChildren<MeshRenderer>(), meshRenderer => meshRenderer.enabled = true);
        yield return new WaitForSeconds(2);
        trafficCar.bodyCollider.isTrigger = false;

    }

    private void OnCollisionEnter(Collision col)
    {

#if PHOTON_UNITY_NETWORKING && BCG_HR_PHOTON

        if (photonView && !photonView.IsMine)
            return;

#endif
        if (HR_GamePlayHandler.Instance != null && HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.AIRacing)
        {
            if (col.gameObject.tag == "TrafficCar" && !isCheckLostOrWin)
            {
                trafficCar = col.gameObject.transform.GetComponent<HPS_TrafficCar>();
                trafficCar.bodyCollider.isTrigger = true;
                //if (CarController.speed < 80)
                //{
                //    trafficCar = col.gameObject.transform.GetComponent<HR_TrafficCar>();
                //}
                //else if (CarController.speed >= 80 && currentTrafficCarNameLeft == null && currentTrafficCarNameRight == null)
                //{
                //    trafficCar = col.gameObject.transform.GetComponent<HR_TrafficCar>();
                //}



            }
            //return;
        }
 
        //	If scene doesn't include gameplay manager, return.
        if (!HR_GamePlayHandler.Instance)
            return;

        if (!canCrash)
            return;

        if (crashed)
            return;
        if (gameWin)
            return;
        //dam vao dai phan cach
        if (col.gameObject.tag == "Railling" && !CarController.useNitro)
        {
            if (HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.MissionMode_1 || HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.MissionMode_2)
                GameFail();
            else if (HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.AIRacing)
                GamePause(true);
            else
                GameOver();
        }
        if (col.gameObject.tag == "TrafficCar")
        {
            isCollide = true;
        }
        //	Calculating collision impulse.
        float impulse = col.impulse.magnitude / 1000f;

        //	If impulse is below the limit, return.
        if (impulse < HR_HighwayRacerProperties.Instance._minimumCollisionForGameOver)
            return;

        // If hit is not a traffic car, return.
        if ((1 << col.gameObject.layer) != HR_HighwayRacerProperties.Instance.trafficCarsLayer.value)
            return;

        //  Increasinf damage.
        //damage += impulse * 2f;

        // Resetting combo to 0.
        combo = 0;

        // If mode is bomb mode, reduce the bomb health.
        if (HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.Bomb)
        {

            bombHealth -= impulse * 3f;
            return;

        }
        //if (damage >= 100f) {


        //	Game over.
        //if (RCC_SceneManager.Instance.activePlayerVehicle.useNitro)
        //return;
        //GameOver();
        //CheckStatus();

        //    //	Game over.
        //    if (RCC_SceneManager.Instance.activePlayerVehicle.useNitro)
        //        return;
        //    GameOver();


        //}

    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("FinishPoint"))
        {
            if (crashed) return;
            winSound = HR_CreateAudioSource.NewAudioSource(gameObject, "Win Sound", 0f, 0f, 1f, SoundCarManager.Instance.winSound, false, true, true);
            winSound.ignoreListenerPause = true;
            if (HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.AIRacing)
            {
                isCheckLostOrWin = true;
                GamePause(false);
                StartCoroutine(delayWinOrLost());
            }
            else
            {

                if (!gameWin)
                {
                    Debug.Log("Win Mission Mode");
                    //rigid.isKinematic = true;
                    GameWin();
                }
            }
        }

    }
    /// <summary>
    /// Checks position of the car. If exceeds limits, respawns it.
    /// </summary>
    private void CheckStatus()
    {

        if (Rigid.isKinematic)
            return;

        if (!HR_GamePlayHandler.Instance.gameStarted)
            return;

        //	If speed is below 5, or X position of the car exceeds limits, respawn it.
        if (speed < 0f || Mathf.Abs(transform.position.x) > 10f || Mathf.Abs(transform.position.y) > 10f)
        {

            transform.position = new Vector3(0f, 0, transform.position.z + 15f);
            transform.rotation = Quaternion.identity;
            Rigid.angularVelocity = Vector3.zero;
            Rigid.velocity = new Vector3(0f, 0f, 12f);

        }

    }
    public void Revive()
    {
        AudioListener.pause = false;
        crashed = false;
        CarController.canControl = true;
        CarController.engineRunning = true;
        Rigid.drag = 0.01f;
        HPS_Ads.Instance.leftBanner.SetActive(false);
        HR_GamePlayHandler.Instance.PLayerRevive(this);
        transform.position = new Vector3(0, transform.position.y + 0.2f, distanceCrashed.z);
        transform.rotation = Quaternion.identity;
        Rigid.angularVelocity = Vector3.zero;
        Rigid.velocity = new Vector3(0f, 0f, 12f);
        CarController.KillEngine();
        CarController.StartEngine();
        StartCoroutine(BlinkingEffect());
        if (timeLeft <= 0)
        {
            timeLeft += 15;
        }
    }
    IEnumerator BlinkingEffect()
    {
        //ShowEffectTrailLight(false);
        Array.ForEach(gameObject.transform.GetComponentsInChildren<MeshRenderer>(), meshRenderer => meshRenderer.enabled = false);
        yield return new WaitForSeconds(0.2f);
        Array.ForEach(gameObject.transform.GetComponentsInChildren<MeshRenderer>(), meshRenderer => meshRenderer.enabled = true);
        yield return new WaitForSeconds(0.2f);
        Array.ForEach(gameObject.transform.GetComponentsInChildren<MeshRenderer>(), meshRenderer => meshRenderer.enabled = false);
        yield return new WaitForSeconds(0.2f);
        Array.ForEach(gameObject.transform.GetComponentsInChildren<MeshRenderer>(), meshRenderer => meshRenderer.enabled = true);
        yield return new WaitForSeconds(0.2f);
        Array.ForEach(gameObject.transform.GetComponentsInChildren<MeshRenderer>(), meshRenderer => meshRenderer.enabled = false);
        yield return new WaitForSeconds(0.2f);
        Array.ForEach(gameObject.transform.GetComponentsInChildren<MeshRenderer>(), meshRenderer => meshRenderer.enabled = true);
        yield return new WaitForSeconds(0.2f);
        //ShowEffectTrailLight(true);

    }
    /// <summary>
    /// Game Over.
    /// </summary>
    public void GameOver()
    {
        Vibration.VibratePop();
        crashed = true;
        CarController.canControl = false;
        CarController.engineRunning = false;
        CarController.indicatorsOn = RCC_CarControllerV3.IndicatorsOn.All;
        Rigid.drag = 1f;
        int[] scores = new int[5];
        scores[0] = Mathf.FloorToInt(distance * 1000);
        scores[1] = Mathf.FloorToInt(score * 500);
        scores[2] = Mathf.FloorToInt(gold * 200);
        scores[3] = Mathf.FloorToInt(diamond * 1000);
        scores[4] = scores[0] + scores[1] + scores[2] + scores[3];

        //for (int i = 0; i < scores.Length - 1; i++)
        //    HR_API.AddCurrencyGold(scores[i]);
        HR_API.AddCurrencyGold(scores[4]);

        HR_API.AddCurrencyGold(gold);
        HR_API.AddCurrencyDiamond(diamond);
        HR_GamePlayHandler.Instance.CrashedPlayer(this, scores);
        //Debug.Log("viet_gameOver");
    }
    public void GamePause(bool isCheck)
    {
        if (!isCheck)
            HR_GamePlayHandler.Instance.gameStarted = false;
        else
        {
            Vibration.VibratePop();
        }
        crashed = true;
        CarController.canControl = false;
        CarController.engineRunning = false;
        CarController.indicatorsOn = RCC_CarControllerV3.IndicatorsOn.All;
        transform.rotation = Quaternion.identity;
        Rigid.angularVelocity = Vector3.zero;
        Rigid.velocity = new Vector3(0f, 0f, 12f);
        speed = 0;
        if (isCheck)
        {
            StartCoroutine(DelayCarDie());
            HR_GamePlayHandler.Instance.CrashedPlayerModDual();
        }
    }
    public void GameCoroutine()
    {
        crashed = false;
        HR_GamePlayHandler.Instance.gameStarted = true;
        CarController.canControl = true;
        CarController.engineRunning = true;
    }
    public void GameWin()
    {
        Vibration.VibratePop();
        gameWin = true;
        level += 1;
        CarController.canControl = false;
        CarController.engineRunning = false;
        CarController.indicatorsOn = RCC_CarControllerV3.IndicatorsOn.All;
        Rigid.drag = 1f;

        int[] scores = new int[5];
        if (HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.AIRacing)
            scores[0] = Mathf.FloorToInt(distance * 1000);
        else
            scores[0] = Mathf.FloorToInt(distanceWinMM * 1000);
        scores[1] = Mathf.FloorToInt(score * 500);
        scores[2] = Mathf.FloorToInt(gold * 200);
        scores[3] = Mathf.FloorToInt(diamond * 1000);
        scores[4] = scores[0] + scores[1] + scores[2] + scores[3];

        //for (int i = 0; i < scores.Length - 1; i++)
        //    HR_API.AddCurrencyGold(scores[i]);

        HR_API.AddCurrencyGold(scores[4]);

        HR_API.AddCurrencyGold(gold);
        HR_API.AddCurrencyDiamond(diamond);
        HR_GamePlayHandler.Instance.PlayerWon(this, scores);
    }
    public void GameFail()
    {
        Vibration.VibratePop();
        crashed = true;
        isCollide = false;
        CarController.canControl = false;
        CarController.engineRunning = false;
        CarController.indicatorsOn = RCC_CarControllerV3.IndicatorsOn.All;
        Rigid.drag = 1f;
        distanceCrashed = transform.position;

        int[] scores = new int[5];
        HR_GamePlayHandler.Instance.PLayerFailed(this, scores);
    }
    /// <summary>
    /// Eliminates ground gap distance on when spawned.
    /// </summary>
    private void CheckGroundGap()
    {

        WheelCollider wheel = GetComponentInChildren<WheelCollider>();
        float distancePivotBetweenWheel = Vector3.Distance(new Vector3(0f, transform.position.y, 0f), new Vector3(0f, wheel.transform.position.y, 0f));

        RaycastHit hit;

        if (Physics.Raycast(wheel.transform.position, -Vector3.up, out hit, 10f))
            transform.position = new Vector3(transform.position.x, hit.point.y + distancePivotBetweenWheel + (wheel.radius / 1f) + (wheel.suspensionDistance / 2f), transform.position.z);

    }

    private void Reset()
    {

        currentEngineTorque = CarController.maxEngineTorque;
        currentBrakeTorque = CarController.brakeTorque;
        currentHandlingStrength = CarController.steerHelperAngularVelStrength;
        currentSpeed = CarController.maxspeed;
        currentNOS = CarController.NoS;

        maxEngineTorque = currentEngineTorque + 50;
        maxBrakeTorque = currentBrakeTorque + 500;
        maxHandlingStrength = currentHandlingStrength + .2f;
        maxSpeed = currentSpeed + 40f;
        maxNOS = currentNOS + 20f;

    }

    public string showTimeLeft(bool isTimeFormat)
    {
        int minutes = Mathf.FloorToInt(timeLeft / 60f);
        int seconds = Mathf.FloorToInt(timeLeft % 60f);
        int milliseconds = Mathf.FloorToInt((timeLeft * 100f) % 100f);
        string timeFormatted;
        if (isTimeFormat)
        {
            timeFormatted = string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, milliseconds);
        }
        else
        {
            timeFormatted = string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        return timeFormatted;
    }
}
