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
/// Selectable tires.
/// </summary>
[System.Serializable]
public class HR_Tires : ScriptableObject {
    
    private static HR_Tires instance;
    public static HR_Tires Instance {

        get {

            if (instance == null)
                instance = Resources.Load("HR_Tires") as HR_Tires;

            return instance;

        }

    }

    [System.Serializable]
    public class Tires {

        public GameObject tire;
        public bool unlocked;
        public int price;

    }

    public Tires[] tires;

}
