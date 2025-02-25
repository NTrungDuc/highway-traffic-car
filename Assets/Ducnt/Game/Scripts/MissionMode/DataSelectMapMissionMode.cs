using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataSelectMapMissionMode : MonoBehaviour
{
    private static DataSelectMapMissionMode instance;
    public static DataSelectMapMissionMode Instance { get { return instance; } }
    public DataMissionMode dataMisionMode;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        // Save to file JSON
        TextAsset jsonTextFile = Resources.Load<TextAsset>("missionModeData");

        // Read from file JSON
        dataMisionMode = JsonUtility.FromJson<DataMissionMode>(jsonTextFile.text);
    }
}
[System.Serializable]
public class SelectMode
{
    public int level;
    public string nameMap;
    public int mode;
    public string nameMode;
}
[System.Serializable]
public class DataMissionMode
{
    public SelectMode[] selectModes;
}

