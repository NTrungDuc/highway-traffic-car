using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class TrafficPooling : MonoBehaviour
{
    #region SINGLETON PATTERN
    private static TrafficPooling instance;
    public static TrafficPooling Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<TrafficPooling>();
            return instance;
        }
    }
    #endregion
    private HR_PlayerHandler player;
    public TrafficCars[] trafficCars;       //  Traffic cars.
    public Transform[] lines;       // Traffic lines.

    [System.Serializable]
    public class TrafficCars
    {

        public GameObject trafficCar;
        public int frequence = 1;

    }

    private List<HR_TrafficCar> _trafficCars = new List<HR_TrafficCar>();       //  Spawned traffic cars.
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
    private void Start()
    {

#if PHOTON_UNITY_NETWORKING && BCG_HR_PHOTON

        if (PhotonNetwork.InRoom && !PhotonNetwork.IsMasterClient)
            return;

#endif
        CreateTraffic();
        SpawnCarFirst();
    }
    private void OnEnable()
    {
        HR_GamePlayHandler.OnPlayerSpawned += HR_PlayerHandler_OnPlayerSpawned;
    }
    private void Update()
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
    }
    private void HR_PlayerHandler_OnPlayerSpawned(HR_PlayerHandler _player)
    {

        player = _player;

    }
    /// <summary>
    /// Spawns all traffic cars.
    /// </summary>
    private void CreateTraffic()
    {

        //  Creating container for the spawned traffic cars.
        container = new GameObject("Traffic Container");

        for (int i = 0; i < trafficCars.Length; i++)
        {

            for (int k = 0; k < trafficCars[i].frequence; k++)
            {

                GameObject go;

                if (PlayerPrefs.GetInt("Multiplayer", 0) == 0)
                {

                    go = Instantiate(trafficCars[i].trafficCar, Vector3.zero, Quaternion.identity);

                }
                else
                {

#if PHOTON_UNITY_NETWORKING && BCG_HR_PHOTON
                 go = PhotonNetwork.Instantiate("Photon Traffic Vehicles/" + trafficCars[i].trafficCar.name, Vector3.zero, Quaternion.identity);
#else
                    go = Instantiate(trafficCars[i].trafficCar, Vector3.zero, Quaternion.identity);
#endif

                }

                _trafficCars.Add(go.GetComponent<HR_TrafficCar>());
                ShuffleCar(_trafficCars);
                go.SetActive(false);
                go.transform.SetParent(container.transform, true);

            }

        }

    }
    private void SpawnCarFirst()
    {
        //countCarActive = _trafficCars.Count - 10;
        for (int i = 0; i < countCarActive; i++)
        {
            ReAlignTraffic(_trafficCars[i], true);
        }
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
    private void ReAlignTraffic(HR_TrafficCar realignableObject, bool firstSpawn)
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
            {

                realignableObject.gameObject.SetActive(true);
            }

        }
        else
        {
            //realignableObject.transform.position = new Vector3(lines[randomLine].position.x, lines[randomLine].position.y,
            //    (realignableObject.transform.position.z + (posSpawnAI * countCarActive)));
            for (int i = 0; i < countCarActive; i++)
            {
                if (_trafficCars[i].transform.position.z > maxZPosition)
                {
                    maxZPosition = _trafficCars[i].transform.position.z;
                }
            }
            realignableObject.transform.position = new Vector3(lines[randomLine].position.x, lines[randomLine].position.y,
                (maxZPosition + posSpawnAI));
            //realignableObject.transform.position = new Vector3(lines[randomLine].position.x, lines[randomLine].position.y,
            //    (_trafficCars[countCarActive - 1].transform.position.z + posSpawnAI));
            //random car
            HR_TrafficCar randomCar = _trafficCars[Random.Range(10, randomFullCar)];

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
    IEnumerator ActiveNewCar(HR_TrafficCar oldCar, HR_TrafficCar newCar)
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
        //_trafficCars.RemoveAt(0);
        //_trafficCars.Insert(10, oldCar);
        //_trafficCars.RemoveAt(_trafficCars.IndexOf(newCar));
        //_trafficCars.Insert(9,newCar);
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
            List<HR_TrafficCar> activeCars;
            if (HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.TwoWay
            || HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.MissionMode_2 || PlayerPrefs.GetInt("numberPooling") == 1)
            {
                activeCars = _trafficCars.Where(car => car.gameObject.activeSelf && car.transform.position.x > 0).ToList();
            }
            else
            {
                activeCars = _trafficCars.Where(car => car.gameObject.activeSelf).ToList();
            }
            if (activeCars.Count > 0)
            {
                int randomIndex = Random.Range(0, activeCars.Count);
                HR_TrafficCar chosenCar = activeCars[randomIndex];
                //Debug.Log(chosenCar);
                if (chosenCar != null)
                {
                    chosenCar.fullLine = Random.value > 0.5f;
                    chosenCar.ChangeLines();
                    //if (player.distance > changeFullLine)
                    //{
                    //    chosenCar.fullLine = true;
                    //}
                }
            }


        }
    }
    void Swap(int indexA, int indexB)
    {
        HR_TrafficCar temp = _trafficCars[indexA];
        _trafficCars[indexA] = _trafficCars[indexB];
        _trafficCars[indexB] = temp;
    }
    void ShuffleCar(List<HR_TrafficCar> cars)
    {
        for (int i = cars.Count - (LargeCar + 1); i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            HR_TrafficCar temp = cars[i];
            cars[i] = cars[randomIndex];
            cars[randomIndex] = temp;
        }
    }
    private void OnDisable()
    {
        HR_GamePlayHandler.OnPlayerSpawned -= HR_PlayerHandler_OnPlayerSpawned;
    }
}
