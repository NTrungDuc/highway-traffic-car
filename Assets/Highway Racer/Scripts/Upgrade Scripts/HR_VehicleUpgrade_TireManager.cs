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
using UnityEngine.UI;

/// <summary>
/// Manager for tires.
/// </summary>
[AddComponentMenu("BoneCracker Games/Highway Racer/Upgrade/HR Tire Manager")]
public class HR_VehicleUpgrade_TireManager : MonoBehaviour
{
    private RCC_CarControllerV3 carController;
    public RCC_CarControllerV3 CarController
    {

        get
        {

            if (carController == null)
                carController = GetComponentInParent<RCC_CarControllerV3>();

            return carController;

        }

    }

    public void Initialize()
    {

        StartCoroutine(CheckTire());

    }

    private IEnumerator CheckTire()
    {

        yield return new WaitForFixedUpdate();

        // If last selected tire found, change the tire.
        int tireIndex = PlayerPrefs.GetInt(transform.root.name + "Tire", 0);

        if (tireIndex != 0)
            RCC_Customization.ChangeTires(CarController, HR_Tires.Instance.tires[tireIndex].tire, true);

    }

    /// <summary>
    /// Changes the tire with target tire index.
    /// </summary>
    /// <param name="tireIndex"></param>
    /// <param name="saved"></param>
    public void UpdateTire(int tireIndex, bool saved = true)
    {
        if (saved)
        {
            PlayerPrefs.SetInt(transform.root.name + "Tire", tireIndex);
            PlayerPrefs.SetInt(transform.root.name + "Tire" + tireIndex, 1);
        }

        RCC_Customization.ChangeTires(CarController, HR_Tires.Instance.tires[tireIndex].tire, true);
    }

}
