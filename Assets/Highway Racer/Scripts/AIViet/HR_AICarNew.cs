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
using UnityEngine.UIElements;
using Unity.VisualScripting;
using TMPro;
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
public class HR_AICarNew : MonoBehaviour
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
    #region HPS_TrafficPooling Instance

    private HPS_TrafficPooling HR_TrafficPoolingInstance;
    private HPS_TrafficPooling HR_TrafficPooling
    {
        get
        {
            if (HR_TrafficPoolingInstance == null)
            {
                HR_TrafficPoolingInstance = HPS_TrafficPooling.Instance;
            }
            return HR_TrafficPoolingInstance;
        }
    }

    #endregion

    public bool canCrash = true;
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

    internal float timeLeft = 0;     //	Time left.
    internal int combo;     //	Current near miss combo.
    internal int maxCombo;      //	Highest combo count.
    internal float distanceToNextPlayer = -9999f;     //	Time left.

    internal float speed = 0f;      //  Current speed.
    internal float distance = 0f;       //  Total distance traveled.
    internal float distanceUI = 0f;       //  Total distance traveled.
    internal float highSpeedCurrent = 0f;       //  Current high speed time.
    internal float highSpeedTotal = 0f;     //  Total high speed time.
    internal float opposideDirectionCurrent = 0f;       //  Current opposite direction time.
    internal float opposideDirectionTotal = 0f;     //  Total opposite direction time.
    internal int rams;        //  Total rams.
    internal int nearMisses;        //  Total near misses.
    private float comboTime;        //  Combo time for near misses.
    public Vector3 previousPosition;       //  Previous position used to calculate total traveled distance.


    //mission mode
    [SerializeField] private GameObject checkPointMissionMode;
    internal float distanceWinMM = 0.1f;
    internal int level = 1;
    internal bool gameWin = false;
    internal bool gameOver = false;

    

    public string currentTrafficCarNameLeft = null;
    public string currentTrafficCarNameRight = null;

    public string currentTrafficCarNameBack = null;

    internal bool bombTriggered = false;
    internal float bombHealth = 100f;

    private AudioSource hornSource;

    public delegate void onNearMiss(HR_AICarNew player, int score, HR_UIDynamicScoreDisplayer.Side side);
    public static event onNearMiss OnNearMiss;

    public delegate void onDestroyTrafficCar(HR_AICarNew player, int score, HR_UIDynamicScoreDisplayer.Side side);
    public static event onDestroyTrafficCar OnDestroyTrafficCar;
    bool isCollide = false;
    [SerializeField] GameObject[] fxNitro;
    private float resetTime = 0f;
    //dev viet
    float timeRun;
    public bool isCheckLostOrWin;
    internal int gold;
    internal int diamond;
    //Obstacle
    public LayerMask layerMaskObstacle;
    public Transform tranformCheckObstacle;
    public Transform tranformCheckObstacleLeft;
    public Transform tranformCheckObstacleRight;
    public bool isCheckObstacle;
    public float speedAI;
    bool isCheckChangeLine = false;
    int numberChange;
    public int currentLine = 0;
    bool isCheckObstacleLeft;
    bool isCheckObstacleRight;
    bool isCheckColiderAI = false;
    public bool isCheckStart = false;
    HPS_TrafficCar trafficCar;
    public float friction;
    public Collider[] colider;
    public Transform tranformRaycast;
    public LayerMask layer;
    private HR_PlayerHandler playerHandler;
    public UnityEngine.UI.Image flag;
    public TextMeshProUGUI name;
    public DataCarAI dataCar;
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

        WheelCollider[] wheelColliders = GetComponentsInChildren<WheelCollider>(true);

        foreach (WheelCollider item in wheelColliders)
            item.forceAppPointDistance = .15f;
        flag.sprite = dataCar.dataCarAi[PlayerPrefs.GetInt("NumberDataAI")].nationalFlag;
        name.text = dataCar.dataCarAi[PlayerPrefs.GetInt("NumberDataAI")].fullName[PlayerPrefs.GetInt("NumberDataAIName")];
        //Mission Mode
        //HR_GamePlayHandler.Instance.mode = HR_GamePlayHandler.Mode.MissionMode_2;

    }
    private void Awake()
    {
        CarController.isCheckAi = true;
        StartCoroutine(DelayStart());
    }
    IEnumerator DelayStart()
    {
        yield return new WaitForSeconds(4f);
        isCheckStart = true;
    }
    private void Start()
    {
        
    }
    private void Update()
    {
        if (Physics.Raycast(tranformRaycast.position, Vector3.right, out RaycastHit hitRinghtBefore, 10f, layer) && playerHandler == null)
        {
            playerHandler = hitRinghtBefore.collider.GetComponentInParent<HR_PlayerHandler>();
            for (int i = 0; i < colider.Length; i++)
            {
                for (int j = 0; j < playerHandler.colider.Length; j++)
                {
                    Physics.IgnoreCollision(colider[i], playerHandler.colider[j]);
                }
            }
        }
        if (transform.position.y == 0)
            transform.position = new Vector3(transform.position.x,0.65f,transform.position.z);
        if(transform.rotation.y > 40f ||  transform.rotation.y < -40f)
            transform.rotation = Quaternion.identity;
        if (!isCheckStart)
            return;
#if PHOTON_UNITY_NETWORKING && BCG_HR_PHOTON

        if (photonView && !photonView.IsMine)
            return;

#endif      
        
        distance += Vector3.Distance(previousPosition, transform.position) / 1000f;
        previousPosition = transform.position;
        distanceUI = distance - 0.07f;

        if (HR_GamePlayHandler.Instance.isCheckAIBefore && !HR_GamePlayHandler.Instance.isCheckAIAfter && HR_GamePlayHandler.Instance.distancePlayerBetweenAI > 30)
        {
            CarController.throttleInputAI = 0.3f;
        }
        else if (HR_GamePlayHandler.Instance.isCheckAIBefore && !HR_GamePlayHandler.Instance.isCheckAIAfter && HR_GamePlayHandler.Instance.distancePlayerBetweenAI < 30)
        {
            if (isCheckObstacle)
                CarController.throttleInputAI = 0;
            else
                CarController.throttleInputAI = 0.5f;

        }
        else if (!HR_GamePlayHandler.Instance.isCheckAIBefore && HR_GamePlayHandler.Instance.isCheckAIAfter && HR_GamePlayHandler.Instance.distancePlayerBetweenAI < 45)
        {
            if (CarController.speed > 100)
            {
                CarController.throttleInputAI = 0;
                //CarController.brakeInput = 1;
            }
            else
                CarController.throttleInputAI = 1f;
            HR_GamePlayHandler.Instance.isCheckNitro = false;
        }
        else if (!HR_GamePlayHandler.Instance.isCheckAIBefore && HR_GamePlayHandler.Instance.isCheckAIAfter && HR_GamePlayHandler.Instance.distancePlayerBetweenAI >= 45)
        {
            CarController.throttleInputAI = 1f;
            HR_GamePlayHandler.Instance.isCheckNitro = true;
        }




        if (!isCheckColiderAI)
        {
            RayCastCheckObstacle();
            RayCastCheckObstacleLeftOrRight();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            isCheckObstacle = true;
            isCheckChangeLine = true;
        }





        if (isCheckObstacle)
        {
            currentLine = Mathf.Clamp(currentLine, 0, 3);
            if (currentLine == 0)
            {
                CarController.steerInputAI = Mathf.MoveTowards(CarController.steerInputAI, CarController.steerInputAI + 1f, 0.5f);
                if (transform.position.x >= HR_TrafficPooling.lines[currentLine + 1].position.x - friction)
                {
                    isCheckObstacle = false;
                    currentLine++;
                }
            }
            else if (currentLine > 0 && currentLine < 3)
            {
                if (isCheckChangeLine)
                {
                    numberChange = Random.Range(0, 100);
                    isCheckChangeLine = false;
                }
                int number = numberChange % 2;
                switch (number)
                {
                    case 0:

                        CarController.steerInputAI = Mathf.MoveTowards(CarController.steerInputAI, CarController.steerInputAI + 1f, 0.5f);
                        if (transform.position.x >= HR_TrafficPooling.lines[currentLine + 1].position.x - friction)
                        {
                            isCheckObstacle = false;
                            currentLine++;
                        }
                        break;
                    case 1:

                        CarController.steerInputAI = Mathf.MoveTowards(CarController.steerInputAI, CarController.steerInputAI - 1f, 0.5f);
                        if (transform.position.x <= HR_TrafficPooling.lines[currentLine - 1].position.x + friction)
                        {
                            isCheckObstacle = false;
                            currentLine--;

                        }
                        Debug.Log("sang trai");

                        break;
                }
            }
            else if (currentLine == 3)
            {
                CarController.steerInputAI = Mathf.MoveTowards(CarController.steerInputAI, CarController.steerInputAI - 1f, 0.5f);
                if (transform.position.x <= HR_TrafficPooling.lines[currentLine - 1].position.x + friction)
                {
                    isCheckObstacle = false;
                    currentLine--;

                }
            }
        }

        if (isCheckObstacleLeft)
        {
            var numberRight = currentLine + 1;
            if (numberRight > 3)
                numberRight = 3;
            CarController.steerInputAI = Mathf.MoveTowards(CarController.steerInputAI, CarController.steerInputAI + 1f, 0.5f);
            if (transform.position.x >= HR_TrafficPooling.lines[numberRight].position.x - friction)
            {
                isCheckObstacleLeft = false;
                currentLine++;
                if (currentLine > 3)
                    currentLine = 3;
            }
        }
        if (isCheckObstacleRight)
        {
            var numberLeft = currentLine - 1;
            if (numberLeft < 0)
                numberLeft = 0;
            CarController.steerInputAI = Mathf.MoveTowards(CarController.steerInputAI, CarController.steerInputAI - 1f, 0.5f);
            if (transform.position.x <= HR_TrafficPooling.lines[numberLeft].position.x + friction)
            {
                isCheckObstacleRight = false;
                currentLine--;
                if (currentLine < 0)
                    currentLine = 0;
            }
        }
    }


    private void FixedUpdate()
    {


#if PHOTON_UNITY_NETWORKING && BCG_HR_PHOTON

        if (photonView && !photonView.IsMine)
            return;

#endif

        //	If scene doesn't include gameplay manager, return.

        //	If game is started, check near misses with raycasts.
        //if (!crashed && HR_GamePlayHandler.Instance.gameStarted)
        //{
        //    CheckNearMiss();
        //}
        if(!isCheckColiderAI)
        {
            if (CarController.useNitro)
            {
                CheckFreeRam(CarController.COM.position + (transform.up * 0.5f), 3.5f, 35, 3);
                Debug.Log("viet_useNitroAI");
            }
        }
        CheckNearMiss();



    }

   
   
    /// <summary>
    /// Checks near vehicles by drawing raycasts to the left and right sides.
    /// </summary>
    private void CheckNearMiss()
    {

        RaycastHit hit;

        Debug.DrawRay(tranformRaycast.position, (Quaternion.Euler(0, 45f, 0) * -transform.right * 1.5f), Color.white);
        Debug.DrawRay(tranformRaycast.position, (Quaternion.Euler(0, -45f, 0) * transform.right * 1.5f), Color.white);
        // Raycasting to the left side
        if (//Physics.Raycast(tranformRaycast.position, (transform.right), out hit, 1f, layer) 
             Physics.Raycast(tranformRaycast.position, (Quaternion.Euler(0, -45f, 0) * transform.right), out hit, 1.5f, layer))
        {
            currentTrafficCarNameRight = hit.transform.name;
        }
        else
        {
            currentTrafficCarNameRight = null;

        }
        if (//Physics.Raycast(tranformRaycast.position, (-transform.right), out hit, 1f, layer)
             Physics.Raycast(tranformRaycast.position, (Quaternion.Euler(0, -45f, 0) * -transform.right), out hit, 1.5f, layer))
        {
            currentTrafficCarNameLeft = hit.transform.name;
        }
        else
        {
            currentTrafficCarNameLeft = null;

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
                        switch (currentAngle)
                        {
                            case var v when currentAngle < 330:
                                OnDestroyTrafficCar(this, 5, HR_UIDynamicScoreDisplayer.Side.Left);
                                break;
                            case var v when currentAngle > 30:
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

        Debug.DrawRay(CarController.COM.position, (transform.forward * 10f), Color.white);
        if (Physics.Raycast(CarController.COM.position, (transform.forward), out hit, 10f, HR_HighwayRacerProperties.Instance.trafficCarsLayer) && !hit.collider.isTrigger)
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
    IEnumerator DelayCarDie()
    {
        GamePause();
        yield return new WaitForSeconds(1);
        GameCoroutine();
        gameObject.transform.GetChild(0).gameObject.SetActive(false);
        gameObject.transform.GetChild(1).GetComponent<MeshRenderer>().enabled = false;
        yield return new WaitForSeconds(0.2f);
        gameObject.transform.GetChild(0).gameObject.SetActive(true);
        gameObject.transform.GetChild(1).GetComponent<MeshRenderer>().enabled = true;
        yield return new WaitForSeconds(0.2f);
        gameObject.transform.GetChild(0).gameObject.SetActive(false);
        gameObject.transform.GetChild(1).GetComponent<MeshRenderer>().enabled = false;
        yield return new WaitForSeconds(0.2f);
        gameObject.transform.GetChild(0).gameObject.SetActive(true);
        gameObject.transform.GetChild(1).GetComponent<MeshRenderer>().enabled = true;
        yield return new WaitForSeconds(0.2f);
        gameObject.transform.GetChild(0).gameObject.SetActive(false);
        gameObject.transform.GetChild(1).GetComponent<MeshRenderer>().enabled = false;
        yield return new WaitForSeconds(0.2f);
        gameObject.transform.GetChild(0).gameObject.SetActive(true);
        gameObject.transform.GetChild(1).GetComponent<MeshRenderer>().enabled = true;
        isCheckColiderAI = false;
        yield return new WaitForSeconds(2f);
        trafficCar.bodyCollider.isTrigger = false;

    }
    float timeRunIsTrigger;
    private void OnCollisionEnter(Collision col)
    {

#if PHOTON_UNITY_NETWORKING && BCG_HR_PHOTON

        if (photonView && !photonView.IsMine)
            return;

#endif        
        if(col.gameObject.tag == "TrafficCar" && !HR_GamePlayHandler.Instance.isCheckTimeAI)
        {
            
            if(currentTrafficCarNameRight != null && currentTrafficCarNameLeft != null) // cả 2 cùng chạm thì phải cho xe die khong sẽ xảy ra hiện tượng animation của AI bị nhảy cùng hướng
            {
                isCheckColiderAI = true;
                trafficCar = col.gameObject.transform.GetComponent<HPS_TrafficCar>();
                StartCoroutine(DelayCarDie());
            }
            isCheckColiderAI = true;
            trafficCar = col.gameObject.transform.GetComponent<HPS_TrafficCar>();
            StartCoroutine(DelayCarDie());

        }
    }
    float timeRunAI;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("FinishPoint"))
        {
            if(!HR_GamePlayHandler.Instance.isCheckTimeAI)
            {
                HR_GamePlayHandler.Instance.isCheckTimeAI = true;
                GameWin();
            }
            
        }

    }
    /// <summary>
    /// Checks position of the car. If exceeds limits, respawns it.
    /// </summary>
    
    public void Revive()
    {
        crashed = false;
        CarController.canControl = true;
        CarController.engineRunning = true;
        Rigid.drag = 0.01f;
        HR_GamePlayHandler.Instance.PLayerRevive(this);
        transform.position = new Vector3(transform.position.x, 2.5f, transform.position.z + 15f);
        transform.rotation = Quaternion.identity;
        Rigid.angularVelocity = Vector3.zero;
        Rigid.velocity = new Vector3(0f, 0f, 12f);
        StartCoroutine(BlinkingEffect());
    }
    IEnumerator BlinkingEffect()
    {
        gameObject.transform.GetChild(0).gameObject.SetActive(false);
        gameObject.transform.GetChild(1).GetComponent<MeshRenderer>().enabled = false;
        yield return new WaitForSeconds(0.2f);
        gameObject.transform.GetChild(0).gameObject.SetActive(true);
        gameObject.transform.GetChild(1).GetComponent<MeshRenderer>().enabled = true;
        yield return new WaitForSeconds(0.2f);
        gameObject.transform.GetChild(0).gameObject.SetActive(false);
        gameObject.transform.GetChild(1).GetComponent<MeshRenderer>().enabled = false;
        yield return new WaitForSeconds(0.2f);
        gameObject.transform.GetChild(0).gameObject.SetActive(true);
        gameObject.transform.GetChild(1).GetComponent<MeshRenderer>().enabled = true;
        yield return new WaitForSeconds(0.2f);
        gameObject.transform.GetChild(0).gameObject.SetActive(false);
        gameObject.transform.GetChild(1).GetComponent<MeshRenderer>().enabled = false;
        yield return new WaitForSeconds(0.2f);
        gameObject.transform.GetChild(0).gameObject.SetActive(true);
        gameObject.transform.GetChild(1).GetComponent<MeshRenderer>().enabled = true;
    }
    /// <summary>
    /// Game Over.
    /// </summary>
    public void GamePause()
    {
        isCheckStart = false;
        CarController.engineRunning = false;
        trafficCar.bodyCollider.isTrigger = true;
        transform.position = new Vector3(transform.position.x, 1f, transform.position.z);
        transform.rotation = Quaternion.identity;
        Rigid.angularVelocity = Vector3.zero;
        Rigid.velocity = new Vector3(0f, 0f, 12f);
    }
    public void GameCoroutine()
    {
        CarController.engineRunning = true;
        isCheckStart = true;

    }
    public void GameWin()
    {
        gameWin = true;
        CarController.engineRunning = false;
        Rigid.drag = 1f;
    }
    public void GameFail(bool isFailedTime)
    {
        crashed = true;
        isCollide = false;
        CarController.canControl = false;
        CarController.engineRunning = false;
        CarController.indicatorsOn = RCC_CarControllerV3.IndicatorsOn.All;
        Rigid.drag = 1f;

        int[] scores = new int[5];
        scores[0] = Mathf.FloorToInt(distance * 1000);
        scores[1] = Mathf.FloorToInt(nearMisses * 500);
        scores[2] = Mathf.FloorToInt(gold * 500);
        scores[3] = Mathf.FloorToInt(diamond * 1000);

        if (!isFailedTime)
        {
            HR_GamePlayHandler.Instance.PLayerFailed(this, scores);
        }
        else
        {
            HR_GamePlayHandler.Instance.PLayerFailedTime(this, scores);
        }
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
    bool isCheckRayCast = false;

    public void RayCastCheckObstacle()
    {
        if(!isCheckRayCast)
        {
            Debug.DrawRay(tranformCheckObstacle.position, Vector3.forward * 15f, Color.green);
            Debug.DrawRay(tranformCheckObstacleLeft.position, Vector3.forward * 7f, Color.green);
            Debug.DrawRay(tranformCheckObstacleRight.position, Vector3.forward * 7f, Color.green);

            if (Physics.Raycast(tranformCheckObstacle.position, Vector3.forward, out RaycastHit hitInfo, 15, layerMaskObstacle))
            {
                if(hitInfo.collider.tag == "TrafficCar")
                {
                    // Log the name of the hit object
                    Debug.Log("Hit object: " + hitInfo.collider.name);
                    Debug.Log("Có chứng ngại vật ");
                    isCheckRayCast = true;
                    isCheckObstacle = true;
                    isCheckChangeLine = true;
                    StartCoroutine(DelayCheckBoolen());
                }
                
            }
            if (Physics.Raycast(tranformCheckObstacleLeft.position, Vector3.forward, out RaycastHit hitInfoLeft, 7, layerMaskObstacle))
            {
                if (hitInfoLeft.collider.tag == "TrafficCar")
                {
                    // Log the name of the hit object
                    Debug.Log("Hit object: " + hitInfoLeft.collider.name);
                    Debug.Log("Có chứng ngại vật ");
                    isCheckObstacleLeft = true;
                    isCheckRayCast = true;
                    StartCoroutine(DelayCheckBoolen());
                }
                    
            }
            if (Physics.Raycast(tranformCheckObstacleRight.position, Vector3.forward, out RaycastHit hitInfoRight, 7, layerMaskObstacle))
            {
                if (hitInfoRight.collider.tag == "TrafficCar")
                {
                    // Log the name of the hit object
                    Debug.Log("Hit object: " + hitInfoRight.collider.name);
                    Debug.Log("Có chứng ngại vật ");
                    isCheckObstacleRight = true;
                    isCheckRayCast = true;
                    StartCoroutine(DelayCheckBoolen());
                }
                
            }

        }
            
    }
    public void RayCastCheckObstacleLeftOrRight()
    {
        
       
        
    }
    IEnumerator DelayCheckBoolen()
    {
        yield return new WaitForSeconds(1);
        isCheckRayCast = false;
        isCheckObstacleRight = false;
        isCheckObstacleLeft = false;
    }
}
