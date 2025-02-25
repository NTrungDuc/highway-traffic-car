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
/// Manager for upgradable wheels.
/// </summary>
[AddComponentMenu("BoneCracker Games/Highway Racer/Upgrade/HR Wheel Manager")]
public class HR_VehicleUpgrade_WheelManager : MonoBehaviour {

    private RCC_CarControllerV3 carController;
    public RCC_CarControllerV3 CarController {

        get {

            if (carController == null)
                carController = GetComponentInParent<RCC_CarControllerV3>();

            return carController;

        }

    }

    public void Initialize() {

        StartCoroutine(CheckWheel());

    }

    private IEnumerator CheckWheel() {

        yield return new WaitForFixedUpdate();

        // If last selected wheel found, change the wheels.
        int wheelIndex = PlayerPrefs.GetInt(transform.root.name + "Wheel", -1);

        if (wheelIndex != -1)
            RCC_Customization.ChangeWheels(CarController, HR_Wheels.Instance.wheels[wheelIndex].wheel, true);

    }

    /// <summary>
    /// Changes the wheel with target wheel index.
    /// </summary>
    /// <param name="wheelIndex"></param>
    public void UpdateWheel(int wheelIndex) {

        PlayerPrefs.SetInt(transform.root.name + "Wheel", wheelIndex);
        PlayerPrefs.SetInt(transform.root.name + "Wheel" + wheelIndex, 1);
        RCC_Customization.ChangeWheels(CarController, HR_Wheels.Instance.wheels[wheelIndex].wheel, true);

    }

}
