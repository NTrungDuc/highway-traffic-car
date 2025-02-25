using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class DataLevelMissionMode : MonoBehaviour
{
    private static DataLevelMissionMode instance;
    public static DataLevelMissionMode Instance { get { return instance; } }
    public LevelData levelData;
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
    void Start()
    {
        // Save to file JSON
        TextAsset jsonTextFile = Resources.Load<TextAsset>("levelData");

        // Read from file JSON
        levelData = JsonUtility.FromJson<LevelData>(jsonTextFile.text);
    }
}
[System.Serializable]
public class levelGame
{
    //public int MissionCoin;
    public int level;
    public float distanceWin;
    public float time;
}

[System.Serializable]
public class LevelData
{
    public levelGame[] levelGames;
}