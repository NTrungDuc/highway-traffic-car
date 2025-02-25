using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetDefaultQualitySetting : MonoBehaviour
{
    string androidVersion;
    int totalRAM;
    private void Awake()
    {
        if (PlayerPrefs.GetInt("FirstRun") == 0)
        {
            SetQuality();
        }
    }
    void SetQuality()
    {
        androidVersion = SystemInfo.operatingSystem;
        totalRAM = SystemInfo.systemMemorySize;
        //Debug.Log("android " + androidVersion);
        //Debug.Log("ram " + totalRAM);
        if (GetAndroidVersionNumber() < 8 || totalRAM < 2048)
        {
            HR_UIOptionsManager.Instance.SetQuality("Low");
            //Debug.Log("Low");
        }
        if(GetAndroidVersionNumber() < 13 || (totalRAM > 2048 && totalRAM < 4096))
        {
            HR_UIOptionsManager.Instance.SetQuality("Medium");
            //Debug.Log("Medium");
        }
        if(GetAndroidVersionNumber() >= 13 || totalRAM > 4096)
        {
            HR_UIOptionsManager.Instance.SetQuality("High");
            //Debug.Log("High");
        }
    }
    private int GetAndroidVersionNumber()
    {
        string osVersion = SystemInfo.operatingSystem;
        string[] parts = osVersion.Split(' ');
        if (parts.Length > 1)
        {
            string versionStr = parts[1].Split('.')[0];
            int versionNumber;
            if (int.TryParse(versionStr, out versionNumber))
            {
                return versionNumber;
            }
        }
        return 0;
    }
}
