//----------------------------------------------
//           	   Highway Racer
//
// Copyright © 2014 - 2023 BoneCracker Games
// http://www.bonecrackergames.com
//
//----------------------------------------------

using UnityEngine;
using System.Collections;
using TMPro;
#if PHOTON_UNITY_NETWORKING
using Photon;
using Photon.Pun;
#endif

/// <summary>
/// Traffic car controller.
/// </summary>
public class HR_AICar : MonoBehaviour
{

    // Getting an Instance of HR_GamePlayHandler.
    #region HR_GamePlayHandler Instance

    private HR_GamePlayHandler HR_GamePlayHandlerInstance;
    private HR_GamePlayHandler HR_GamePlayHandler
    {
        get
        {
            if (HR_GamePlayHandlerInstance == null)
            {
                HR_GamePlayHandlerInstance = HR_GamePlayHandler.Instance;
            }
            return HR_GamePlayHandlerInstance;
        }
    }

    #endregion

    // Getting an Instance of HR_TrafficPooling.
    #region HR_TrafficPooling Instance

    private HR_TrafficPooling HR_TrafficPoolingInstance;
    private HR_TrafficPooling HR_TrafficPooling
    {
        get
        {
            if (HR_TrafficPoolingInstance == null)
            {
                HR_TrafficPoolingInstance = HR_TrafficPooling.Instance;
            }
            return HR_TrafficPoolingInstance;
        }
    }

    #endregion

    public Rigidbody rigid;        //  Rigidbody.
    public BoxCollider bodyCollider;        //  Collider.
    [HideInInspector] public BoxCollider triggerCollider;       //  Trigger.

    private bool crashed = false;       //  Crashed?

    public ChangingLines changingLines = ChangingLines.Straight;
    public enum ChangingLines { Straight, Right, Left }
    internal int currentLine = 0;       //  Current line.

    public float maximumSpeed = 60f;        //  Maximum speed of the car.
    private float _maximumSpeed = 60f;
    public float desiredSpeed;     //  Desired speed (adapted) of the car.
    private float distance = 0f;        //  Distance to next car.
    private Quaternion steeringAngle = Quaternion.identity;     //  Steering angle.

    public Transform[] wheelModels;     //  Wheel models.
    private float wheelRotation = 0f;       //  Wheel rotation.

    [Header("Just Lights. All of them will work on ''NOT Important'' Render Mode.")]
    public Light[] headLights;
    public Light[] brakeLights;
    public Light[] signalLights;

    private bool headlightsOn = false;
    private bool brakingOn = false;

    private SignalsOn signalsOn = SignalsOn.Off;
    private enum SignalsOn { Off, Right, Left, All }
    private enum CheckDirection { Off, Right, Left }
    private CheckDirection checkDirection = CheckDirection.Off;
    private float signalTimer = 0f;
    private float spawnProtection = 0f;
    [Space(10)]

    public AudioClip engineSound;
    private AudioSource engineSoundSource;

    private bool stab;

    [SerializeField] Animator anim;
    //dev viet
    float timeRun;

    bool isCheckStart = false;

    private Vector3 previousPosition;       //  Previous position used to calculate total traveled distance.

    public float totalDistance;

    public TextMeshProUGUI textName;

    public ColiderCar coliderCar;

    public bool isCheckDistance = false;

    public float numberFake;
    float angle;
    public float angleMax;
    public float timeAngle;

    public string currentTrafficCarNameLeft = null;
    public string currentTrafficCarNameRight = null;
    public string currentTrafficCarNameForward = null;
    public LayerMask layerMask;
    public Transform Ai;
#if PHOTON_UNITY_NETWORKING && BCG_HR_PHOTON
    private bool networked = false;
#endif

    private void Awake()
    {

        // Rigidbody.
        rigid = GetComponent<Rigidbody>();
        rigid.drag = 1f;
        rigid.angularDrag = 0.5f;
        rigid.maxAngularVelocity = 2.5f;
        rigid.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY;

        //  Getting all lights.
        Light[] allLights = GetComponentsInChildren<Light>();

        //  Setting lights as vertex lights and make sure they are not affecting to any surface.
        foreach (Light l in allLights)
            l.renderMode = LightRenderMode.ForceVertex;

        //  Forward distance.
        distance = 50f;

        //  Collider.
        if (!bodyCollider)
        {

            Debug.LogWarning(transform.name + "is missing collider in HR_TrafficCar script. Select your vehicle collider. Assigning collider automatically now, but it may select the wrong collider now...");
            bodyCollider = GetComponentInChildren<BoxCollider>();

        }

        //  Creating trigger for detecting front vehicles.
        GameObject triggerColliderGO = new GameObject("TriggerVolume");
        triggerColliderGO.transform.position = bodyCollider.bounds.center;
        triggerColliderGO.transform.rotation = bodyCollider.transform.rotation;
        triggerColliderGO.transform.SetParent(transform, true);
        triggerColliderGO.transform.localScale = bodyCollider.transform.localScale;
        triggerColliderGO.AddComponent<BoxCollider>();
        triggerColliderGO.GetComponent<BoxCollider>().isTrigger = true;
        triggerColliderGO.GetComponent<BoxCollider>().size = bodyCollider.size;
        triggerColliderGO.GetComponent<BoxCollider>().center = bodyCollider.center;

        triggerCollider = triggerColliderGO.GetComponent<BoxCollider>();
        triggerCollider.size = new Vector3(bodyCollider.size.x * .95f, bodyCollider.size.y, bodyCollider.size.z + (bodyCollider.size.z * 3f));
        triggerCollider.center = new Vector3(bodyCollider.center.x, 0f, bodyCollider.center.z + (triggerCollider.size.z / 2f) - (bodyCollider.size.z / 2f));

        // Enabling lights if scene is a night or rainy scene.
        if (HR_GamePlayHandler.dayOrNight == HR_GamePlayHandler.DayOrNight.Night)
            headlightsOn = true;
        else
            headlightsOn = false;

        //  Creating engine sound.
        engineSoundSource = HR_CreateAudioSource.NewAudioSource(gameObject, "Engine Sound", 2f, 5f, 1f, engineSound, true, true, false);
        engineSoundSource.pitch = 1.5f;

        _maximumSpeed = maximumSpeed;

        //  Setting layer of the child gameobjects.
        foreach (Transform t in transform)
            t.gameObject.layer = (int)Mathf.Log(HR_HighwayRacerProperties.Instance.trafficCarsLayer.value, 2);

        //  Setting layer of the trigger volume.
        triggerCollider.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

    }

    private void Start()
    {

#if PHOTON_UNITY_NETWORKING && BCG_HR_PHOTON

        if (GetComponent<PhotonView>() && !GetComponent<PhotonView>().IsMine)
            networked = true;

                    if (networked)
            return;

#endif

        //  Speeding up the vehicle at each 4 seconds.
        InvokeRepeating("SpeedUp", 4f, 4f);
        //  Changing lines randomly.
        InvokeRepeating("ChangeLines", Random.Range(15, 45), Random.Range(15, 45));
        desiredSpeed = 13f;
        StartCoroutine(dealyStart());

    }
    private IEnumerator dealyStart()
    {
        yield return new WaitForSeconds(4f);
        isCheckStart = true;

    }
    bool isCheck;
    float timeRunAI;
    private void Update()
    {
        //  Spawn protection used for preventing crashes too soon.
        spawnProtection += Time.deltaTime;
        if (stab)
            return;
        Lights();
        Wheels();
        //if (!isCheckDistance)
        //    return;
        totalDistance += Vector3.Distance(previousPosition, transform.position) / 1000f;
        previousPosition = transform.position;
        var totalDistanceAfter = totalDistance - 0.08f;

        
        if(isCheckStart)
        {
            CheckNearMiss();
        }    
        
        if (Input.GetKeyDown(KeyCode.E))
        {
            signalsOn = SignalsOn.Right;
            changingLines = ChangingLines.Right;
            coliderCar.gameObject.SetActive(false);
            coliderCar.isCheckCollide = false;
            isCheck = true;
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            signalsOn = SignalsOn.Left;
            changingLines = ChangingLines.Left;
            coliderCar.gameObject.SetActive(false);
            coliderCar.isCheckCollide = false;
            isCheck = true;
        }
    }

    /// <summary>
    /// Lights of the car.
    /// </summary>uuq
    private void Lights()
    {

        signalTimer += Time.deltaTime;

        for (int i = 0; i < signalLights.Length; i++)
        {

            if (signalsOn == SignalsOn.Off)
                signalLights[i].intensity = 0f;

            if (signalsOn == SignalsOn.Left)
            {

                if (signalTimer >= .5f)
                {

                    if (signalLights[i].transform.localPosition.x < 0f)
                        signalLights[i].intensity = 0f;

                }
                else
                {

                    if (signalLights[i].transform.localPosition.x < 0f)
                        signalLights[i].intensity = 1f;
                }

                if (signalTimer >= 1f)
                    signalTimer = 0f;

            }

            if (signalsOn == SignalsOn.Right)
            {

                if (signalTimer >= .5f)
                {

                    if (signalLights[i].transform.localPosition.x > 0f)
                        signalLights[i].intensity = 0f;

                }
                else
                {

                    if (signalLights[i].transform.localPosition.x > 0f)
                        signalLights[i].intensity = 1f;

                }

                if (signalTimer >= 1f)
                    signalTimer = 0f;
            }

            if (signalsOn == SignalsOn.All)
            {

                if (signalTimer >= .5f)
                    signalLights[i].intensity = 0f;
                else
                    signalLights[i].intensity = 1f;

                if (signalTimer >= 1f)
                    signalTimer = 0f;
            }

        }

        for (int i = 0; i < headLights.Length; i++)
        {

            if (!headlightsOn)
                headLights[i].intensity = 0f;
            else
                headLights[i].intensity = 1f;

        }

        for (int i = 0; i < brakeLights.Length; i++)
        {

            if (brakingOn)
            {

                brakeLights[i].intensity = 1f;

            }
            else
            {

                if (!headlightsOn)
                    brakeLights[i].intensity = 0f;
                else
                    brakeLights[i].intensity = .6f;

            }

        }

    }

    /// <summary>
    /// Wheels rotation.
    /// </summary>
    private void Wheels()
    {

        for (int i = 0; i < wheelModels.Length; i++)
        {

            wheelRotation += desiredSpeed * 20 * Time.deltaTime;
            wheelModels[i].transform.localRotation = Quaternion.Euler(wheelRotation, 0f, 0f);

        }

    }
    bool isCheckColiderCarAiWithCar = false;
    private void FixedUpdate()
    {
        if (stab)
            return;

#if PHOTON_UNITY_NETWORKING && BCG_HR_PHOTON

        if (networked)
            return;

#endif

        //  Adjusting desired speed according to distance to the next car. If crashed, set it to 0.
        //if (!crashed)
        //    desiredSpeed = Mathf.Clamp(maximumSpeed - Mathf.Lerp(maximumSpeed, 0f, (distance - 10f) / 50f), 0f, maximumSpeed);
        //else
        //    desiredSpeed = Mathf.Lerp(desiredSpeed, 0f, Time.fixedDeltaTime);

        //  Braking distance.

        //if (distance < 50)
        //    brakingOn = true;
        //else
        //    brakingOn = false;

        // If mode is not two ways, adjust steering angle.
        if (!crashed && HR_GamePlayHandler.mode != HR_GamePlayHandler.Mode.TwoWay)
            transform.rotation = Quaternion.Lerp(transform.rotation, steeringAngle, 0.5f); // Time.fixedDeltaTime * 1f);

        // Setting linear and angular velocity of the car.
        //rigid.velocity = Vector3.Slerp(rigid.velocity, transform.forward * desiredSpeed, Time.fixedDeltaTime * 3f);
        if (isCheckStart)
        {
            isCheckDistance = true;
            desiredSpeed += 5 * Time.deltaTime;
            desiredSpeed = Mathf.Clamp(desiredSpeed, 13f, maximumSpeed * numberFake);
        }
        rigid.velocity = Vector3.Slerp(rigid.velocity, transform.forward * desiredSpeed, Time.fixedDeltaTime * 3f);
        rigid.angularVelocity = Vector3.Slerp(rigid.angularVelocity, Vector3.zero, Time.fixedDeltaTime * 10f);
        #region AI
        //  If game mode is not two ways, change the lines.
        if (coliderCar.isCheckCollide)
        {
            if (currentLine == 0)
            {
                signalsOn = SignalsOn.Right;
                changingLines = ChangingLines.Right;
                coliderCar.gameObject.SetActive(false);
                coliderCar.isCheckCollide = false;
                isCheck = true;

            }
            else if (currentLine > 0)
            {
                var numberTurn = Random.Range(0, 100);
                Debug.Log("viet_numberTurn " + numberTurn);

                if (numberTurn % 2 == 0)
                {
                    signalsOn = SignalsOn.Right;
                    changingLines = ChangingLines.Right;
                    coliderCar.gameObject.SetActive(false);
                    coliderCar.isCheckCollide = false;
                    isCheck = true;
                }
                else
                {
                    signalsOn = SignalsOn.Left;
                    changingLines = ChangingLines.Left;
                    coliderCar.gameObject.SetActive(false);
                    coliderCar.isCheckCollide = false;
                    isCheck = true;
                }
            }
            else if (currentLine == 3)
            {
                signalsOn = SignalsOn.Left;
                changingLines = ChangingLines.Left;
                coliderCar.gameObject.SetActive(false);
                coliderCar.isCheckCollide = false;
                isCheck = true;

            }
        }

        if (!crashed)
        {
            currentLine = Mathf.Clamp(currentLine, 0, 3);
            switch (changingLines)
            {

                case ChangingLines.Straight:
                    switch (checkDirection)
                    {
                        case CheckDirection.Left:
                            angle += Time.deltaTime * timeAngle;
                            if (angle > 0.2f)
                            {
                                angle = 0;
                                checkDirection = CheckDirection.Off;
                            }
                            steeringAngle = Quaternion.identity * Quaternion.Euler(0f, angle, 0f);
                            break;
                        case CheckDirection.Right:
                            angle -= Time.deltaTime * timeAngle;
                            if (angle < -0.2f)
                            {
                                angle = 0;
                                checkDirection = CheckDirection.Off;
                            }
                            steeringAngle = Quaternion.identity * Quaternion.Euler(0f, angle, 0f);
                            break;
                    }
                    //steeringAngle = Quaternion.identity;
                    coliderCar.gameObject.SetActive(true);
                    break;

                case ChangingLines.Left:
                    if (currentLine == 0)
                    {
                        changingLines = ChangingLines.Straight;
                        break;
                    }

                    if (isCheck)
                    {
                        angle -= Time.deltaTime * timeAngle;
                        if (angle < -angleMax)
                        {
                            angle = -angleMax;
                            currentLine--;
                            signalsOn = SignalsOn.Off;
                            changingLines = ChangingLines.Straight;
                            checkDirection = CheckDirection.Left;
                            coliderCar.gameObject.SetActive(true);
                            isCheck = false;
                            return;
                        }
                        steeringAngle = Quaternion.identity * Quaternion.Euler(0f, angle, 0f);
                        signalsOn = SignalsOn.Left;

                    }
                    break;

                case ChangingLines.Right:
                    if (currentLine == 3)
                    {

                        changingLines = ChangingLines.Straight;
                        coliderCar.gameObject.SetActive(true);
                        break;

                    }

                    if (isCheck)
                    {
                        angle += Time.deltaTime * timeAngle;
                        if (angle > angleMax)
                        {
                            angle = angleMax;
                            currentLine++;
                            signalsOn = SignalsOn.Off;
                            changingLines = ChangingLines.Straight;
                            checkDirection = CheckDirection.Right;
                            coliderCar.gameObject.SetActive(true);
                            isCheck = false;
                            return;
                        }
                        steeringAngle = Quaternion.identity * Quaternion.Euler(0f, angle, 0f);
                        signalsOn = SignalsOn.Right;
                    }

                    break;

            }


        }
        #endregion
    }
    IEnumerator delayCarDie()
    {
        yield return new WaitForSeconds(0.5f);
        isCheckStart = true;
        yield return new WaitForSeconds(0.2f);
        gameObject.transform.GetChild(0).gameObject.SetActive(false);
        yield return new WaitForSeconds(0.2f);
        gameObject.transform.GetChild(0).gameObject.SetActive(true);
        yield return new WaitForSeconds(0.2f);
        gameObject.transform.GetChild(0).gameObject.SetActive(false);
        yield return new WaitForSeconds(0.2f);
        gameObject.transform.GetChild(0).gameObject.SetActive(true);
        yield return new WaitForSeconds(0.2f);
        gameObject.transform.GetChild(0).gameObject.SetActive(false);
        yield return new WaitForSeconds(0.2f);
        gameObject.transform.GetChild(0).gameObject.SetActive(true);
        yield return new WaitForSeconds(1f);
        bodyCollider.isTrigger = false;
        isCheckColiderCarAiWithCar = false;

    }
    private void OnTriggerStay(Collider col)
    {

#if PHOTON_UNITY_NETWORKING && BCG_HR_PHOTON

        if (networked)
            return;

#endif

        //if ((1 << col.gameObject.layer) != HR_HighwayRacerProperties.Instance.trafficCarsLayer.value || col.isTrigger)
        //    return;

        //Calculating distance to the next car.
        distance = Vector3.Distance(transform.position, col.transform.position);

    }

    private void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "TrafficCar")
        {
            HR_TrafficCar hR_TrafficCar = col.gameObject.GetComponent<HR_TrafficCar>();
            if (isCheckStart && hR_TrafficCar != null && currentTrafficCarNameForward == null)
            {
            }
            else if(isCheckStart && hR_TrafficCar != null && currentTrafficCarNameForward != null)
            {
                CarAiDie();
            }    
            else
            {
                CarAiDie();
            }
        }
    }
    public void CarAiDie()
    {
        isCheckStart = false;
        desiredSpeed = 0;
        bodyCollider.isTrigger = true;
        isCheckColiderCarAiWithCar = true;
        StartCoroutine(delayCarDie());
    }
    public void OnReAligned()
    {

        crashed = false;
        spawnProtection = 0f;
        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;
        signalsOn = SignalsOn.Off;
        changingLines = ChangingLines.Straight;
        maximumSpeed = Random.Range(_maximumSpeed, _maximumSpeed * 1.5f);
        distance = 50f;

    }
    public void Reset()
    {
        stab = false;
        transform.position = new Vector3(transform.position.x, 0, transform.position.z);
        rigid.useGravity = true;
        anim.Play("idle");

        Rigidbody rigidbody = GetComponent<Rigidbody>();

        if (!rigidbody)
            rigidbody = gameObject.AddComponent<Rigidbody>();

        rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        rigidbody.mass = 1500f;
        rigidbody.drag = 1f;
        rigidbody.angularDrag = 4f;
        rigidbody.maxAngularVelocity = 2.5f;
        rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY;
    }

    /// <summary>
    /// Speeds up the car.
    /// </summary>
    private void SpeedUp()
    {

        distance = 50f;

    }

    /// <summary>
    /// Switches the lines.
    /// </summary>
    private void ChangeLines()
    {

        if (changingLines == ChangingLines.Left || changingLines == ChangingLines.Right)
            return;

        int randomNumber = Random.Range(0, 2);

        changingLines = randomNumber == 0 ? ChangingLines.Left : ChangingLines.Right;

    }
    private void CheckNearMiss()
    {

        RaycastHit hit;
        Debug.DrawRay(Ai.position, (transform.forward * 3.5f), Color.green);
        Debug.DrawRay(Ai.position, (-transform.right * 1.5f), Color.green);
        Debug.DrawRay(Ai.position, (transform.right * 1.5f), Color.green);
        Debug.DrawRay(Ai.position, (Quaternion.Euler(0, 45f, 0) * -transform.right * 1.75f), Color.green);
        Debug.DrawRay(Ai.position, (Quaternion.Euler(0, -45f, 0) * transform.right * 1.75f), Color.green);
        
        // Raycasting to the left side.
        if (Physics.Raycast(Ai.position, (-transform.right), out hit, 1.5f, layerMask ))
        {

            //	If hits, get it's name.
            currentTrafficCarNameLeft = hit.transform.name;
            Debug.Log("viet_left_currentTrafficCarNameLeft khong null: " + currentTrafficCarNameLeft);

        }
        else
        {
                currentTrafficCarNameLeft = null;
            Debug.Log("viet_left_currentTrafficCarNameLeft null");


        }

        // Raycasting to the right side.
        if (Physics.Raycast(Ai.position, transform.right, out hit, 1.5f, layerMask))
        {

            //	If hits, get it's name.
            currentTrafficCarNameRight = hit.transform.name;
            Debug.Log("viet_right_currentTrafficCarNameLeft khong null: "+ currentTrafficCarNameRight);


        }
        else
        {
                currentTrafficCarNameRight = null;
            Debug.Log("viet_right_currentTrafficCarNameLeft null");


        }
        if (Physics.Raycast(Ai.position, transform.forward, out hit, 3.5f, layerMask))
        {

            //	If hits, get it's name.
            currentTrafficCarNameForward = hit.transform.name;
            Debug.Log("viet_right_currentTrafficCarNameForward khong null: "+ currentTrafficCarNameForward);


        }
        else
        {
            currentTrafficCarNameForward = null;
            Debug.Log("viet_right_currentTrafficCarNameForward null");


        }
    }
}
