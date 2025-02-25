//----------------------------------------------
//           	   Highway Racer
//
// Copyright © 2014 - 2023 BoneCracker Games
// http://www.bonecrackergames.com
//
//----------------------------------------------

using UnityEngine;
using System.Collections;

/// <summary>
/// Selectable rims.
/// </summary>
[System.Serializable]
public class HR_Rims : ScriptableObject {

    private static HR_Rims instance;
    public static HR_Rims Instance
    {

        get
        {

            if (instance == null)
                instance = Resources.Load("HR_Rims") as HR_Rims;

            return instance;

        }

    }

    [System.Serializable]
    public class Rims {

        public GameObject rim;
        public bool unlocked;
        public int price;

    }

    public Rims[] rims;

}
