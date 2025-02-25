//----------------------------------------------
//           	   Highway Racer
//
// Copyright © 2014 - 2023 BoneCracker Games
// http://www.bonecrackergames.com
//
//----------------------------------------------

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if PHOTON_UNITY_NETWORKING
using Photon;
using Photon.Pun;
#endif

[AddComponentMenu("BoneCracker Games/Highway Racer/Traffic/HR Traffic Pooling")]
public class HR_TrafficPooling : MonoBehaviour {

    #region SINGLETON PATTERN
    private static HR_TrafficPooling instance;
    public static HR_TrafficPooling Instance {
        get {
            if (instance == null)
                instance = FindObjectOfType<HR_TrafficPooling>();
            return instance;
        }
    }
    #endregion
    private HR_PlayerHandler player;
    public TrafficCars[] trafficCars;       //  Traffic cars.
    public Transform[] lines;       // Traffic lines.

    [System.Serializable]
    public class TrafficCars {

        public GameObject trafficCar;
        public int frequence = 1;

    }

    private List<HR_TrafficCar> _trafficCars = new List<HR_TrafficCar>();       //  Spawned traffic cars.
    internal GameObject container;      //  Container of the spawned traffic cars.

    private int countCarActive = 1;
    public float distanceInterval = 0.5f;
    private float previousDistance = 0f;
    private float timeChangeLine = 0;
    public float maxTimeChangeLine = 10f;
    public float disStartChangeLine = 0.5f;
    public float changeFullLine = 2;
    private void Start() {

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
        HR_GamePlayHandler.OnPlayerRevive += DisableCar;
    }
    private void Update() {

#if PHOTON_UNITY_NETWORKING && BCG_HR_PHOTON

        if (PhotonNetwork.InRoom && !PhotonNetwork.IsMasterClient)
            return;

#endif
        
        if (HR_GamePlayHandler.Instance.gameStarted)
        {
            timeChangeLine += Time.deltaTime;
            //if (HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.OneWay
            //    || HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.TwoWay)
            //{
            //    if (player.distance - previousDistance >= distanceInterval)
            //    {
            //        previousDistance += distanceInterval;
            //        countCarActive++;
            //        if (countCarActive >= _trafficCars.Count)
            //        {
            //            countCarActive = _trafficCars.Count;
            //        }
            //        //Debug.Log("So xe hoat dong: " + countCarActive);
            //    }
            //}
            //else
            //{
                countCarActive = _trafficCars.Count;
            //}
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
    private void CreateTraffic() {

        //  Creating container for the spawned traffic cars.
        container = new GameObject("Traffic Container");

        for (int i = 0; i < trafficCars.Length; i++) {

            //for (int k = 0; k < trafficCars[i].frequence; k++) {

                GameObject go;

                if (PlayerPrefs.GetInt("Multiplayer", 0) == 0) {

                    go = Instantiate(trafficCars[i].trafficCar, Vector3.zero, Quaternion.identity);

                } else {

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

            //}

        }

    }
    private void DisableCar(HR_PlayerHandler player)
    {
        for(int i = 0; i < _trafficCars.Count; i++)
        {
            _trafficCars[i].gameObject.SetActive(false);
        }
    }
    void ShuffleCar(List<HR_TrafficCar> cars)
    {
        for (int i = cars.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            HR_TrafficCar temp = cars[i];
            cars[i] = cars[randomIndex];
            cars[randomIndex] = temp;
        }
    }
    /// <summary>
    /// Animates the traffic cars.
    /// </summary>
    private void AnimateTraffic() {

        //  If there is no camera, return.
        if (!Camera.main)
            return;

        //  If traffic car is below the camera or too far away, realign.
        for (int i = 0; i < countCarActive; i++) {
            //_trafficCars[i].InvokeRepeating("ChangeLines", Random.Range(15, 45), Random.Range(15, 45));
            if (Camera.main.transform.position.z > (_trafficCars[i].transform.position.z + 100) || Camera.main.transform.position.z < (_trafficCars[i].transform.position.z - 300))
                ReAlignTraffic(_trafficCars[i]);

        }

    }
    private void SpawnCarFirst()
    {
        GameObject Car_1 = _trafficCars[0].gameObject;
        Car_1.transform.position = new Vector3(lines[3].position.x, lines[3].position.y, 100);
        Car_1.SetActive(true);
    }
    /// <summary>
    /// Realigns the traffic car.
    /// </summary>
    /// <param name="realignableObject"></param>
    private void ReAlignTraffic(HR_TrafficCar realignableObject) {

        if (!realignableObject.gameObject.activeSelf) {

            realignableObject.gameObject.SetActive(true);

#if PHOTON_UNITY_NETWORKING && BCG_HR_PHOTON

            if (HR_PhotonHandler.Instance)
                HR_PhotonHandler.Instance.EnableTrafficvehicle(realignableObject.gameObject);

#endif

        }

        int randomLine = Random.Range(0, lines.Length);
        realignableObject.Reset();
        realignableObject.currentLine = randomLine;
        realignableObject.transform.position = new Vector3(lines[randomLine].position.x, lines[randomLine].position.y, (Camera.main.transform.position.z + (Random.Range(100, 300))));

        switch (HR_GamePlayHandler.Instance.mode) {

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
            case (HR_GamePlayHandler.Mode.TimeAttack):
                realignableObject.transform.rotation = Quaternion.identity;
                break;
            case (HR_GamePlayHandler.Mode.Bomb):
                realignableObject.transform.rotation = Quaternion.identity;
                break;

        }

        realignableObject.OnReAligned();

        if (CheckIfClipping(realignableObject.triggerCollider)) {

            realignableObject.gameObject.SetActive(false);

#if PHOTON_UNITY_NETWORKING && BCG_HR_PHOTON

            if (HR_PhotonHandler.Instance)
                HR_PhotonHandler.Instance.DisableTrafficvehicle(realignableObject.gameObject);

#endif

        }

    }

    /// <summary>
    /// Checks if the new aligned car is clipping with another traffic car.
    /// </summary>
    /// <param name="trafficCarBound"></param>
    /// <returns></returns>
    private bool CheckIfClipping(BoxCollider trafficCarBound) {

        for (int i = 0; i < _trafficCars.Count; i++) {

            if (!trafficCarBound.transform.IsChildOf(_trafficCars[i].transform) && _trafficCars[i].gameObject.activeSelf) {

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
            int randomIndex=Random.Range(0,countCarActive);
            HR_TrafficCar chosenCar = _trafficCars[randomIndex];
            //Debug.Log(chosenCar);
            if (chosenCar != null)
            {
                chosenCar.ChangeLines();
                if(player.distance > changeFullLine)
                {
                    chosenCar.fullLine = true;
                }
            }
        }
    }
    private void OnDisable()
    {
        HR_GamePlayHandler.OnPlayerSpawned -= HR_PlayerHandler_OnPlayerSpawned;
        HR_GamePlayHandler.OnPlayerRevive -= DisableCar;
    }
}
