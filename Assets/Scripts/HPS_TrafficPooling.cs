using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class HPS_TrafficPooling : MonoBehaviour
{
    #region SINGLETON PATTERN
    private static HPS_TrafficPooling instance;
    public static HPS_TrafficPooling Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<HPS_TrafficPooling>();
            return instance;
        }
    }
    #endregion

    private HR_PlayerHandler player;
    public HPS_TrafficCars[] trafficCars;
    public Transform[] lines;

    #region CarGraphicDatas
    [System.Serializable]
    public class HPS_TrafficCars
    {
        [System.Serializable]
        public class HPS_CarFragments
        {
            public Vector3 Position;
            public Vector3 Rotation;
            public Vector3 Scale;
            public Mesh Mesh;
            public Material Material;
        }

        [System.Serializable]
        public class HPS_CarFragmentWheel
        {
            public Vector3[] Positions = new Vector3[2];
            public Vector3[] Rotations = new Vector3[] { Vector3.zero, 180 * Vector3.up, Vector3.zero, 180 * Vector3.up };
            public Vector3[] Scales = new Vector3[] { Vector3.one, Vector3.one, Vector3.one, Vector3.one };
            public Mesh Mesh;
            public Material Material;
        }

        [System.Serializable]
        public class HPS_CarFragmentLight
        {
            public Vector3[] Positions;
            public Vector3[] Rotations;
            public Vector3[] Scales;
            public Mesh Mesh;
            public Material Material;
        }

        public GameObject trafficCar;
        public HPS_CarFragments[] fragments;
        public HPS_CarFragmentWheel wheels;
        public HPS_CarFragmentLight lights;
        public int frequence = 1;
    }

    [System.Serializable]
    private class HPS_CarGraphicDatas
    {
        public Mesh Mesh;
        public Material Material;
        public MaterialPropertyBlock MaterialPropertyBlock;
        public List<Matrix4x4> Matrices;
    }

    private List<HPS_CarGraphicDatas> carGraphicDatas = new();

    #endregion

    #region TrafficGraphicMapping

    private class HPS_TrafficGraphicMapping
    {
        public List<KeyValuePair<int, int>> FragmentPointers;
        public List<KeyValuePair<int, int>> WheelPointers;
        public List<KeyValuePair<int, int>> LightPointers;
        public HPS_TrafficCar TrafficCar;
    }

    private List<HPS_TrafficGraphicMapping> trafficGraphicMapping = new();

    #endregion

    private List<HPS_TrafficCar> _trafficCars = new();       //  Spawned traffic cars.
    internal GameObject container;      //  Container of the spawned traffic cars.

    public int countCarActive;
    private float timeChangeLine = 0;
    public float maxTimeChangeLine = 10f;
    public float disStartChangeLine = 0.5f;
    public float changeFullLine = 1f;

    public float posSpawnAI = 20;
    private int randomFullCar = 0;
    public int LargeCar = 0;
    float maxZPosition = float.MinValue;

    private void OnEnable()
    {
        HR_GamePlayHandler.OnPlayerSpawned += HR_PlayerHandler_OnPlayerSpawned;
    }

    private void HR_PlayerHandler_OnPlayerSpawned(HR_PlayerHandler _player)
    {
        player = _player;
    }

    // Start is called before the first frame update
    void Start()
    {

#if PHOTON_UNITY_NETWORKING && BCG_HR_PHOTON
        if (PhotonNetwork.InRoom && !PhotonNetwork.IsMasterClient)
            return;
#endif

        SetupGraphicDatas();
        CreateTraffic();
        GenerateTrafficGraphicMapping();
        SpawnCarFirst();
    }

    // Update is called once per frame
    void Update()
    {

#if PHOTON_UNITY_NETWORKING && BCG_HR_PHOTON

        if (PhotonNetwork.InRoom && !PhotonNetwork.IsMasterClient)
            return;

#endif

        if (HR_GamePlayHandler.Instance.gameStarted)
        {
            timeChangeLine += Time.deltaTime;
            SetDistanceSpawnCar();
            AnimateTraffic();
            if (player.distance > disStartChangeLine && timeChangeLine > maxTimeChangeLine)
            {
                timeChangeLine = 0;
                CallChangeLineRandomCar();
            }
        }

        ChangePosition();
        RenderCars();
    }

    private void SetupGraphicDatas()
    {
        for (int i = 0; i < trafficCars.Length; i++)
        {
            if (trafficCars[i].frequence == 0)
                continue;

            HPS_TrafficCar trafficCar = trafficCars[i].trafficCar.GetComponent<HPS_TrafficCar>();

            MaterialPropertyBlock MaterialPropertyBlock = new();

            List<Matrix4x4> Fragments = new();

            for (int k = 0; k < trafficCars[i].frequence; k++)
                Fragments.Add(Matrix4x4.TRS(trafficCar.body.localPosition, trafficCar.body.localRotation, trafficCar.body.localScale));

            if (trafficCar.body.GetComponent<MeshRenderer>().sharedMaterial.mainTexture)
                MaterialPropertyBlock.SetTexture("_MainTex", trafficCar.body.GetComponent<MeshRenderer>().sharedMaterial.mainTexture);

            if (trafficCar.body.GetComponent<MeshRenderer>().sharedMaterial.HasProperty("_MetallicGlossMap"))
                MaterialPropertyBlock.SetTexture("_MetallicGlossMap", trafficCar.body.GetComponent<MeshRenderer>().sharedMaterial.GetTexture("_MetallicGlossMap"));

            if (trafficCar.body.GetComponent<MeshRenderer>().sharedMaterial.HasProperty("_Glossiness"))
                MaterialPropertyBlock.SetFloat("_Glossiness", trafficCar.body.GetComponent<MeshRenderer>().sharedMaterial.GetFloat("_Glossiness"));

            if (trafficCar.body.GetComponent<MeshRenderer>().sharedMaterial.HasProperty("_OcclusionMap"))
            {
                MaterialPropertyBlock.SetTexture("_OcclusionMap", trafficCar.body.GetComponent<MeshRenderer>().sharedMaterial.GetTexture("_OcclusionMap"));
                MaterialPropertyBlock.SetFloat("_OcclusionStrength", trafficCar.body.GetComponent<MeshRenderer>().sharedMaterial.GetFloat("_OcclusionStrength"));
            }

            if (trafficCar.body.GetComponent<MeshRenderer>().sharedMaterial.HasProperty("_EmissionMap"))
            {
                MaterialPropertyBlock.SetTexture("_EmissionMap", trafficCar.body.GetComponent<MeshRenderer>().sharedMaterial.GetTexture("_EmissionMap"));
                MaterialPropertyBlock.SetColor("_EmissionColor", trafficCar.body.GetComponent<MeshRenderer>().sharedMaterial.GetColor("_EmissionColor"));
            }

            carGraphicDatas.Add(new HPS_CarGraphicDatas { Mesh = trafficCar.body.GetComponent<MeshFilter>().sharedMesh, Material = trafficCar.body.GetComponent<MeshRenderer>().sharedMaterial, MaterialPropertyBlock = MaterialPropertyBlock, Matrices = Fragments });

            List<Matrix4x4> Wheels = new();
            for (int j = 0; j < trafficCar.wheels.Length; j++)
            {
                for (int k = 0; k < trafficCars[i].frequence; k++)
                    Wheels.Add(Matrix4x4.TRS(trafficCar.wheels[j].localPosition, trafficCar.wheels[j].localRotation, trafficCar.wheels[j].localScale));
            }

            carGraphicDatas.Add(new HPS_CarGraphicDatas { Mesh = trafficCar.wheels[0].GetComponent<MeshFilter>().sharedMesh, Material = trafficCar.wheels[0].GetComponent<MeshRenderer>().sharedMaterial, MaterialPropertyBlock = MaterialPropertyBlock, Matrices = Wheels });

            List<Matrix4x4> Lights = new();
            for (int j = 0; j < trafficCar.lights.Length; j++)
            {
                for (int k = 0; k < trafficCars[i].frequence; k++)
                    Lights.Add(Matrix4x4.TRS(trafficCar.lights[j].localPosition, trafficCar.lights[j].localRotation, trafficCar.lights[j].localScale));
            }

            MaterialPropertyBlock MaterialPropertyBlockLight = new();
            MaterialPropertyBlockLight.SetColor("_Color", trafficCar.lights[0].GetComponent<MeshRenderer>().sharedMaterial.color);

            if (trafficCar.lights[0].GetComponent<MeshRenderer>().sharedMaterial.mainTexture)
                MaterialPropertyBlockLight.SetTexture("_MainTex", trafficCar.lights[0].GetComponent<MeshRenderer>().sharedMaterial.mainTexture);

            carGraphicDatas.Add(new HPS_CarGraphicDatas { Mesh = trafficCar.lights[0].GetComponent<MeshFilter>().sharedMesh, Material = trafficCar.lights[0].GetComponent<MeshRenderer>().sharedMaterial, MaterialPropertyBlock = MaterialPropertyBlockLight, Matrices = Lights });

        }
        #region Legacy
        //for (int i = 0; i < trafficCars.Length; i++)
        //{
        //    if (trafficCars[i].frequence == 0)
        //        continue;

        //    for (int j = 0; j < trafficCars[i].fragments.Length; j++)
        //    {
        //        List<Matrix4x4> Fragments = new();

        //        for (int k = 0; k < trafficCars[i].frequence; k++)
        //            Fragments.Add(Matrix4x4.TRS(trafficCars[i].fragments[j].Position, Quaternion.Euler(trafficCars[i].fragments[j].Rotation.x, trafficCars[i].fragments[j].Rotation.y, trafficCars[i].fragments[j].Rotation.z), trafficCars[i].fragments[j].Scale));

        //        MaterialPropertyBlock MaterialPropertyBlockFragment = new();
        //        MaterialPropertyBlockFragment.SetColor("_Color", trafficCars[i].fragments[j].Material.color);

        //        if (trafficCars[i].fragments[j].Material.mainTexture)
        //            MaterialPropertyBlockFragment.SetTexture("_MainTex", trafficCars[i].fragments[j].Material.mainTexture);

        //        carGraphicDatas.Add(new HPS_CarGraphicDatas { Mesh = trafficCars[i].fragments[j].Mesh, Material = trafficCars[i].fragments[j].Material, MaterialPropertyBlock = MaterialPropertyBlockFragment, Matrices = Fragments });
        //    }

        //    List<Matrix4x4> Wheels = new();
        //    for (int j = 0; j < trafficCars[i].wheels.Positions.Length; j++)
        //    {
        //        for (int k = 0; k < trafficCars[i].frequence; k++)
        //            Wheels.Add(Matrix4x4.TRS(trafficCars[i].wheels.Positions[j], Quaternion.Euler(trafficCars[i].wheels.Rotations[j]), trafficCars[i].wheels.Scales[j]));

        //    }
        //    MaterialPropertyBlock MaterialPropertyBlockWheel = new();
        //    MaterialPropertyBlockWheel.SetColor("_Color", trafficCars[i].wheels.Material.color);

        //    if (trafficCars[i].wheels.Material.mainTexture)
        //        MaterialPropertyBlockWheel.SetTexture("_MainTex", trafficCars[i].wheels.Material.mainTexture);

        //    carGraphicDatas.Add(new HPS_CarGraphicDatas { Mesh = trafficCars[i].wheels.Mesh, Material = trafficCars[i].wheels.Material, MaterialPropertyBlock = MaterialPropertyBlockWheel, Matrices = Wheels });

        //    List<Matrix4x4> Lights = new();
        //    for (int j = 0; j < trafficCars[i].lights.Positions.Length; j++)
        //    {
        //        for (int k = 0; k < trafficCars[i].frequence; k++)
        //            Lights.Add(Matrix4x4.TRS(trafficCars[i].lights.Positions[j], Quaternion.Euler(trafficCars[i].lights.Rotations[j]), trafficCars[i].lights.Scales[j]));
        //    }

        //    MaterialPropertyBlock MaterialPropertyBlockLight = new();
        //    MaterialPropertyBlockLight.SetColor("_Color", trafficCars[i].lights.Material.color);

        //    if (trafficCars[i].lights.Material.mainTexture)
        //        MaterialPropertyBlockLight.SetTexture("_MainTex", trafficCars[i].lights.Material.mainTexture);

        //    carGraphicDatas.Add(new HPS_CarGraphicDatas { Mesh = trafficCars[i].lights.Mesh, Material = trafficCars[i].lights.Material, MaterialPropertyBlock = MaterialPropertyBlockLight, Matrices = Lights });

        //}
        #endregion
    }

    /// <summary>
    /// Spawns all traffic cars.
    /// </summary>
    private void CreateTraffic()
    {
        container = new GameObject("Traffic Container");

        for (int i = 0; i < trafficCars.Length; i++)
        {
            for (int k = 0; k < trafficCars[i].frequence; k++)
            {
                GameObject go;
                if (PlayerPrefs.GetInt("Multiplayer", 0) == 0)
                    go = Instantiate(trafficCars[i].trafficCar, Vector3.zero, Quaternion.identity);
                else
                {
#if PHOTON_UNITY_NETWORKING && BCG_HR_PHOTON
                 go = PhotonNetwork.Instantiate("Photon Traffic Vehicles/" + trafficCars[i].trafficCar.name, Vector3.zero, Quaternion.identity);
#else
                    go = Instantiate(trafficCars[i].trafficCar, Vector3.zero, Quaternion.identity);
#endif

                }

                _trafficCars.Add(go.GetComponent<HPS_TrafficCar>());
                go.SetActive(false);
                go.transform.SetParent(container.transform, true);
                go.name = trafficCars[i].trafficCar.name;
            }
        }
    }

    private void GenerateTrafficGraphicMapping()
    {
        bool found = false;

        int bodyIndex = 0;
        int wheelIndex = 0;
        int lightIndex = 0;

        for (int i = 0; i < _trafficCars.Count; i++)
        {
            List<KeyValuePair<int, int>> BodyPointers = new();
            List<KeyValuePair<int, int>> WheelPointers = new();
            List<KeyValuePair<int, int>> LightPointers = new();
            for (int j = 0; j < carGraphicDatas.Count; j++)
            {
                int frequence = carGraphicDatas[j].Matrices.Count;

                if (carGraphicDatas[j].Material.name.Contains(_trafficCars[i].name))
                {
                    found = true;

                    for (int k = j; k < j + 3; k++)
                    {
                        if (k < j + 3 - 2)
                            BodyPointers.Add(new KeyValuePair<int, int>(k, bodyIndex));
                        else if (k < j + 3 - 1)
                        {
                            for (int l = wheelIndex * 4; l < (wheelIndex * 4) + 4; l++)
                                WheelPointers.Add(new KeyValuePair<int, int>(k, l));
                        }
                        else
                        {
                            for (int l = lightIndex * 8; l < (lightIndex * 8) + 8; l++)
                                LightPointers.Add(new KeyValuePair<int, int>(k, l));
                        }
                    }

                    if (found && bodyIndex < frequence)
                    {
                        bodyIndex++;
                        wheelIndex++;
                        lightIndex++;
                    }
                    break;
                }
                else
                {
                    if (found && bodyIndex > frequence - 1)
                    {
                        found = !found;
                        bodyIndex = 0;
                        wheelIndex = 0;
                        lightIndex = 0;
                    }
                }
            }

            trafficGraphicMapping.Add(new HPS_TrafficGraphicMapping { FragmentPointers = BodyPointers, WheelPointers = WheelPointers, LightPointers = LightPointers, TrafficCar = _trafficCars[i] });
        }

        #region Legacy

        //int fragmentIndex = 0;
        //int wheelIndex = 0;
        //int lightIndex = 0;

        //for (int i = 0; i < _trafficCars.Count; i++)
        //{
        //    List<KeyValuePair<int, int>> FragmentPointers = new();
        //    List<KeyValuePair<int, int>> WheelPointers = new();
        //    List<KeyValuePair<int, int>> LightPointers = new();
        //    for (int j = 0; j < carGraphicDatas.Count; j++)
        //    {
        //        if (carGraphicDatas[j].Material.name.Contains(_trafficCars[i].name))
        //        {
        //            found = true;

        //            for (int k = j; k < j + 4; k++)
        //            {
        //                if (k < j + 4 - 2)
        //                    FragmentPointers.Add(new KeyValuePair<int, int>(k, fragmentIndex));
        //                else if (k < j + 4 - 1)
        //                {
        //                    for (int l = wheelIndex * 4; l < (wheelIndex * 4) + 4; l++)
        //                        WheelPointers.Add(new KeyValuePair<int, int>(k, l));
        //                }
        //                else
        //                {
        //                    for (int l = lightIndex * 8; l < (lightIndex * 8) + 8; l++)
        //                        LightPointers.Add(new KeyValuePair<int, int>(k, l));
        //                }
        //            }

        //            if (found && fragmentIndex < 4)
        //            {
        //                fragmentIndex++;
        //                wheelIndex++;
        //                lightIndex++;
        //            }
        //            break;
        //        }
        //        else
        //        {
        //            if (found && fragmentIndex > 3)
        //            {
        //                found = !found;
        //                fragmentIndex = 0;
        //                wheelIndex = 0;
        //                lightIndex = 0;
        //            }
        //        }
        //    }

        //    trafficGraphicMapping.Add(new HPS_TrafficGraphicMapping { FragmentPointers = FragmentPointers, WheelPointers = WheelPointers, LightPointers = LightPointers, TrafficCar = _trafficCars[i] });
        //}
        #endregion
    }

    private void SpawnCarFirst()
    {
        for (int i = 0; i < countCarActive; i++)
            ShuffleCar(_trafficCars);

        for (int i = 0; i < countCarActive; i++)
            ReAlignTraffic(_trafficCars[i], true);
    }

    private void SetDistanceSpawnCar()
    {
        //khoang cach sinh xe
        if (player.distance < 0.5f)
        {
            posSpawnAI = 30;
            //chi spawn xe nho
            randomFullCar = _trafficCars.Count - LargeCar;
        }
        else if (player.distance >= 500 && player.distance < 1)
        {
            //spawn tat ca xe
            randomFullCar = _trafficCars.Count;
        }
        else if (player.distance >= 1 && player.distance < 2)
        {
            posSpawnAI = 25;
            randomFullCar = _trafficCars.Count;
        }
        else
        {
            posSpawnAI = 20;
            randomFullCar = _trafficCars.Count;
        }
    }

    /// <summary>
    /// Animates the traffic cars.
    /// </summary>
    private void AnimateTraffic()
    {
        //  If there is no camera, return.
        if (!Camera.main)
            return;

        //  If traffic car is below the camera or too far away, realign.
        for (int i = 0; i < countCarActive; i++)
        {
            if (Camera.main.transform.position.z > (_trafficCars[i].transform.position.z + 15) /*|| Camera.main.transform.position.z < (_trafficCars[i].transform.position.z - 300)*/)
                ReAlignTraffic(_trafficCars[i], false);
        }
    }

    /// <summary>
    /// Realigns the traffic car.
    /// </summary>
    /// <param name="realignableObject"></param>
    private void ReAlignTraffic(HPS_TrafficCar realignableObject, bool firstSpawn)
    {
        int randomLine = Random.Range(0, lines.Length);
        realignableObject.Reset();
        realignableObject.currentLine = randomLine;

        int index = _trafficCars.IndexOf(realignableObject) + 1;
        if (firstSpawn)
        {
            realignableObject.transform.position = new Vector3(lines[randomLine].position.x, lines[randomLine].position.y,
                (realignableObject.transform.position.z + 100 + (index * posSpawnAI)));

            switch (HR_GamePlayHandler.Instance.mode)
            {

                case (HR_GamePlayHandler.Mode.OneWay):
                case (HR_GamePlayHandler.Mode.MissionMode_1):
                    realignableObject.transform.rotation = Quaternion.identity;
                    break;
                case (HR_GamePlayHandler.Mode.TwoWay):
                case (HR_GamePlayHandler.Mode.MissionMode_2):
                    if (realignableObject.transform.position.x <= 0f)
                        realignableObject.transform.rotation = Quaternion.identity * Quaternion.Euler(0f, 180f, 0f);
                    else
                        realignableObject.transform.rotation = Quaternion.identity;
                    break;
                case (HR_GamePlayHandler.Mode.AIRacing):
                    if (PlayerPrefs.GetInt("numberPooling") == 0)
                    {
                        realignableObject.transform.rotation = Quaternion.identity;
                        break;
                    }
                    else
                    {
                        if (realignableObject.transform.position.x <= 0f)
                            realignableObject.transform.rotation = Quaternion.identity * Quaternion.Euler(0f, 180f, 0f);
                        else
                            realignableObject.transform.rotation = Quaternion.identity;
                        break;
                    }
            }
            
            if (!realignableObject.gameObject.activeSelf)
                realignableObject.gameObject.SetActive(true);

        }
        else
        {
            for (int i = 0; i < countCarActive; i++)
            {
                if (_trafficCars[i].transform.position.z > maxZPosition)
                    maxZPosition = _trafficCars[i].transform.position.z;
            }

            realignableObject.transform.position = new Vector3(lines[randomLine].position.x, lines[randomLine].position.y,
                (maxZPosition + posSpawnAI));

            HPS_TrafficCar randomCar = _trafficCars[Random.Range(10, randomFullCar)];

            switch (HR_GamePlayHandler.Instance.mode)
            {

                case (HR_GamePlayHandler.Mode.OneWay):
                case (HR_GamePlayHandler.Mode.MissionMode_1):
                    realignableObject.transform.rotation = Quaternion.identity;
                    break;
                case (HR_GamePlayHandler.Mode.TwoWay):
                case (HR_GamePlayHandler.Mode.MissionMode_2):
                    if (realignableObject.transform.position.x <= 0f)
                        realignableObject.transform.rotation = Quaternion.identity * Quaternion.Euler(0f, 180f, 0f);
                    else
                        realignableObject.transform.rotation = Quaternion.identity;
                    break;
                case (HR_GamePlayHandler.Mode.AIRacing):
                    if (PlayerPrefs.GetInt("numberPooling") == 0)
                    {
                        realignableObject.transform.rotation = Quaternion.identity;
                        break;
                    }
                    else
                    {
                        if (realignableObject.transform.position.x <= 0f)
                            realignableObject.transform.rotation = Quaternion.identity * Quaternion.Euler(0f, 180f, 0f);
                        else
                            realignableObject.transform.rotation = Quaternion.identity;
                        break;
                    }

            }

            realignableObject.OnReAligned();
            StartCoroutine(ActiveNewCar(realignableObject, randomCar));
        }
        if (CheckIfClipping(realignableObject.triggerCollider))
        {

            //realignableObject.gameObject.SetActive(false);

#if PHOTON_UNITY_NETWORKING && BCG_HR_PHOTON

            if (HR_PhotonHandler.Instance)
                HR_PhotonHandler.Instance.DisableTrafficvehicle(realignableObject.gameObject);

#endif

        }

    }

    IEnumerator ActiveNewCar(HPS_TrafficCar oldCar, HPS_TrafficCar newCar)
    {
        newCar.transform.position = oldCar.transform.position;
        newCar.transform.rotation = oldCar.transform.rotation;
        oldCar.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.05f);
        newCar.gameObject.SetActive(true);
        if (CheckIfClipping(newCar.triggerCollider))
        {
            newCar.gameObject.SetActive(false);
        }
        Swap(_trafficCars.IndexOf(oldCar), _trafficCars.IndexOf(newCar));
    }

    /// <summary>
    /// Checks if the new aligned car is clipping with another traffic car.
    /// </summary>
    /// <param name="trafficCarBound"></param>
    /// <returns></returns>
    private bool CheckIfClipping(BoxCollider trafficCarBound)
    {
        for (int i = 0; i < _trafficCars.Count; i++)
        {
            if (!trafficCarBound.transform.IsChildOf(_trafficCars[i].transform) && _trafficCars[i].gameObject.activeSelf)
            {
                if (HR_BoundsExtension.ContainBounds(trafficCarBound.transform, trafficCarBound.bounds, _trafficCars[i].triggerCollider.bounds))
                    return true;
            }
        }
        return false;
    }

    void CallChangeLineRandomCar()
    {
        if (_trafficCars.Count > 0)
        {
            List<HPS_TrafficCar> activeCars;
            if (HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.TwoWay
            || HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.MissionMode_2 || PlayerPrefs.GetInt("numberPooling") == 1)
                activeCars = _trafficCars.Where(car => car.gameObject.activeSelf && car.transform.position.x > 0).ToList();
            else
                activeCars = _trafficCars.Where(car => car.gameObject.activeSelf).ToList();
            
            if (activeCars.Count > 0)
            {
                int randomIndex = Random.Range(0, activeCars.Count);
                HPS_TrafficCar chosenCar = activeCars[randomIndex];
                if (chosenCar != null)
                {
                    chosenCar.fullLine = Random.value > 0.5f;
                    chosenCar.ChangeLines();
                }
            }


        }
    }

    void Swap(int indexA, int indexB)
    {
        HPS_TrafficCar temp = _trafficCars[indexA];
        _trafficCars[indexA] = _trafficCars[indexB];
        _trafficCars[indexB] = temp;
    }

    void ShuffleCar(List<HPS_TrafficCar> cars)
    {
        for (int i = cars.Count - (LargeCar + 1); i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            HPS_TrafficCar temp = cars[i];
            cars[i] = cars[randomIndex];
            cars[randomIndex] = temp;
        }
    }

    private void OnDisable()
    {
        HR_GamePlayHandler.OnPlayerSpawned -= HR_PlayerHandler_OnPlayerSpawned;
    }
    
    public void ChangePosition()
    {
        HPS_TrafficCar[] activeCars = _trafficCars.Where(car => car.gameObject.activeSelf).ToArray();

        for (int i = 0; i < activeCars.Length; i++)
        {
            var mapping = trafficGraphicMapping.First(mapping => mapping.TrafficCar.gameObject.GetInstanceID() == activeCars[i].gameObject.GetInstanceID());
            var trafficCar = trafficCars.First(trafficCar => trafficCar.trafficCar.name.Equals(activeCars[i].name));

            for (int j = 0; j < mapping.FragmentPointers.Count; j++)
            {
                if (HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.TwoWay || HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.MissionMode_2 || PlayerPrefs.GetInt("numberPooling") == 1)
                {
                    if (activeCars[i].transform.position.x > 0)
                        carGraphicDatas[mapping.FragmentPointers[j].Key].Matrices[mapping.FragmentPointers[j].Value] = Matrix4x4.TRS(activeCars[i].transform.position + trafficCar.trafficCar.GetComponent<HPS_TrafficCar>().body.localPosition, trafficCar.trafficCar.GetComponent<HPS_TrafficCar>().body.localRotation, trafficCar.trafficCar.GetComponent<HPS_TrafficCar>().body.localScale);
                    if (activeCars[i].transform.position.x < 0)
                        carGraphicDatas[mapping.FragmentPointers[j].Key].Matrices[mapping.FragmentPointers[j].Value] = Matrix4x4.TRS(activeCars[i].transform.position + trafficCar.trafficCar.GetComponent<HPS_TrafficCar>().body.localPosition, trafficCar.trafficCar.GetComponent<HPS_TrafficCar>().body.localRotation * Quaternion.Euler(0f, 180f, 0f), trafficCar.trafficCar.GetComponent<HPS_TrafficCar>().body.localScale);
                }
                else
                    carGraphicDatas[mapping.FragmentPointers[j].Key].Matrices[mapping.FragmentPointers[j].Value] = Matrix4x4.TRS(activeCars[i].transform.position + trafficCar.trafficCar.GetComponent<HPS_TrafficCar>().body.localPosition, trafficCar.trafficCar.GetComponent<HPS_TrafficCar>().body.localRotation, trafficCar.trafficCar.GetComponent<HPS_TrafficCar>().body.localScale);
            }
            for (int j = 0; j < mapping.WheelPointers.Count; j++)
            {
                if (HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.TwoWay || HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.MissionMode_2 || PlayerPrefs.GetInt("numberPooling") == 1)
                {
                    if (activeCars[i].transform.position.x > 0)
                        carGraphicDatas[mapping.WheelPointers[j].Key].Matrices[mapping.WheelPointers[j].Value] = Matrix4x4.TRS(activeCars[i].transform.position + trafficCar.trafficCar.GetComponent<HPS_TrafficCar>().wheels[j].localPosition, trafficCar.trafficCar.GetComponent<HPS_TrafficCar>().wheels[j].localRotation, trafficCar.trafficCar.GetComponent<HPS_TrafficCar>().wheels[j].localScale);
                    if (activeCars[i].transform.position.x < 0)
                        carGraphicDatas[mapping.WheelPointers[j].Key].Matrices[mapping.WheelPointers[j].Value] = Matrix4x4.TRS(activeCars[i].transform.position + trafficCar.trafficCar.GetComponent<HPS_TrafficCar>().wheels[j].localPosition, trafficCar.trafficCar.GetComponent<HPS_TrafficCar>().wheels[j].localRotation * Quaternion.Euler(0f, 180f, 0f), trafficCar.trafficCar.GetComponent<HPS_TrafficCar>().wheels[j].localScale);
                }
                else
                    carGraphicDatas[mapping.WheelPointers[j].Key].Matrices[mapping.WheelPointers[j].Value] = Matrix4x4.TRS(activeCars[i].transform.position + trafficCar.trafficCar.GetComponent<HPS_TrafficCar>().wheels[j].localPosition, trafficCar.trafficCar.GetComponent<HPS_TrafficCar>().wheels[j].localRotation, trafficCar.trafficCar.GetComponent<HPS_TrafficCar>().wheels[j].localScale);
            }
            for (int j = 0; j < mapping.LightPointers.Count; j++)
            {
                if (HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.TwoWay || HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.MissionMode_2 || PlayerPrefs.GetInt("numberPooling") == 1)
                {
                    if (activeCars[i].transform.position.x > 0)
                        carGraphicDatas[mapping.LightPointers[j].Key].Matrices[mapping.LightPointers[j].Value] = Matrix4x4.TRS(activeCars[i].transform.position + trafficCar.trafficCar.GetComponent<HPS_TrafficCar>().lights[j].localPosition, trafficCar.trafficCar.GetComponent<HPS_TrafficCar>().lights[j].localRotation, trafficCar.trafficCar.GetComponent<HPS_TrafficCar>().lights[j].localScale);
                    if (activeCars[i].transform.position.x < 0)
                        carGraphicDatas[mapping.LightPointers[j].Key].Matrices[mapping.LightPointers[j].Value] = Matrix4x4.TRS(activeCars[i].transform.position + trafficCar.trafficCar.GetComponent<HPS_TrafficCar>().lights[j].localPosition, trafficCar.trafficCar.GetComponent<HPS_TrafficCar>().lights[j].localRotation * Quaternion.Euler(0f, 180f, 0f), trafficCar.trafficCar.GetComponent<HPS_TrafficCar>().lights[j].localScale);
                }
                else
                    carGraphicDatas[mapping.LightPointers[j].Key].Matrices[mapping.LightPointers[j].Value] = Matrix4x4.TRS(activeCars[i].transform.position + trafficCar.trafficCar.GetComponent<HPS_TrafficCar>().lights[j].localPosition, trafficCar.trafficCar.GetComponent<HPS_TrafficCar>().lights[j].localRotation, trafficCar.trafficCar.GetComponent<HPS_TrafficCar>().lights[j].localScale);
            }
        }

        #region Legacy
        //for (int i = 0; i < mapping.FragmentPointers.Count; i++)
        //{
        //    var fragmentPointer = mapping.FragmentPointers[i];
        //    carGraphicDatas[fragmentPointer.Key].Matrices[fragmentPointer.Value] = Matrix4x4.TRS(car.transform.position + trafficCar.fragments[i].Position, Quaternion.Euler(trafficCar.fragments[i].Rotation), trafficCar.fragments[i].Scale);
        //}

        //for (int i = 0; i < mapping.WheelPointers.Count; i++)
        //{
        //    var wheelPointer = mapping.WheelPointers[i];
        //    carGraphicDatas[wheelPointer.Key].Matrices[wheelPointer.Value] = Matrix4x4.TRS(car.transform.position + trafficCar.wheels.Positions[i], Quaternion.Euler(trafficCar.wheels.Rotations[i]), trafficCar.wheels.Scales[i]);
        //}

        //for (int i = 0; i < mapping.LightPointers.Count; i++)
        //{
        //    var lightPointer = mapping.LightPointers[i];
        //    carGraphicDatas[lightPointer.Key].Matrices[lightPointer.Value] = Matrix4x4.TRS(car.transform.position + trafficCar.lights.Positions[i], Quaternion.Euler(trafficCar.lights.Rotations[i]), trafficCar.lights.Scales[i]);
        //}
        #endregion
    }

    private void RenderCars()
    {
        int trafficGraphicMappingIndex = 0;
        HPS_TrafficCars trafficCar = new();
        List<KeyValuePair<int, int>> fragments = new();
        List<KeyValuePair<int, int>> wheels = new();
        List<KeyValuePair<int, int>> lights = new();
        
        for (int i = 0; i < trafficGraphicMapping.Count; i++)
        {
            if (i < trafficGraphicMappingIndex)
            {
                i = trafficGraphicMappingIndex - 1;
                continue;
            }
            else
            {
                fragments.Clear();
                wheels.Clear();
                lights.Clear();
                trafficCar = trafficCars.First(trafficCar => trafficCar.trafficCar.name.StartsWith(trafficGraphicMapping[i].TrafficCar.name));
            }

            for (int j = i; j < i + trafficCar.frequence; j++)
            {
                if (!trafficGraphicMapping[j].TrafficCar.gameObject.activeInHierarchy)
                {
                    for (int k = 0; k < trafficGraphicMapping[j].FragmentPointers.Count; k++)
                        fragments.Add(trafficGraphicMapping[j].FragmentPointers[k]);

                    for (int k = 0; k < trafficGraphicMapping[j].WheelPointers.Count; k++)
                        wheels.Add(trafficGraphicMapping[j].WheelPointers[k]);

                    for (int k = 0; k < trafficGraphicMapping[j].LightPointers.Count; k++)
                        lights.Add(trafficGraphicMapping[j].LightPointers[k]);
                }
                trafficGraphicMappingIndex++;
            }
        }

        foreach (HPS_CarGraphicDatas carGraphicData in carGraphicDatas)
            Graphics.DrawMeshInstanced(carGraphicData.Mesh, 0, carGraphicData.Material, carGraphicData.Matrices, carGraphicData.MaterialPropertyBlock, UnityEngine.Rendering.ShadowCastingMode.Off, false);

        #region Original
        //int trafficGraphicMappingIndex = 0;
        //HPS_TrafficCars trafficCar = new();
        //List<KeyValuePair<int, int>> fragments = new();
        //List<KeyValuePair<int, int>> wheels = new();
        //List<KeyValuePair<Matrix4x4, KeyValuePair<int, int>>> tempDatas = new();

        //for (int i = 0; i < trafficGraphicMapping.Count; i++)
        //{
        //    if (i < trafficGraphicMappingIndex)
        //    {
        //        i = trafficGraphicMappingIndex - 1;
        //        continue;
        //    }
        //    else
        //    {
        //        fragments.Clear();
        //        wheels.Clear();
        //        trafficCar = trafficCars.First(trafficCar => trafficCar.trafficCar.name.StartsWith(trafficGraphicMapping[i].TrafficCar.name.Substring(0, 11)));
        //    }

        //    for (int j = i; j < i + trafficCar.frequence; j++)
        //    {
        //        if (!trafficGraphicMapping[j].TrafficCar.gameObject.activeInHierarchy)
        //        {
        //            for (int k = 0; k < trafficGraphicMapping[j].FragmentPointers.Count; k++)
        //                fragments.Add(trafficGraphicMapping[j].FragmentPointers[k]);

        //            for (int k = 0; k < trafficGraphicMapping[j].WheelPointers.Count; k++)
        //                wheels.Add(trafficGraphicMapping[j].WheelPointers[k]);
        //        }
        //        trafficGraphicMappingIndex++;
        //    }
        //}

        //for (int i = 0; i < fragments.Count; i++)
        //{
        //    tempDatas.Add(new KeyValuePair<Matrix4x4, KeyValuePair<int, int>>(carGraphicDatas[fragments[i].Key].Matrices[fragments[i].Value], fragments[i]));
        //    carGraphicDatas[fragments[i].Key].Matrices[fragments[i].Value] = Matrix4x4.zero;
        //}

        //for (int i = 0; i < wheels.Count; i++)
        //{
        //    tempDatas.Add(new KeyValuePair<Matrix4x4, KeyValuePair<int, int>>(carGraphicDatas[wheels[i].Key].Matrices[wheels[i].Value], wheels[i]));
        //    carGraphicDatas[wheels[i].Key].Matrices[wheels[i].Value] = Matrix4x4.zero;
        //}

        //foreach (HPS_CarGraphicDatas carGraphicData in carGraphicDatas)
        //    Graphics.DrawMeshInstanced(carGraphicData.Mesh, 0, carGraphicData.Material, carGraphicData.Matrices, carGraphicData.MaterialPropertyBlock, UnityEngine.Rendering.ShadowCastingMode.Off, false);

        //foreach (var tempData in tempDatas)
        //    carGraphicDatas[tempData.Value.Key].Matrices[tempData.Value.Value] = tempData.Key;

        //yield return null;
        #endregion

    }


}
