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

/// <summary>
/// Pooling the road with given amount. Calculates total length of the pool, and translates previous roads to the next position.
/// </summary>
[AddComponentMenu("BoneCracker Games/Highway Racer/Road/HR Road Pooling")]
public class HR_RoadPooling : MonoBehaviour {

    #region SINGLETON PATTERN
    private static HR_RoadPooling instance;
    public static HR_RoadPooling Instance {
        get {
            if (instance == null)
                instance = FindObjectOfType<HR_RoadPooling>();
            return instance;
        }
    }
    #endregion

    [System.Serializable]
    public class RoadObjects {

        public GameObject roadObject;

    }

    public int roadAmountInPool = 5;       //  How many roads will be used in the pool?
    private float[] roadLength;     //  Length of the roads.

    public bool automaticRoadLength = true;     //  Calculates total length of the road automatically.
    public float manualRoadLength = 60f;        //  Manual length of the road.

    [Header("Use This Layer On Road For Calculating Road Length")]
    public LayerMask asphaltLayer;      //  Asphalt layer used for calculating the target meshes only.

    [Header("Pooling Road Objects. Select Them While They Are On Your Scene")]
    public RoadObjects[] roadObjects;
    public RoadObjects[] listRoadObectjs;
    internal List<GameObject> roads = new List<GameObject>();

    internal GameObject allRoads;       //  All spawned roads in the pool.
    private int index = 0;
    private void Awake()
    {
        int randomRoad = Random.Range(0, listRoadObectjs.Length);
        Debug.Log("Map " + (randomRoad + 1));
        roadObjects[0] = listRoadObectjs[randomRoad];
    }
    private void Start() {
        
        //  Getting length of the each road.
        roadLength = new float[roadObjects.Length];

        for (int i = 0; i < roadObjects.Length; i++) {

            if (automaticRoadLength)
                roadLength[i] = GetRoadLength(roadObjects[i].roadObject);
            else
                roadLength[i] = manualRoadLength;

        }

        //  Creating the roads.
        CreateRoads();

    }

    /// <summary>
    /// Length of the road.
    /// </summary>
    /// <param name="road"></param>
    /// <returns></returns>
    private float GetRoadLength(GameObject road) {

        GameObject roadReference = Instantiate(road, Vector3.zero, Quaternion.identity);

        Bounds combinedBounds = roadReference.GetComponentInChildren<Renderer>().bounds;
        Renderer[] renderers = roadReference.GetComponentsInChildren<Renderer>();

        foreach (Renderer render in renderers) {

            if (render != roadReference.GetComponent<Renderer>() && 1 << render.gameObject.layer == asphaltLayer)
                combinedBounds.Encapsulate(render.bounds);

        }

        Destroy(roadReference);
        //return combinedBounds.size.x;
        return combinedBounds.size.z;

    }

    /// <summary>
    /// Creates all roads.
    /// </summary>
    private void CreateRoads() {

        allRoads = new GameObject("All Roads");

        for (int i = 0; i < roadAmountInPool; i++) {

            for (int k = 0; k < roadObjects.Length; k++) {

                GameObject go = Instantiate(roadObjects[k].roadObject, roadObjects[k].roadObject.transform.position, roadObjects[k].roadObject.transform.rotation);
                go.isStatic = false;
                roads.Add(go);
                HR_SetLightmapsManually.AlignLightmaps(roadObjects[k].roadObject, go);
                go.transform.SetParent(allRoads.transform);

            }

        }

        for (int i = 0; i < roads.Count; i++) {

            if (i != 0)
                roads[i].transform.position = new Vector3(0f, roads[i].transform.position.y, roads[i - 1].transform.position.z + roadLength[(index <= 0) ? roadObjects.Length - 1 : index - 1]);

            index++;

            if (index >= roadObjects.Length)
                index = 0;

        }

        //for (int j = 0; j < roadObjects.Length; j++) {

        //    if (roadObjects[j].roadObject.activeSelf)
        //        roadObjects[j].roadObject.SetActive(false);

        //}
        for (int i = 0; i < listRoadObectjs.Length; i++)
        {
            if (listRoadObectjs[i].roadObject.activeSelf)
                listRoadObectjs[i].roadObject.SetActive(false);
        }
        index = 0;

    }

    private void Update() {

        // Animating the roads.
        AnimateRoads();

    }

    /// <summary>
    /// Animating the roads.
    /// </summary>
    private void AnimateRoads() {

        if (!Camera.main)
            return;

        for (int i = 0; i < roads.Count; i++) {

            if (Camera.main.transform.position.z > (roads[i].transform.position.z + (roadLength[index] * 2f)))
                roads[i].transform.position = new Vector3(0f, roads[i].transform.position.y, (roads[i].transform.position.z + (roadLength[index] * roads.Count)));

            index++;

            if (index >= roadObjects.Length)
                index = 0;

        }

    }

}
