using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class DataRank : MonoBehaviour
{
    private static DataRank instance;
    public static DataRank Instance { get { return instance; } }
    public RankData rankData;
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
        TextAsset jsonTextFile = Resources.Load<TextAsset>("rankData");

        // Read from file JSON
        rankData = JsonUtility.FromJson<RankData>(jsonTextFile.text);
    }
}
[System.Serializable]
public class rankGame
{
    public string rankName;
    public float rankPoint;
}

[System.Serializable]
public class RankData
{
    public rankGame[] rankGames;
}
