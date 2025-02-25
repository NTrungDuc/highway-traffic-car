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

/// <summary>
/// Modification manager used with UI.
/// </summary>
[AddComponentMenu("BoneCracker Games/Highway Racer/Main Menu/HR Mod Handler")]
public class HR_ModHandler : MonoBehaviour {

    #region SINGLETON PATTERN
    private static HR_ModHandler instance;
    public static HR_ModHandler Instance {
        get {
            if (instance == null) {
                instance = FindObjectOfType<HR_ModHandler>();
            }

            return instance;
        }
    }
    #endregion

    //Classes
    private HR_ModApplier currentApplier;       //	Current applier component of the player car.

    //UI Panels.
    [Header("Modify Panels")]
    public GameObject colorClass;
    public GameObject rimClass;
    public GameObject tireClass;
    public GameObject wheelClass;
    public GameObject modificationClass;
    public GameObject upgradesClass;
    public GameObject spoilerClass;
    public GameObject sirenClass;
    public GameObject plateClass;

    //UI Buttons.
    [Header("Modify Buttons")]
    public Button bodyPaintButton;
    public Button rimButton;
    public Button tireButton;
    public Button wheelButton;
    public Button customizationButton;
    public Button upgradeButton;
    public Button spoilersButton;
    public Button sirensButton;
    public Button platesButton;

    private Color orgButtonColor = Color.white;

    //UI Texts.
    [Header("Upgrade Levels Texts")]
    public Text engineUpgradeLevel;
    public Text handlingUpgradeLevel;
    public Text brakeUpgradeLevel;
    public Text nosUpgradeLevel;
    public Text speedUpgradeLevel;

    // UI Sliders.
    [Header("Upgrade Sliders")]
    public Slider engine;
    public Slider handling;
    public Slider brake;
    public Slider speed;
    public Slider nos;

    private void Update() {

        //  Getting HR_ModApplier script of the player car.
        if (HR_MainMenuHandler.Instance.currentApplier)
            currentApplier = HR_MainMenuHandler.Instance.currentApplier;
        else
            currentApplier = null;

        // If no any player car, return.
        if (!currentApplier)
            return;

        // Setting interactable states of the buttons depending on upgrade managers. 
        //	Ex. If spoiler manager not found, spoiler button will be disabled.
        upgradeButton.interactable = currentApplier.UpgradeManager;
        spoilersButton.interactable = currentApplier.SpoilerManager;
        sirensButton.interactable = currentApplier.SirenManager;
        wheelButton.interactable = currentApplier.WheelManager;
        bodyPaintButton.interactable = currentApplier.PaintManager;

        // Feeding upgrade level texts for enigne, brake, handling.
        if (currentApplier.UpgradeManager) {

            if (engineUpgradeLevel)
                engineUpgradeLevel.text = currentApplier.UpgradeManager.engineLevel.ToString("F0");
            if (handlingUpgradeLevel)
                handlingUpgradeLevel.text = currentApplier.UpgradeManager.handlingLevel.ToString("F0");
            if (brakeUpgradeLevel)
                brakeUpgradeLevel.text = currentApplier.UpgradeManager.brakeLevel.ToString("F0");
            if (nosUpgradeLevel)
                nosUpgradeLevel.text = currentApplier.UpgradeManager.nosLevel.ToString("F0");
            if (speedUpgradeLevel)
                speedUpgradeLevel.text = currentApplier.UpgradeManager.speedLevel.ToString("F0");

        }

        //  Displaying stats of the current car if found.
        if (currentApplier) {

            engine.value = Mathf.Lerp(.1f, 1f, (currentApplier.CarController.maxEngineTorque) / 1000f);
            handling.value = Mathf.Lerp(.1f, 1f, currentApplier.CarController.steerHelperAngularVelStrength / .5f);
            brake.value = Mathf.Lerp(.1f, 1f, currentApplier.CarController.brakeTorque / 6000f);
            speed.value = Mathf.Lerp(.1f, 1f, currentApplier.CarController.maxspeed / 400f);
            nos.value = Mathf.Lerp(.1f, 1f, currentApplier.CarController.NoS / 160f);

        } else {

            engine.value = 0;
            handling.value = 0;
            brake.value = 0;
            speed.value = 0;
            nos.value = 0;

        }

    }

    /// <summary>
    /// Opens up the taget class panel.
    /// </summary>
    /// <param name="activeClass"></param>
    public void ChooseClass(GameObject activeClass) {

        colorClass.SetActive(false);
        rimClass.SetActive(false);
        tireClass.SetActive(false);
        wheelClass.SetActive(false);
        modificationClass.SetActive(false);
        upgradesClass.SetActive(false);
        spoilerClass.SetActive(false);
        sirenClass.SetActive(false);
        plateClass.SetActive(false);

        if (activeClass)
            activeClass.SetActive(true);

        CheckButtonColors(null);

    }

    /// <summary>
    /// Checks colors of the UI buttons. Ex. If paint class is enabled, color of the button will be green. 
    /// </summary>
    /// <param name="activeButton"></param>
    public void CheckButtonColors(Button activeButton) {

        bodyPaintButton.image.color = orgButtonColor;
        rimButton.image.color = orgButtonColor;
        tireButton.image.color = orgButtonColor;
        wheelButton.image.color = orgButtonColor;
        customizationButton.image.color = orgButtonColor;
        upgradeButton.image.color = orgButtonColor;
        spoilersButton.image.color = orgButtonColor;
        sirensButton.image.color = orgButtonColor;
        platesButton.image.color = orgButtonColor;

        if (activeButton)
            activeButton.image.color = new Color(0f, 1f, 0f);

    }

    /// <summary>
    /// Sets auto rotation of the showrooom camera.
    /// </summary>
    /// <param name="state"></param>
    public void ToggleAutoRotation(bool state) {

        Camera.main.gameObject.GetComponent<HR_ShowroomCamera>().ToggleAutoRotation(state);

    }

    /// <summary>
    /// Sets horizontal angle of the showroom camera.
    /// </summary>
    /// <param name="hor"></param>
    public void SetHorizontal(float hor) {

        Camera.main.gameObject.GetComponent<HR_ShowroomCamera>().orbitX = hor;

    }

    /// <summary>
    /// Sets vertical angle of the showroom camera.
    /// </summary>
    /// <param name="ver"></param>
    public void SetVertical(float ver) {

        Camera.main.gameObject.GetComponent<HR_ShowroomCamera>().orbitY = ver;

    }

}
