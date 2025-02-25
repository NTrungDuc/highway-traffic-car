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
/// Manager for upgradable rims.
/// </summary>
[AddComponentMenu("BoneCracker Games/Highway Racer/Upgrade/HR Rim Manager")]
public class HR_VehicleUpgrade_RimManager : MonoBehaviour
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

        StartCoroutine(CheckRim());

    }

    private IEnumerator CheckRim()
    {

        yield return new WaitForFixedUpdate();

        // If last selected wheel found, change the wheels.
        int rimIndex = PlayerPrefs.GetInt(transform.root.name + "Rim", 0);

        if (rimIndex != 0)
            RCC_Customization.ChangeRims(CarController, HR_Rims.Instance.rims[rimIndex].rim, true);

    }

    /// <summary>
    /// Changes the rim with target rim index.
    /// </summary>
    /// <param name="rimIndex"></param>
    /// <param name="saved"></param>
    public void UpdateRim(int rimIndex, bool saved = true)
    {
        if (saved)
        {
            PlayerPrefs.SetInt(transform.root.name + "Rim", rimIndex);
            PlayerPrefs.SetInt(transform.root.name + "Rim" + rimIndex, 1);
        }

        RCC_Customization.ChangeRims(CarController, HR_Rims.Instance.rims[rimIndex].rim, true);

    }

}
