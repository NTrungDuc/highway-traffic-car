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
/// Upgrades NOS of the car controller.
/// </summary>
[AddComponentMenu("BoneCracker Games/Highway Racer/Upgrade/HR NOS")]
public class HR_VehicleUpgrade_NOS : MonoBehaviour {

    private RCC_CarControllerV3 carController;
    public RCC_CarControllerV3 CarController {

        get {

            if (carController == null)
                carController = GetComponentInParent<RCC_CarControllerV3>();

            return carController;

        }

    }

    private int _nosLevel = 0;
    public int nosLevel
    {
        get
        {
            return _nosLevel;
        }
        set
        {
            if (value <= 5)
                _nosLevel = value;
        }
    }

    private float defNOS = -1;
    private float maxNOS = 10f;

    public void Initialize() {

        //  Setting NOS if saved.

        CarController.canNitro = true;

        if (defNOS == -1)
            defNOS = CarController.NitroTime;
        if (!CarController.isCheckAi && CarController.GetComponent<HR_PlayerHandler>() != null)
            maxNOS = CarController.GetComponent<HR_PlayerHandler>().maxNOS;
        else if (CarController.isCheckAi && CarController.GetComponent<HR_AICarNew>() != null)
            maxNOS = CarController.GetComponent<HR_AICarNew>().maxNOS;


        nosLevel = PlayerPrefs.GetInt(transform.root.name + "NOSLevel");
        CarController.NitroTime = Mathf.Lerp(defNOS, maxNOS, nosLevel / 5f);

    }

    public void Initialize(float defaultNOS, float maxNOS)
    {
        CarController.canNitro = true;

        defNOS = defaultNOS;
        if (!CarController.isCheckAi && CarController.GetComponent<HR_PlayerHandler>() != null)
            this.maxNOS = CarController.GetComponent<HR_PlayerHandler>().maxNOS = maxNOS;
        else if (CarController.isCheckAi && CarController.GetComponent<HR_AICarNew>() != null)
            this.maxNOS = CarController.GetComponent<HR_AICarNew>().maxNOS = maxNOS;

        nosLevel = PlayerPrefs.GetInt(transform.root.name + "NOSLevel");
        CarController.NitroTime = Mathf.Lerp(defNOS, this.maxNOS, nosLevel / 5f);
        if (!CarController.isCheckAi && CarController.GetComponent<HR_PlayerHandler>() != null)
            CarController.GetComponent<HR_PlayerHandler>().currentNOS = Mathf.Lerp(defNOS, this.maxNOS, nosLevel / 5f);
        else if (CarController.isCheckAi && CarController.GetComponent<HR_AICarNew>() != null)
            CarController.GetComponent<HR_AICarNew>().currentNOS = Mathf.Lerp(defNOS, this.maxNOS, nosLevel / 5f);
    }

    /// <summary>
    /// Upgrades NOS and save it.
    /// </summary>
    public void UpdateStats() {

        if (!carController)
            return;

        CarController.NitroTime = Mathf.Lerp(defNOS, maxNOS, nosLevel / 5f);
        PlayerPrefs.SetInt(transform.root.name + "NOSLevel", nosLevel);

    }

    private void Update()
    {
        if (!carController)
            return;

        if (maxNOS < CarController.NitroTime)
            maxNOS = CarController.NitroTime;
    }

}
