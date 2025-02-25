//----------------------------------------------
//           	   Highway Racer
//
// Copyright © 2014 - 2023 BoneCracker Games
// http://www.bonecrackergames.com
//
//----------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manager for all upgradable scripts (Engine, Brake, Handling).
/// </summary>
[AddComponentMenu("BoneCracker Games/Highway Racer/Upgrade/HR Upgrade Manager")]
public class HR_VehicleUpgrade_UpgradeManager : MonoBehaviour {

    private HR_VehicleUpgrade_Engine engine;        //  Upgradable engine.
    private HR_VehicleUpgrade_Handling handling;        //  Upgradable handling.
    private HR_VehicleUpgrade_NOS nos;      //  Upgradable nos.
    private HR_VehicleUpgrade_Speed speed;      //  Upgradable speed.

    internal int engineLevel = 0;       //  Current upgraded engine level.
    internal int brakeLevel = 0;        //  Current upgraded brake level.
    internal int handlingLevel = 0;     //  Current upgraded handling level.
    internal int nosLevel = 0;     //  Current upgraded nos level.
    internal int speedLevel = 0;        //  Current upgraded speed level.
    internal HPS_CarDatas.CarDatas carDatas;

    public void Initialize() {

        carDatas = HPS_API.GetCarDatas(transform.root.name.Replace("(Clone)", ""));

        //  Getting engine, brake, and handling upgrade scripts.
        engine = GetComponentInChildren<HR_VehicleUpgrade_Engine>();
        handling = GetComponentInChildren<HR_VehicleUpgrade_Handling>();
        nos = GetComponentInChildren<HR_VehicleUpgrade_NOS>();
        speed = GetComponentInChildren<HR_VehicleUpgrade_Speed>();

        if (engine && carDatas == null)
            engine.Initialize();
        else
            engine.Initialize(carDatas.defaultEngineTorque, carDatas.maxEngineTorque);

        if (handling && carDatas == null)
            handling.Initialize();
        else
            handling.Initialize(carDatas.defaultHandlingStrength, carDatas.maxHandlingStrength);

        if (nos && carDatas == null)
            nos.Initialize();
        else
            nos.Initialize(carDatas.defaultNOS, carDatas.maxNOS);

        if (speed && carDatas == null)
            speed.Initialize();
        else
            speed.Initialize(carDatas.defaultSpeed, carDatas.maxSpeed);


    }

    private void Update() {

        //  Getting current upgrade levels

        if (engine)
            engineLevel = engine.engineLevel;

        if (handling)
            handlingLevel = handling.handlingLevel;

        if (nos)
            nosLevel = nos.nosLevel;

        if (speed)
            speedLevel = speed.speedLevel;

    }

    /// <summary>
    /// Upgrades the engine torque.
    /// </summary>
    public void UpgradeEngine() {

        if (!engine)
            return;

        engine.engineLevel++;
        engine.UpdateStats();

    }

    /// <summary>
    /// Upgrades the traction helper (Handling).
    /// </summary>
    public void UpgradeHandling() {

        if (!handling)
            return;

        handling.handlingLevel++;
        handling.UpdateStats();

    }

    /// <summary>
    /// Upgrades the NOS.
    /// </summary>
    public void UpgradeNOS() {

        if (!nos)
            return;

        nos.nosLevel++;
        nos.UpdateStats();

    }

    /// <summary>
    /// Upgrades the max speed.
    /// </summary>
    public void UpgradeSpeed() {

        if (!speed)
            return;

        speed.speedLevel++;
        speed.UpdateStats();

    }

    private void Reset() {

        if (transform.Find("Engine")) {

            engine = transform.Find("Engine").gameObject.GetComponent<HR_VehicleUpgrade_Engine>();

        } else {

            GameObject newEngine = new GameObject("Engine");
            newEngine.transform.SetParent(transform);
            newEngine.transform.localPosition = Vector3.zero;
            newEngine.transform.localRotation = Quaternion.identity;
            engine = newEngine.AddComponent<HR_VehicleUpgrade_Engine>();

        }

        if (transform.Find("Handling")) {

            handling = transform.Find("Handling").gameObject.GetComponent<HR_VehicleUpgrade_Handling>();

        } else {

            GameObject newHandling = new GameObject("Handling");
            newHandling.transform.SetParent(transform);
            newHandling.transform.localPosition = Vector3.zero;
            newHandling.transform.localRotation = Quaternion.identity;
            handling = newHandling.AddComponent<HR_VehicleUpgrade_Handling>();

        }

        if (transform.Find("NOS")) {

            nos = transform.Find("NOS").gameObject.GetComponent<HR_VehicleUpgrade_NOS>();

        } else {

            GameObject newNOS = new GameObject("NOS");
            newNOS.transform.SetParent(transform);
            newNOS.transform.localPosition = Vector3.zero;
            newNOS.transform.localRotation = Quaternion.identity;
            nos = newNOS.AddComponent<HR_VehicleUpgrade_NOS>();

        }

        if (transform.Find("Speed")) {

            speed = transform.Find("Speed").gameObject.GetComponent<HR_VehicleUpgrade_Speed>();

        } else {

            GameObject newSpeed = new GameObject("Speed");
            newSpeed.transform.SetParent(transform);
            newSpeed.transform.localPosition = Vector3.zero;
            newSpeed.transform.localRotation = Quaternion.identity;
            speed = newSpeed.AddComponent<HR_VehicleUpgrade_Speed>();

        }

    }

}
