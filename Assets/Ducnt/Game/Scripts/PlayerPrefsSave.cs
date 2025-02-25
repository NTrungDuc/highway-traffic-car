using System;
using UnityEngine;

public class PlayerPrefsSave : MonoBehaviour
{    
    public static bool IsAcceptNotiAndroid13
    {
        get => PlayerPrefs.GetInt("IsAcceptNotiAndroid13", 0) == 1;
        set => PlayerPrefs.SetInt("IsAcceptNotiAndroid13", value ? 1 : 0);
    }
}
