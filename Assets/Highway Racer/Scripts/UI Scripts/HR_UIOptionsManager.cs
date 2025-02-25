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
using UnityEngine.SceneManagement;
using static HR_GamePlayHandler;

/// <summary>
/// Options manager that handles quality, gameplay, and controller settings.
/// </summary>
[AddComponentMenu("BoneCracker Games/Highway Racer/UI/HR UI Options Manager")]
public class HR_UIOptionsManager : MonoBehaviour {

    #region SINGLETON PATTERN
    private static HR_UIOptionsManager instance;
    public static HR_UIOptionsManager Instance {
        get {
            if (instance == null) {
                instance = FindObjectOfType<HR_UIOptionsManager>();
            }

            return instance;
        }
    }
    #endregion

    public Slider musicVolume;
    public Slider masterVolume;

    public delegate void OptionsChanged();
    public static event OptionsChanged OnOptionsChanged;

    private void OnEnable() {

        masterVolume.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
        musicVolume.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
        SetMasterVolume(masterVolume);
        SetMusicVolume(musicVolume);

    }

    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void OnUpdate() {

        if (OnOptionsChanged != null)
            OnOptionsChanged();

    }

    public void SetControllerType(string mode) {

        switch (mode) {

            case "Touchscreen":
                PlayerPrefs.SetInt("ControllerType", 0);
                RCC.SetMobileController(RCC_Settings.MobileController.TouchScreen);
                break;
            case "Accelerometer":
                PlayerPrefs.SetInt("ControllerType", 1);
                RCC.SetMobileController(RCC_Settings.MobileController.Gyro);
                break;
            //case "SteeringWheel":
            //    PlayerPrefs.SetInt("ControllerType", 2);
            //    RCC.SetMobileController(RCC_Settings.MobileController.SteeringWheel);
            //    break;
            //case "Joystick":
            //    PlayerPrefs.SetInt("ControllerType", 3);
            //    RCC.SetMobileController(RCC_Settings.MobileController.Joystick);
            //    touch.isOn = false;
            //    tilt.isOn = false;
            //    joystick.isOn = true;
            //    break;

        }

        if (OnOptionsChanged != null)
            OnOptionsChanged();

    }

    public void SetMasterVolume(Slider slider) {

        PlayerPrefs.SetFloat("MasterVolume", slider.value);

        if (OnOptionsChanged != null)
            OnOptionsChanged();

    }

    public void SetMusicVolume(Slider slider) {

        PlayerPrefs.SetFloat("MusicVolume", slider.value);

        if (OnOptionsChanged != null)
            OnOptionsChanged();

    }

    public void SetQuality(string level) {

        switch (level) {

            case "Low":
                PlayerPrefs.SetInt("QualityLevel", 0);
                QualitySettings.SetQualityLevel(0);
                break;
            case "Medium":
                PlayerPrefs.SetInt("QualityLevel", 1);
                QualitySettings.SetQualityLevel(1);
                break;
            case "High":
                PlayerPrefs.SetInt("QualityLevel", 2);
                QualitySettings.SetQualityLevel(2);
                break;

        }

        if (OnOptionsChanged != null)
            OnOptionsChanged();
    }

    public void QuitGame() {

        Application.Quit();

    }

}
