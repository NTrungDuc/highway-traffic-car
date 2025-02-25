//----------------------------------------------
//           	   Highway Racer
//
// Copyright © 2014 - 2023 BoneCracker Games
// http://www.bonecrackergames.com
//
//----------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[AddComponentMenu("BoneCracker Games/Highway Racer/UI/HR UI Mobile Controller Type")]
public class HR_UIController_Type : MonoBehaviour {

    public ControllerType _controllerType;
    public enum ControllerType { keypad, accelerometer }

    private Button sprite;
    private Color defCol;

    private void Start() {

        sprite = GetComponent<Button>();
        defCol = sprite.image.color;

        if (!PlayerPrefs.HasKey("ControllerType"))
            PlayerPrefs.SetInt("ControllerType", 0);

        Check();

    }


    public void OnClick() {

        if (_controllerType == ControllerType.keypad) {

            PlayerPrefs.SetInt("ControllerType", 0);
            RCC.SetMobileController(RCC_Settings.MobileController.TouchScreen);

        }

        if (_controllerType == ControllerType.accelerometer) {

            PlayerPrefs.SetInt("ControllerType", 1);
            RCC.SetMobileController(RCC_Settings.MobileController.Gyro);

        }

        HR_UIController_Type[] ct = FindObjectsOfType<HR_UIController_Type>();

        foreach (HR_UIController_Type cts in ct)
            cts.Check();

    }

    private void Check() {

        if (PlayerPrefs.GetInt("ControllerType") == 0) {

            if (_controllerType == ControllerType.keypad)
                sprite.image.color = new Color(.667f, 1f, 0f);

            if (_controllerType == ControllerType.accelerometer)
                sprite.image.color = defCol;

        }

        if (PlayerPrefs.GetInt("ControllerType") == 1) {

            if (_controllerType == ControllerType.keypad)
                sprite.image.color = defCol;

            if (_controllerType == ControllerType.accelerometer)
                sprite.image.color = new Color(.667f, 1f, 0f);

        }

    }

}
