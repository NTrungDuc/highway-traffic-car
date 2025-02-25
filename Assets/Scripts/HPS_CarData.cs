using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HPS_CarDatas : ScriptableObject
{
    private static HPS_CarDatas instance;
    public static HPS_CarDatas Instance
    {
        get
        {
            if (instance == null)
                instance = Resources.Load("HPS_CarDatas") as HPS_CarDatas;

            return instance;
        }
    }


    [System.Serializable]
    public class CarDatas
    {
        public string carName = "";
        public string carClass = "S";
        public int stars = 0;
        public float defaultEngineTorque = 200f;
        public float defaultHandlingStrength = .1f;
        public float defaultSpeed = 180f;
        public float defaultNOS = 3f;
        public float maxEngineTorque = 250f;
        public float maxHandlingStrength = .3f;
        public float maxSpeed = 220f;
        public float maxNOS = 10f;

        public bool unlocked = false;
        public int price = 25000;

    }

    public CarDatas[] cardatas;

}