using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static HR_GamePlayHandler;

[RequireComponent(typeof(Rigidbody))]
public class HPS_TrafficCar : MonoBehaviour
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

    // Getting an Instance of HPS_TrafficPooling.
    #region HPS_TrafficPooling Instance
    private HPS_TrafficPooling HPS_TrafficPoolingInstance;
    private HPS_TrafficPooling HPS_TrafficPooling
    {
        get
        {
            if (HPS_TrafficPoolingInstance == null)
            {
                HPS_TrafficPoolingInstance = HPS_TrafficPooling.Instance;
            }
            return HPS_TrafficPoolingInstance;
        }
    }

    #endregion

    public Rigidbody rigid;
    public BoxCollider bodyCollider;
    [HideInInspector] public BoxCollider triggerCollider;       //  Trigger.

    private bool crashed = false;       //  Crashed?

    public ChangingLines changingLines = ChangingLines.Straight;
    public enum ChangingLines { Straight, Right, Left }
    internal int currentLine = 0;       //  Current line.

    public float maximumSpeed = 7f;        //  Maximum speed of the car.
    private float desiredSpeed;     //  Desired speed (adapted) of the car.
    private float distance = 0f;        //  Distance to next car.
    private Quaternion steeringAngle = Quaternion.identity;     //  Steering angle.
    private int countLine;
    internal bool fullLine = false;

    [Header("Parts for Instancing")]
    public Transform body;
    public Transform[] wheels;     //  Wheel models.
    public Transform[] lights;
    private float wheelRotation = 0f;       //  Wheel rotation.

    [Header("Just Lights. All of them will work on ''NOT Important'' Render Mode.")]
    public Light[] headLights;
    public Light[] brakeLights;
    public Light[] signalLights;

    private bool headlightsOn = false;
    private bool brakingOn = false;

    private SignalsOn signalsOn = SignalsOn.Off;
    private enum SignalsOn { Off, Right, Left, All }
    private float signalTimer = 0f;
    private float spawnProtection = 0f;

    [Space(10)]

    public AudioClip engineSound;
    private AudioSource engineSoundSource;

    private bool stab;

    [SerializeField] Animator anim;

#if PHOTON_UNITY_NETWORKING && BCG_HR_PHOTON
    private bool networked = false;
#endif

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        rigid.drag = 1f;
        rigid.angularDrag = 4f;
        rigid.maxAngularVelocity = 2.5f;
        rigid.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY;
        
        Light[] allLights = GetComponentsInChildren<Light>();
        //  Setting lights as vertex lights and make sure they are not affecting to any surface.
        foreach (Light l in allLights)
            l.renderMode = LightRenderMode.ForceVertex;

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

        //  Setting layer of the child gameobjects.
        foreach (Transform t in transform)
            t.gameObject.layer = (int)Mathf.Log(HR_HighwayRacerProperties.Instance.trafficCarsLayer.value, 2);

        //  Setting layer of the trigger volume.
        triggerCollider.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

    }

    private void OnEnable()
    {
        HR_GamePlayHandler.OnPlayerRevive += PlayerRevive;
        HR_GamePlayHandler.OnPlayerDied += OnGameEnd;
        HR_GamePlayHandler.OnPlayerWon += OnGameEnd;
        HR_GamePlayHandler.OnPlayerFailed += OnGameEnd;
    }

    // Start is called before the first frame update
    void Start()
    {
#if PHOTON_UNITY_NETWORKING && BCG_HR_PHOTON

        if (GetComponent<PhotonView>() && !GetComponent<PhotonView>().IsMine)
            networked = true;

                    if (networked)
            return;

#endif

        InvokeRepeating("SpeedUp", 4f, 4f);
    }

    // Update is called once per frame
    void Update()
    {
        //  Spawn protection used for preventing crashes too soon.
        spawnProtection += Time.deltaTime;
        if (stab || rigid.isKinematic)
            return;
        Lights();
        Wheels();
    }

    /// <summary>
    /// Lights of the car.
    /// </summary>
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
                brakeLights[i].intensity = 1f;
            else
            {
                if (!headlightsOn)
                    brakeLights[i].intensity = 0f;
                else
                    brakeLights[i].intensity = .6f;
            }
        }
    }

    private void Wheels()
    {
        for (int i = 0; i < wheels.Length; i++)
        {
            wheelRotation += desiredSpeed * 20 * Time.deltaTime;
            wheels[i].transform.localRotation = Quaternion.Euler(wheelRotation, 0f, 0f);
        }
    }

    void FixedUpdate()
    {
        if (stab || rigid.isKinematic)
            return;

#if PHOTON_UNITY_NETWORKING && BCG_HR_PHOTON

        if (networked)
            return;

#endif
        int numberPooling = PlayerPrefs.GetInt("numberPooling");
        //  Adjusting desired speed according to distance to the next car. If crashed, set it to 0.
        if (!crashed)
            desiredSpeed = Mathf.Clamp(maximumSpeed - Mathf.Lerp(maximumSpeed, 0f, (distance - 10f) / 50f), 0f, maximumSpeed);
        else
            desiredSpeed = Mathf.Lerp(desiredSpeed, 0f, Time.fixedDeltaTime);

        //  Braking distance.
        if (distance < 50)
            brakingOn = true;
        else
            brakingOn = false;

        // If mode is not two ways, adjust steering angle.
        if (!crashed && HR_GamePlayHandler.mode != HR_GamePlayHandler.Mode.TwoWay && HR_GamePlayHandler.mode != HR_GamePlayHandler.Mode.MissionMode_2 && numberPooling != 1)
            transform.rotation = Quaternion.Lerp(transform.rotation, steeringAngle, Time.fixedDeltaTime * 3f);

        // Setting linear and angular velocity of the car.
        rigid.velocity = Vector3.Slerp(rigid.velocity, transform.forward * desiredSpeed, Time.fixedDeltaTime * 3f);
        rigid.angularVelocity = Vector3.Slerp(rigid.angularVelocity, Vector3.zero, Time.fixedDeltaTime * 10f);

        //  If game mode is not two ways, change the lines.
        if (!crashed && HR_GamePlayHandler.mode != HR_GamePlayHandler.Mode.TwoWay && HR_GamePlayHandler.mode != HR_GamePlayHandler.Mode.MissionMode_2 && numberPooling != 1)
        {   
            currentLine = Mathf.Clamp(currentLine, 0, 3);
            int lineLenght = 3;
            if (fullLine)
            {
                if (changingLines == ChangingLines.Right)
                {
                    for (int i = 0; i <= lineLenght; i++)
                    {
                        if (currentLine == i)
                            countLine = lineLenght - i;
                    }
                }
                if (changingLines == ChangingLines.Left)
                    countLine = currentLine;
            }
            else
                countLine = 1;

            switch (changingLines)
            {
                case ChangingLines.Straight:
                    steeringAngle = Quaternion.identity;
                    maximumSpeed = 7;
                    break;
                case ChangingLines.Left:
                    if (currentLine == 0)
                    {
                        changingLines = ChangingLines.Right;
                        break;
                    }
                    if (transform.position.x <= HPS_TrafficPooling.lines[currentLine - countLine].position.x + .5f)
                    {
                        currentLine--;
                        signalsOn = SignalsOn.Off;
                        changingLines = ChangingLines.Straight;
                        fullLine = false;
                    }
                    else
                    {
                        steeringAngle = Quaternion.identity * Quaternion.Euler(0f, -5f, 0f);
                        signalsOn = SignalsOn.Left;
                    }
                    break;
                case ChangingLines.Right:
                    if (currentLine == 3)
                    {
                        changingLines = ChangingLines.Left;
                        break;
                    }
                    if (transform.position.x >= HPS_TrafficPooling.lines[currentLine + countLine].position.x - .5f)
                    {
                        currentLine++;
                        signalsOn = SignalsOn.Off;
                        changingLines = ChangingLines.Straight;
                        fullLine = false;
                    }
                    else
                    {
                        steeringAngle = Quaternion.identity * Quaternion.Euler(0f, 5f, 0f);
                        signalsOn = SignalsOn.Right;
                    }
                    break;
            }

        }
        if (!crashed && HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.TwoWay
            || !crashed && HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.MissionMode_2 || !crashed && numberPooling == 1)
        {
            currentLine = Mathf.Clamp(currentLine, 2, 3);
            if (transform.position.x > 0)
                transform.rotation = Quaternion.Lerp(transform.rotation, steeringAngle, Time.fixedDeltaTime * 3f);

            switch (changingLines)
            {
                case ChangingLines.Straight:
                    steeringAngle = Quaternion.identity;
                    maximumSpeed = 7;
                    break;
                case ChangingLines.Left:

                    if (currentLine == 2)
                    {
                        changingLines = ChangingLines.Right;
                        break;
                    }

                    if (transform.position.x <= HPS_TrafficPooling.lines[currentLine - 1].position.x + .5f)
                    {
                        currentLine--;
                        signalsOn = SignalsOn.Off;
                        changingLines = ChangingLines.Straight;
                    }
                    else
                    {
                        steeringAngle = Quaternion.identity * Quaternion.Euler(0f, -5f, 0f);
                        signalsOn = SignalsOn.Left;
                    }
                    break;
                case ChangingLines.Right:

                    if (currentLine == 3)
                    {
                        changingLines = ChangingLines.Left;
                        break;
                    }

                    if (transform.position.x >= HPS_TrafficPooling.lines[currentLine + 1].position.x - .5f)
                    {
                        currentLine++;
                        signalsOn = SignalsOn.Off;
                        changingLines = ChangingLines.Straight;
                    }
                    else
                    {
                        steeringAngle = Quaternion.identity * Quaternion.Euler(0f, 5f, 0f);
                        signalsOn = SignalsOn.Right;
                    }
                    break;
            }
        }
    }

    private void OnTriggerStay(Collider col)
    {

#if PHOTON_UNITY_NETWORKING && BCG_HR_PHOTON

        if (networked)
            return;

#endif

        if ((1 << col.gameObject.layer) != HR_HighwayRacerProperties.Instance.trafficCarsLayer.value || col.isTrigger)
            return;

        //  Calculating distance to the next car.
        distance = Vector3.Distance(transform.position, col.transform.position);

    }

    private void OnCollisionEnter(Collision col)
    {

        if (crashed || spawnProtection < .5f)
            return;

        crashed = true;
        signalsOn = SignalsOn.All;

    }

    private void OnTriggerEnter(Collider other)
    {
        //signalsOn = SignalsOn.All;
    }

    public void OnReAligned()
    {
        crashed = false;
        spawnProtection = 0f;
        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;
        signalsOn = SignalsOn.Off;
        changingLines = ChangingLines.Straight;
        distance = 50f;
    }

    public void Rammed(bool left, bool right, bool front)
    {
        if (stab)
            return;

        stab = true;
        rigid.useGravity = false;
        bodyCollider.enabled = false;

        if (left || right)
        {
            if (HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.TwoWay
            || HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.MissionMode_2 || PlayerPrefs.GetInt("numberPooling") == 1)
            {
                if (transform.position.x > 0)
                {
                    anim.Play("carCrash");
                    StartCoroutine(CrashAnimation(left ? "leftCrash" : "rightCrash"));
                }
                else
                {
                    anim.Play("carCrash");
                    StartCoroutine(CrashAnimation(left ? "rightCrashTwoWay" : "leftCrashTwoWay"));
                }
            }
            else
            {
                anim.Play("carCrash");
                StartCoroutine(CrashAnimation(left ? "leftCrash" : "rightCrash"));
            }
        }
        else
        {
            anim.Play("carCrash");
            StartCoroutine(CrashAnimation(front ? "carCrashFront" : "carCrash"));
        }
    }

    public void RammedAi(bool left, bool right)
    {
        if (stab)
            return;
        stab = true;
        rigid.useGravity = false;
        bodyCollider.enabled = false;
        if (left || right)
        {
            anim.Play("carCrash");
            StartCoroutine(CrashAnimation(left ? "leftCrash" : "rightCrash"));
        }
    }

    public void Reset()
    {
        if (!gameObject.activeSelf)
            return;

        stab = false;
        transform.position = new Vector3(transform.position.x, 0, transform.position.z);
        rigid.useGravity = true;
        bodyCollider.enabled = true;
        if (!anim.GetCurrentAnimatorStateInfo(0).IsName("idle"))
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
    public void ChangeLines()
    {

        if (changingLines == ChangingLines.Left || changingLines == ChangingLines.Right)
            return;

        int randomNumber = Random.Range(0, 2);

        changingLines = randomNumber == 0 ? ChangingLines.Left : ChangingLines.Right;
        maximumSpeed = 20;
    }

    private IEnumerator CrashAnimation(string animationName)
    {
        float time = 0f;
        float timer = 0.2f;
        Vector3 position = transform.position;
        Vector3 rotation = transform.rotation.eulerAngles;

        switch (animationName)
        {
            case "carCrash":
                while (time < timer)
                {
                    transform.SetLocalPositionAndRotation(Vector3.Lerp(position, position + new Vector3(0f, 10f, -10f), time / timer), Quaternion.Euler(Vector3.Lerp(rotation, rotation + new Vector3(25f, 0f, 0f), time / timer)));
                    time += Time.deltaTime;
                    yield return null;
                }
                time = 0f;
                position = transform.position;
                rotation = transform.rotation.eulerAngles;
                while (time < timer)
                {
                    transform.SetLocalPositionAndRotation(Vector3.Lerp(position, position + new Vector3(0f, -10f, -2f), time / timer), Quaternion.Euler(Vector3.Lerp(rotation, rotation + new Vector3(-15f, 0f, 0f), time / timer)));
                    time += Time.deltaTime;
                    yield return null;
                }
                time = 0f;
                timer = 0.05f;
                position = transform.position;
                rotation = transform.rotation.eulerAngles;
                while (time < timer)
                {
                    transform.SetLocalPositionAndRotation(Vector3.Lerp(position, position + new Vector3(0f, 0f, -3f), time / timer), Quaternion.Euler(Vector3.Lerp(rotation, rotation + new Vector3(-10f, 0f, 0f), time / timer)));
                    time += Time.deltaTime;
                    yield return null;
                }
                break;
            case "carCrashFront":
                while (time < timer)
                {
                    transform.SetLocalPositionAndRotation(Vector3.Lerp(position, position + new Vector3(0f, 10f, 5f), time / timer), Quaternion.Euler(Vector3.Lerp(rotation, rotation + new Vector3(-25f, 0f, 0f), time / timer)));
                    time += Time.deltaTime;
                    yield return null;
                }
                time = 0f;
                position = transform.position;
                rotation = transform.rotation.eulerAngles;
                while (time < timer)
                {
                    transform.SetLocalPositionAndRotation(Vector3.Lerp(position, position + new Vector3(0f, -10f, 15f), time / timer), Quaternion.Euler(Vector3.Lerp(rotation, rotation + new Vector3(35f, 0f, 0f), time / timer)));
                    time += Time.deltaTime;
                    yield return null;
                }
                time = 0f;
                position = transform.position;
                rotation = transform.rotation.eulerAngles;
                while (time < timer)
                {
                    transform.SetLocalPositionAndRotation(Vector3.Lerp(position, position, time / timer), Quaternion.Euler(Vector3.Lerp(rotation, rotation + new Vector3(-10f, 0f, 0f), time / timer)));
                    time += Time.deltaTime;
                    yield return null;
                }
                break;
            case "leftCrash":
                while (time < timer)
                {
                    transform.SetLocalPositionAndRotation(Vector3.Lerp(position, position + new Vector3(-2f, 3f, 8.3f), time / timer), Quaternion.Euler(Vector3.Lerp(rotation, rotation + new Vector3(0f, 0f, 30f), time / timer)));
                    time += Time.deltaTime;
                    yield return null;
                }
                time = 0f;
                position = transform.position;
                rotation = transform.rotation.eulerAngles;
                while (time < timer)
                {
                    transform.SetLocalPositionAndRotation(Vector3.Lerp(position, position + new Vector3(-8f, -3f, 0f), time / timer), Quaternion.Euler(Vector3.Lerp(rotation, rotation + new Vector3(0f, 0f, -30f), time / timer)));
                    time += Time.deltaTime;
                    yield return null;
                }
                break;
            case "leftCrashTwoWay":
                while (time < timer)
                {
                    transform.SetLocalPositionAndRotation(Vector3.Lerp(position, position + new Vector3(-2f, 3f, -8.3f), time / timer), Quaternion.Euler(Vector3.Lerp(rotation, rotation + new Vector3(0f, 0f, 30f), time / timer)));
                    time += Time.deltaTime;
                    yield return null;
                }
                time = 0f;
                position = transform.position;
                rotation = transform.rotation.eulerAngles;
                while (time < timer)
                {
                    transform.SetLocalPositionAndRotation(Vector3.Lerp(position, position + new Vector3(-8f, -3f, 0f), time / timer), Quaternion.Euler(Vector3.Lerp(rotation, rotation + new Vector3(0f, 0f, -30f), time / timer)));
                    time += Time.deltaTime;
                    yield return null;
                }
                break;
            case "rightCrash":
                while (time < timer)
                {
                    transform.SetLocalPositionAndRotation(Vector3.Lerp(position, position + new Vector3(2f, 3f, 8.3f), time / timer), Quaternion.Euler(Vector3.Lerp(rotation, rotation + new Vector3(0f, 0f, -30f), time / timer)));
                    time += Time.deltaTime;
                    yield return null;
                }
                time = 0f;
                position = transform.position;
                rotation = transform.rotation.eulerAngles;
                while (time < timer)
                {
                    transform.SetLocalPositionAndRotation(Vector3.Lerp(position, position + new Vector3(8f, -3f, 0f), time / timer), Quaternion.Euler(Vector3.Lerp(rotation, rotation + new Vector3(0f, 0f, 30f), time / timer)));
                    time += Time.deltaTime;
                    yield return null;
                }
                break;
            case "rightCrashTwoWay":
                while (time < timer)
                {
                    transform.SetLocalPositionAndRotation(Vector3.Lerp(position, position + new Vector3(2f, 3f, -8.3f), time / timer), Quaternion.Euler(Vector3.Lerp(rotation, rotation + new Vector3(0f, 0f, -30f), time / timer)));
                    time += Time.deltaTime;
                    yield return null;
                }
                time = 0f;
                position = transform.position;
                rotation = transform.rotation.eulerAngles;
                while (time < timer)
                {
                    transform.SetLocalPositionAndRotation(Vector3.Lerp(position, position + new Vector3(8f, -3f, 0f), time / timer), Quaternion.Euler(Vector3.Lerp(rotation, rotation + new Vector3(0f, 0f, 30f), time / timer)));
                    time += Time.deltaTime;
                    yield return null;
                }
                break;
        }

        yield return null;
    }

    public void PlayerRevive(HR_PlayerHandler player)
    {
        StartCoroutine(DisableCollider(player));
    }
    public IEnumerator DisableCollider(HR_PlayerHandler player)
    {
        rigid.isKinematic = false;
        Collider collider = GetComponentInChildren<Collider>();
        collider.isTrigger = true;
        yield return new WaitForSeconds(3f);
        collider.isTrigger = false;
    }
    public void OnGameEnd(HR_PlayerHandler player, int[] scores)
    {
        Collider collider = GetComponentInChildren<Collider>();
        collider.isTrigger = true;
        rigid.isKinematic = true;
    }
    private void OnDisable()
    {
        HR_GamePlayHandler.OnPlayerRevive -= PlayerRevive;
        HR_GamePlayHandler.OnPlayerDied -= OnGameEnd;
        HR_GamePlayHandler.OnPlayerWon -= OnGameEnd;
        HR_GamePlayHandler.OnPlayerFailed -= OnGameEnd;
    }

}
