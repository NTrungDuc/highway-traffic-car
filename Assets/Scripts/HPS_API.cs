using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

public class HPS_API : MonoBehaviour
{

    /// <summary>
    /// Save data to a file (overwrite completely)
    /// </summary>
    /// <param name="folder"></param>
    /// <param name="file"></param>
    public static void Save(string folder, string file)
    {
        // get the data path of this save data
        string dataPath = GetFilePath(folder, file);

        string jsonData = JsonUtility.ToJson(HPS_CarDatas.Instance, true);

        // create the file in the path if it doesn't exist
        // if the file path or name does not exist, return the default SO
        if (!Directory.Exists(Path.GetDirectoryName(dataPath)))
            Directory.CreateDirectory(Path.GetDirectoryName(dataPath));

        // attempt to save here data
        try
        {
            // save datahere
            File.WriteAllText(dataPath, jsonData);
        }
        catch (Exception e)
        {
            // write out error here
            Debug.LogError("Failed to save data to: " + dataPath);
            Debug.LogError("Error " + e.Message);
        }
    }

    /// <summary>
    /// Load all data at a specified file and folder location
    /// </summary>
    /// <param name="folder"></param>
    /// <param name="file"></param>
    /// <returns></returns>
    public static void Load(string folder, string file)
    {
        // get the data path of this save data
        string dataPath = GetFilePath(folder, file);

        // if the file path or name does not exist, return the default SO
        if (!Directory.Exists(Path.GetDirectoryName(dataPath)))
            Debug.LogWarning("File or path does not exist! " + dataPath);

        // load in the save data as byte array
        string jsonData = null;

        try
        {
            jsonData = File.ReadAllText(dataPath);
        }
        catch (Exception e)
        {
            Debug.LogWarning("Failed to load data from: " + dataPath);
            Debug.LogWarning("Error: " + e.Message);
        }

        // convert to the specified object type
        JsonUtility.FromJsonOverwrite(jsonData, HPS_CarDatas.Instance);
    }

    /// <summary>
    /// Create file path for where a file is stored on the specific platform given a folder name and file name
    /// </summary>
    /// <param name="FolderName"></param>
    /// <param name="FileName"></param>
    /// <returns></returns>
    private static string GetFilePath(string FolderName, string FileName = "")
    {
        string filePath;
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        // mac
        filePath = Path.Combine(Application.streamingAssetsPath, ("data/" + FolderName));

        if (FileName != "")
            filePath = Path.Combine(filePath, (FileName + ".json"));
#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        // windows
        filePath = Path.Combine(Application.persistentDataPath, ("data/" + FolderName));

        if (FileName != "")
            filePath = Path.Combine(filePath, (FileName + ".json"));
#elif UNITY_ANDROID
        // android
        filePath = Path.Combine(Application.persistentDataPath, ("data/" + FolderName));

        if(FileName != "")
            filePath = Path.Combine(filePath, (FileName + ".json"));
#elif UNITY_IOS
        // ios
        filePath = Path.Combine(Application.persistentDataPath, ("data/" + FolderName));

        if(FileName != "")
            filePath = Path.Combine(filePath, (FileName + ".json"));
#endif
        return filePath;
    }

    /// <summary>
    /// Gets the corresponding HPS_CarDatas.CarDatas with provided carName 
    /// </summary>
    /// <param name="carName"></param>
    public static HPS_CarDatas.CarDatas GetCarDatas(string carName)
    {
        Load("Resources", "HPS_CarDatas");
        if (!HPS_CarDatas.Instance.cardatas.Any(carData => carData.carName == carName))
            return null;
        else
            return HPS_CarDatas.Instance.cardatas.Where(carData => carData.carName == carName).FirstOrDefault();
    }

    /// <summary>
    /// Unlocks target vehicle.
    /// </summary>
    /// <param name="index"></param>
    public static void UnlockVehice(int index)
    {

        PlayerPrefs.SetInt(HPS_CarDatas.Instance.cardatas[index].carName + "Owned", 1);

    }

    /// <summary>
    /// Is this vehicle purchased?
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public static bool OwnedVehicle(int index)
    {

        if (PlayerPrefs.HasKey(HPS_CarDatas.Instance.cardatas[index].carName + "Owned"))
            return true;
        else
            return false;

    }

    /// <summary>
    /// Is this vehicle purchased?
    /// </summary>
    /// <param name="carName"></param>
    /// <returns></returns>
    public static bool OwnedVehicle(string carName)
    {

        if (PlayerPrefs.HasKey(carName + "Owned"))
            return true;
        else
            return false;

    }

    /// <summary>
    /// Gets the current currency as an int.
    /// </summary>
    /// <returns></returns>
    public static int GetCurrency()
    {

        return PlayerPrefs.GetInt("CurrencyGold", 0);

    }

}
