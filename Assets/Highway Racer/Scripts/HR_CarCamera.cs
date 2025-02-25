//----------------------------------------------
//           	   Highway Racer
//
// Copyright © 2014 - 2023 BoneCracker Games
// http://www.bonecrackergames.com
//
//----------------------------------------------

using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

/// <summary>
/// Car camera with three modes.
/// </summary>
[AddComponentMenu("BoneCracker Games/Highway Racer/Camera/HR Car Camera")]
public class HR_CarCamera : MonoBehaviour
{
    private static HR_CarCamera instance;
    public static HR_CarCamera Instance { get { return instance; } }
    public CameraMode cameraMode = CameraMode.TPS;       //  Camera modes.
    public enum CameraMode { TPS, Top, FPS, FPS1, FPS2 }

    internal int cameraSwitchCount = 0; //  Current camera mode as int.

    private RCC_HoodCamera hoodCam;     //  Hood camera transform.
    private RCC_HoodCamera CamFps1, CamFps2, cameraBack;
    private RCC_HoodCamera[] hoodCams;
    private float targetFieldOfView = 50f;      //  Field of the camera will be adapted to this value.
    public float topFOV = 48f;      //  FOV for top mode.
    public float tpsFOV = 55f;      //  FOV for tps mode.
    public float fpsFOV = 65f;      //  FOV for fps mode.

    // The target we are following.
    public Transform playerCar;
    public ParticleSystem speedEffects;
    private Rigidbody playerRigid;

    public bool gameover = false;
    public bool isCheckColiderPlayerWithCar = false;
    private Camera cam;     //  Actual camera.
    private Vector3 targetPosition = new Vector3(0, 0, 50);     //  Target position.
    private Vector3 pastFollowerPosition, pastTargetPosition;       //  Past position.

    // The distance in the x-z plane to the target
    public float distance = 12f;

    // The height we want the camera to be above the target
    public float height = 8.5f;

    //  X Rotation of the camera.
    public float rotation = 30f;

    private float currentT;
    private float oldT;

    private float speed = 0f;

    private GameObject mirrors;
    //Time Lerp Camera
    float timeMLerp = 4f;
    float timeLerp;
    float timeGameOver;
    float timeGameDie;
    private bool isCameraMovingBack = false;
    private bool isDelayCamera = false;

    public ParticleSystem windEffect;
    public ParticleSystem rainEffect;
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        //  Getting camera component.
        cam = GetComponentInChildren<Camera>();

        //  Setting very first position and rotation of the camera (before the intro animation).
        transform.position = new Vector3(2f, 1f, 55f);
        transform.rotation = Quaternion.Euler(new Vector3(0f, -40f, 0f));
        if(PlayerPrefs.GetString("SelectedScene") == "HighwayRainy")
        {
            rainEffect.Play();
        }
        else
        {
            rainEffect.Stop();
        }
    }

    private void OnEnable()
    {

        //  Listening events when player spawns, dies, and change cameras.
        HR_GamePlayHandler.OnPlayerSpawned += OnPlayerSpawned;
        HR_GamePlayHandler.OnPlayerDied += OnPlayerCrashed;
        HR_GamePlayHandler.OnPlayerFailed += OnPlayerFailed;
        HR_GamePlayHandler.OnPlayerRevive += OnPlayerRevive;
        RCC_InputManager.OnChangeCamera += RCC_InputManager_OnChangeCamera;
        HR_GamePlayHandler.OnPauseModeDual += OnPauseModeAI;
    }

    /// <summary>
    /// When player changes the camera mode.
    /// </summary>
    private void RCC_InputManager_OnChangeCamera()
    {

        ChangeCamera();

    }

    /// <summary>
    /// When player spawns, set player variable and get hood and tps camera locations.
    /// </summary>
    /// <param name="player"></param>
    private void OnPlayerSpawned(HR_PlayerHandler player)
    {

        playerCar = player.transform;
        playerRigid = player.Rigid;
        //hoodCam = player.GetComponentInChildren<RCC_HoodCamera>();
        hoodCams = player.GetComponentsInChildren<RCC_HoodCamera>();
        hoodCam = hoodCams[0];
        CamFps1 = hoodCams[1];
        CamFps2 = hoodCams[2];
        cameraBack = hoodCams[3];
        //if (GameObject.Find("Mirrors"))
        //    mirrors = GameObject.Find("Mirrors").gameObject;

    }

    /// <summary>
    /// When player dies.
    /// </summary>
    /// <param name="player"></param>
    private void OnPauseModeAI()
    {
        gameover = true;
        timeGameDie = 0;
        timeGameOver = 0;
        TakeDamageEffect.Instance.isEnable = false;
    }
    private void OnPlayerCrashed(HR_PlayerHandler player, int[] scores)
    {
        gameover = true;
        timeGameDie = 0;
        timeGameOver = 0;
        TakeDamageEffect.Instance.isEnable = false;

    }
    private void OnPlayerFailed(HR_PlayerHandler player, int[] scores)
    {
        if (player.timeLeft > 0)
        {
            gameover = true;
            timeGameDie = 0;
            timeGameOver = 0;
            TakeDamageEffect.Instance.isEnable = false;
        }

    }
    private void OnPlayerRevive(HR_PlayerHandler player)
    {

        gameover = false;
        timeGameOver = 0;
        timeGameDie = 0;
        TakeDamageEffect.Instance.isEnable = false;

    }
    private void LateUpdate()
    {

        //  If no player car found yet, return.
        if (!playerCar)
            return;
        //Debug.Log("gameOver " + gameover);
        //  Animating the camera at spawn sequence.
        if (!playerCar || !playerRigid || Time.timeSinceLevelLoad < 1.5f)
        {

            transform.position += Quaternion.identity * Vector3.forward * (Time.deltaTime * 3f);

        }
        else if (playerCar && playerRigid)
        {

            //  Setting FOV of the camera.
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFieldOfView, Time.deltaTime * 3f);

            //  Running corresponding method with current camera mode.
            if (!gameover)
            {
                //if (!isCameraMovingBack)
                //{
                    switch (cameraMode)
                    {

                        case CameraMode.Top:

                            TOP();

                            if (mirrors)
                                mirrors.SetActive(false);

                            break;

                        case CameraMode.TPS:

                            TPS();

                            if (mirrors)
                                mirrors.SetActive(false);

                            break;

                        case CameraMode.FPS1:
                        case CameraMode.FPS2:
                            FPS();

                            if (mirrors)
                                mirrors.SetActive(true);

                            break;                        

                        case CameraMode.FPS:

                            if (hoodCam)
                            {

                                FPS();

                                if (mirrors)
                                    mirrors.SetActive(true);

                            }
                            else
                            {

                                cameraSwitchCount++;
                                ChangeCamera();

                            }

                            break;


                    //}
                }
            }
            else
            {
                //transform.LookAt(playerCar.transform);
                //targetFieldOfView = Mathf.Lerp(targetFieldOfView, Mathf.Lerp(50f, 10f, Mathf.InverseLerp(0f, 30f, Vector3.Distance(transform.position, playerCar.transform.position))), Time.deltaTime * 3f);

                //if (Vector3.Distance(transform.position, playerCar.transform.position) > 30f) {

                //    transform.position = playerCar.transform.position;
                //    transform.position += Vector3.forward * (20f * Mathf.Sign(playerCar.InverseTransformDirection(playerRigid.velocity).z));
                //    transform.position += Vector3.up * 2f;
                //    transform.position += Vector3.right * Random.Range(-2f, 2f);

                //}
                if (!HR_GamePlayHandler.Instance.player.isCheckLostOrWin)
                    StartCoroutine(TakeDamageEffect.Instance.takeDamageEffect(true));

                if (HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.AIRacing && !HR_GamePlayHandler.Instance.player.isCheckLostOrWin)
                {
                    StartCoroutine(delayCam());
                }
                else if (HR_GamePlayHandler.Instance.mode != HR_GamePlayHandler.Mode.AIRacing)
                {
                    timeGameOver += Time.deltaTime;
                    if (timeGameOver < 0.3f)
                    {
                        if (cameraMode == CameraMode.TPS)
                        {
                            timeLerp += Time.deltaTime;
                            Vector3 targetPosition_1 = new Vector3(transform.position.x, 1, hoodCam.transform.position.z + 75);
                            transform.position = Vector3.Lerp(transform.position, targetPosition_1, timeLerp / timeMLerp);
                        }
                        else
                        {
                            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + Mathf.Clamp(currentT, 0f, Mathf.Infinity));
                        }
                    }
                }
            }

            //  Setting proper camera mode with int.
            switch (cameraSwitchCount)
            {

                case 0:
                    cameraMode = CameraMode.TPS;
                    break;
                case 1:
                    cameraMode = CameraMode.FPS1;
                    break;
                case 2:
                    cameraMode = CameraMode.FPS2;
                    break;
                case 3:
                    cameraMode = CameraMode.FPS;
                    break;

            }

        }

        pastFollowerPosition = transform.position;
        pastTargetPosition = targetPosition;

        currentT = (transform.position.z - oldT);
        oldT = transform.position.z;

    }
    IEnumerator delayCam()
    {

        timeGameOver += Time.deltaTime;
        if (timeGameOver < 0.3f)
        {
                timeLerp += Time.deltaTime;
                Vector3 targetPosition_1 = new Vector3(transform.position.x, 1, hoodCam.transform.position.z + 75);
                transform.position = Vector3.Lerp(transform.position, targetPosition_1, timeLerp / timeMLerp);
        }
        yield return new WaitForSeconds(1f);
        timeGameDie += Time.deltaTime;
        if (timeGameDie < 1f)
        {
                timeLerp += Time.deltaTime;
                Vector3 targetPosition_1 = new Vector3(transform.position.x, transform.position.y, hoodCam.transform.position.z - 25);
                transform.position = Vector3.Lerp(transform.position, targetPosition_1, timeLerp / timeMLerp);
                gameover = false;
                Debug.Log("viet_cam quay lai");
        }

    }

    /// <summary>
    /// Changes the camera mode.
    /// </summary>
    public void ChangeCamera()
    {

        cameraSwitchCount++;

        if (cameraSwitchCount >= 4)
            cameraSwitchCount = 0;

    }

    /// <summary>
    /// Top camera mode.
    /// </summary>
    private void TOP()
    {

        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(rotation, 0f, 0f), Time.deltaTime * 2f);

        targetPosition = new Vector3(0f, playerCar.position.y, playerCar.position.z);
        targetPosition -= transform.rotation * Vector3.forward * distance;
        targetPosition = new Vector3(targetPosition.x, height, targetPosition.z);

        //if (Time.timeSinceLevelLoad < 3f)
        //    transform.position = SmoothApproach(pastFollowerPosition, pastTargetPosition, targetPosition, (speed / 2f) * Mathf.Clamp(Time.timeSinceLevelLoad - 1.5f, 0f, 10f));
        //else
        transform.position = targetPosition;

        targetFieldOfView = topFOV;
        rotation = 20;
    }

    /// <summary>
    /// TPS camera mode.
    /// </summary>
    private void TPS()
    {

        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(rotation / 6f, 0f, 0f), Time.deltaTime * 2f);
        targetPosition = new Vector3(playerCar.position.x, height / 3f, playerCar.position.z - (distance / 1.75f));
        if (gameover)
        {
            return;
        }
        if (Vector3.Distance(transform.position, targetPosition) > 0.1f)
            transform.position = SmoothApproach(pastFollowerPosition, pastTargetPosition, targetPosition, (speed / 2f) * Mathf.Clamp(Time.timeSinceLevelLoad - 1.5f, 0f, 10f));
        else
            transform.position = targetPosition;

        targetFieldOfView = tpsFOV;
        rotation = -10;
    }

    /// <summary>
    /// FPS camera mode.
    /// </summary>
    private void FPS()
    {

        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.identity, Time.deltaTime * 2f);

        if (HR_HighwayRacerProperties.Instance._tiltCamera)
            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.InverseTransformDirection(playerRigid.velocity).x / 2f, -transform.InverseTransformDirection(playerRigid.velocity).x / 2f);
        if(cameraMode == CameraMode.FPS)
            targetPosition = hoodCam.transform.position;
        else if(cameraMode == CameraMode.FPS1)
            targetPosition = CamFps1.transform.position;
        else if(cameraMode == CameraMode.FPS2)
            targetPosition = CamFps2.transform.position;
        //if (!isCameraMovingBack)
            transform.position = targetPosition;
        //else
        //    transform.position = Vector3.Lerp(transform.position, targetPosition, 1f * Time.deltaTime);
        targetFieldOfView = fpsFOV;
        rotation = -10;
    }

    /// <summary>
    /// Used for smooth position lerping.
    /// </summary>
    /// <param name="pastPosition"></param>
    /// <param name="pastTargetPosition"></param>
    /// <param name="targetPosition"></param>
    /// <param name="delta"></param>
    /// <returns></returns>
    private Vector3 SmoothApproach(Vector3 pastPosition, Vector3 pastTargetPosition, Vector3 targetPosition, float delta)
    {

        if (Time.timeScale == 0 || float.IsNaN(delta) || float.IsInfinity(delta) || delta == 0 || pastPosition == Vector3.zero || pastTargetPosition == Vector3.zero || targetPosition == Vector3.zero)
            return transform.position;

        float t = (Time.deltaTime * delta) + .00001f;
        Vector3 v = (targetPosition - pastTargetPosition) / t;
        Vector3 f = pastPosition - pastTargetPosition + v;
        Vector3 l = targetPosition - v + f * Mathf.Exp(-t);

        if (l != Vector3.negativeInfinity && l != Vector3.positiveInfinity && l != Vector3.zero)
            return l;
        else
            return transform.position;

    }

    private void FixedUpdate()
    {

        //  If no player rigid found yet, return.
        if (!playerRigid)
            return;

        //  Getting speed of the car.
        speed = Mathf.Lerp(speed, (playerRigid.velocity.magnitude * 3.6f), Time.deltaTime * 1.5f);

    }

    public void CameraBack(HR_PlayerHandler player = null)
    {
        if (!isDelayCamera)
            StartCoroutine(CameraDestroyTrafficCar(player));
    }

    IEnumerator CameraDestroyTrafficCar(HR_PlayerHandler player = null)
    {
        float newFOV;
        if (player != null && player.CarController.useNitro)
        {
            speedEffects.gameObject.SetActive(true);
            newFOV = 20;
        }
        else
            newFOV = 100;

        float tempFOV = fpsFOV;
        isDelayCamera = true;
        if (cameraMode == CameraMode.TPS)
        {
            tempFOV = tpsFOV;
            tpsFOV += newFOV;
            StartCoroutine(MoveSpeedEffects());
        }
        else
        {
            //if (player == null)
            //{
            //    isCameraMovingBack = true;

            //    Vector3 targetPos = cameraBack.transform.position;
            //    transform.position = targetPos;
            //    tempFOV = fpsFOV;
            //    fpsFOV += newFOV;
            //}       
        }
        yield return new WaitForSecondsRealtime(1f);

        if (player != null && player.CarController.useNitro)
            yield return new WaitUntil(() => !player.CarController.useNitro);

        speedEffects.gameObject.SetActive(false);
        isDelayCamera = false;
        if (cameraMode == CameraMode.TPS)
        {
            tpsFOV = tempFOV;
        }
        else
        {
            //if (player == null)
            //{
            //    float time = 0;
            //    fpsFOV = tempFOV;

            //    while (time < 0.5f)
            //    {
            //        Vector3 currentPositon = Vector3.zero;
            //        if (cameraMode == CameraMode.FPS)
            //            currentPositon = hoodCam.transform.position;
            //        else if (cameraMode == CameraMode.FPS1)
            //            currentPositon = CamFps1.transform.position;
            //        else if (cameraMode == CameraMode.FPS2)
            //            currentPositon = CamFps2.transform.position;
            //        time += Time.deltaTime;
            //        transform.position = Vector3.Lerp(transform.position, currentPositon, time / 0.1f);
            //        yield return null;
            //    }

            //    isCameraMovingBack = false;
            //}
        }
    }

    IEnumerator MoveSpeedEffects()
    {
        ParticleSystemRenderer particleSystemRenderer = speedEffects.GetComponent<ParticleSystemRenderer>();

        float t = 0f;
        float defLength = particleSystemRenderer.lengthScale;

        while (speedEffects.gameObject.activeSelf)
        {
            t += Time.deltaTime;
            if (t < 0.5f)
            {
                particleSystemRenderer.lengthScale = Mathf.Lerp(defLength, defLength - 1.5f, t / 0.5f);
            }
            yield return null;
        }

        particleSystemRenderer.lengthScale = defLength;
    }
    public void FOVGas(int value)
    {
        //if (!isCameraMovingBack)
        //{
            if(cameraMode == CameraMode.TPS)
                tpsFOV = Mathf.Lerp(tpsFOV, value, Time.deltaTime * 2f);            
        //}
    }
    public void activeWindEffect(bool active, HR_PlayerHandler player)
    {
        if (player.CarController.useNitro)
        {
            windEffect.gameObject.SetActive(false);
        }
        else
        {
            if (active)
                windEffect.gameObject.SetActive(true);
            else
                windEffect.gameObject.SetActive(false);
        }
    }
    private void OnDisable()
    {

        HR_GamePlayHandler.OnPlayerSpawned -= OnPlayerSpawned;
        HR_GamePlayHandler.OnPlayerDied -= OnPlayerCrashed;
        HR_GamePlayHandler.OnPlayerFailed -= OnPlayerFailed;
        HR_GamePlayHandler.OnPlayerRevive -= OnPlayerRevive;
        RCC_InputManager.OnChangeCamera -= RCC_InputManager_OnChangeCamera;
        HR_GamePlayHandler.OnPauseModeDual -= OnPauseModeAI;

    }

}