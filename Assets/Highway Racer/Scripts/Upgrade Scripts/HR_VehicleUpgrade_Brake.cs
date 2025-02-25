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
/// Upgrades brake torque of the car controller.
/// </summary>
[AddComponentMenu("BoneCracker Games/Highway Racer/Upgrade/HR Brake")]
public class HR_VehicleUpgrade_Brake : MonoBehaviour {

    private RCC_CarControllerV3 carController;
    public RCC_CarControllerV3 CarController {

        get {

            if (carController == null)
                carController = GetComponentInParent<RCC_CarControllerV3>();

            return carController;

        }

    }

    private int _brakeLevel = 0;
    public int brakeLevel {
        get {
            return _brakeLevel;
        }
        set {
            if (value <= 5)
                _brakeLevel = value;
        }
    }

    private float defBrake = -1;
    private float maxBrake = 4000f;

    public void Initialize() {

        if (defBrake == -1)
            defBrake = CarController.brakeTorque;

        maxBrake = CarController.GetComponent<HR_PlayerHandler>().maxBrakeTorque;

        //  Setting upgraded brake torque if saved.
        brakeLevel = PlayerPrefs.GetInt(transform.root.name + "BrakeLevel");
        CarController.brakeTorque = Mathf.Lerp(defBrake, maxBrake, brakeLevel / 5f);

    }

    public void Initialize(float defaultBreakTorque, float maxBreakTorque)
    {
        defBrake = defaultBreakTorque;
        maxBrake = CarController.GetComponent<HR_PlayerHandler>().maxBrakeTorque = maxBreakTorque;

        //  Setting upgraded brake torque if saved.
        brakeLevel = PlayerPrefs.GetInt(transform.root.name + "BrakeLevel");
        CarController.brakeTorque = Mathf.Lerp(defBrake, maxBrake, brakeLevel / 5f);
        CarController.GetComponent<HR_PlayerHandler>().currentBrakeTorque = Mathf.Lerp(defBrake, maxBrake, brakeLevel / 5f);

    }

    /// <summary>
    /// Updates brake torque and save it.
    /// </summary>
    public void UpdateStats() {

        CarController.brakeTorque = Mathf.Lerp(defBrake, maxBrake, brakeLevel / 5f);
        PlayerPrefs.SetInt(transform.root.name + "BrakeLevel", brakeLevel);

    }

    private void Update() {

        if (!carController)
            return;

        //  Make sure max brake is not smaller.
        if (maxBrake < CarController.brakeTorque)
            maxBrake = CarController.brakeTorque;

    }

}
