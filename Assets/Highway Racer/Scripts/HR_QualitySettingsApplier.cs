//----------------------------------------------
//           	   Highway Racer
//
// Copyright © 2014 - 2023 BoneCracker Games
// http://www.bonecrackergames.com
//
//----------------------------------------------

using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;

/// <summary>
/// Camera settings applier.
/// </summary>
[AddComponentMenu("BoneCracker Games/Highway Racer/Camera/HR Camera Options Applier")]
public class HR_QualitySettingsApplier : MonoBehaviour {

    #region SINGLETON PATTERN
    private static HR_QualitySettingsApplier instance;
    public static HR_QualitySettingsApplier Instance {
        get {
            if (instance == null) {
                instance = FindObjectOfType<HR_QualitySettingsApplier>();
            }

            return instance;
        }
    }
    #endregion

    private void Start() {

        Check();

    }

    /// <summary>
    /// Checks the saved properties and applies them.
    /// </summary>
    public void Check() {

        if (Camera.main) {

            int drawD = PlayerPrefs.GetInt("DrawDistance", 300);
            Camera.main.farClipPlane = drawD;

        }

        AudioListener.volume = PlayerPrefs.GetFloat("MasterVolume", 1f);

        if (HPS_MainMenuHandler.Instance)
            HPS_MainMenuHandler.Instance.mainMenuSoundtrack.volume = PlayerPrefs.GetFloat("MusicVolume", 1f);

    }

    private void OnEnable() {

        //  Listening an event when options are changed.
        HR_UIOptionsManager.OnOptionsChanged += OptionsManager_OnOptionsChanged;

    }

    public void OptionsManager_OnOptionsChanged() {

        //  Checks the saved properties and applies them.
        Check();

    }

    private void OnDisable() {

        HR_UIOptionsManager.OnOptionsChanged -= OptionsManager_OnOptionsChanged;

    }

}
