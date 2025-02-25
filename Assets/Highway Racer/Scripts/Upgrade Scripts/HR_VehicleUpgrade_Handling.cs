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
/// Upgrades traction strength of the car controller.
/// </summary>
[AddComponentMenu("BoneCracker Games/Highway Racer/Upgrade/HR Handling")]
public class HR_VehicleUpgrade_Handling : MonoBehaviour {

    private RCC_CarControllerV3 carController;
    public RCC_CarControllerV3 CarController {

        get {

            if (carController == null)
                carController = GetComponentInParent<RCC_CarControllerV3>();

            return carController;

        }

    }

    private int _handlingLevel = 0;
    public int handlingLevel {
        get {
            return _handlingLevel;
        }
        set {
            if (value <= 5)
                _handlingLevel = value;
        }
    }

    private float defHandling = -1;
    private float maxHandling = .4f;

    public void Initialize() {

        if (defHandling == -1)
            defHandling = CarController.steerHelperAngularVelStrength;
        if(!CarController.isCheckAi && CarController.GetComponent<HR_PlayerHandler>() != null)
            maxHandling = CarController.GetComponent<HR_PlayerHandler>().maxHandlingStrength;
        else if(CarController.isCheckAi && CarController.GetComponent<HR_AICarNew>() != null)
            maxHandling = CarController.GetComponent<HR_AICarNew>().maxHandlingStrength;


        //  Setting upgraded handling strength if saved.
        handlingLevel = PlayerPrefs.GetInt(transform.root.name + "HandlingLevel");
        CarController.steerHelperAngularVelStrength = Mathf.Lerp(defHandling, maxHandling, handlingLevel / 5f);
        CarController.steerHelperLinearVelStrength = Mathf.Lerp(defHandling, maxHandling, handlingLevel / 5f);

    }

    public void Initialize(float defaultHandlingStrength, float maxHandlingStrength)
    {
        defHandling = defaultHandlingStrength;
        
        if (!CarController.isCheckAi && CarController.GetComponent<HR_PlayerHandler>() != null)
            maxHandling = CarController.GetComponent<HR_PlayerHandler>().maxHandlingStrength = maxHandlingStrength;
        else if (CarController.isCheckAi && CarController.GetComponent<HR_AICarNew>() != null)
            maxHandling = CarController.GetComponent<HR_AICarNew>().maxHandlingStrength = maxHandlingStrength;
        //  Setting upgraded handling strength if saved.
        handlingLevel = PlayerPrefs.GetInt(transform.root.name + "HandlingLevel");
        CarController.steerHelperAngularVelStrength = Mathf.Lerp(defHandling, maxHandling, handlingLevel / 5f);
        CarController.steerHelperLinearVelStrength = Mathf.Lerp(defHandling, maxHandling, handlingLevel / 5f);
        if (!CarController.isCheckAi && CarController.GetComponent<HR_PlayerHandler>() != null)
            CarController.GetComponent<HR_PlayerHandler>().currentHandlingStrength = Mathf.Lerp(defHandling, maxHandling, handlingLevel / 5f);
        else if (CarController.isCheckAi && CarController.GetComponent<HR_AICarNew>() != null)
            CarController.GetComponent<HR_AICarNew>().currentHandlingStrength = Mathf.Lerp(defHandling, maxHandling, handlingLevel / 5f);

    }

    /// <summary>
    /// Updates handling strength and save it.
    /// </summary>
    public void UpdateStats() {

        CarController.steerHelperAngularVelStrength = Mathf.Lerp(defHandling, maxHandling, handlingLevel / 5f);
        CarController.steerHelperLinearVelStrength = Mathf.Lerp(defHandling, maxHandling, handlingLevel / 5f);
        PlayerPrefs.SetInt(transform.root.name + "HandlingLevel", handlingLevel);

    }

    private void Update() {

        //  Make sure max handling is not smaller.
        if (maxHandling < CarController.steerHelperAngularVelStrength)
            maxHandling = CarController.steerHelperAngularVelStrength;

    }

}
