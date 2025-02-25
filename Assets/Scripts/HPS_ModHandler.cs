using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HPS_ModHandler : MonoBehaviour
{
    #region SINGLETON PATTERN
    private static HPS_ModHandler instance;
    public static HPS_ModHandler Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<HPS_ModHandler>();
            }

            return instance;
        }
    }
    #endregion

    //Classes
    private HR_ModApplier currentApplier;       //	Current applier component of the player car.

    //UI Panels.
    [Header("Modify Panels")]
    public GameObject mainsClass;
    [Space]
    public GameObject upgradesClass;
    public GameObject colorClass;
    public GameObject plateClass;
    public GameObject tireClass;
    public GameObject rimClass;
    public GameObject spoilerClass;

    //UI Buttons.
    [Header("Modify Buttons")]
    public Button upgradeButton;
    public Button bodyPaintButton;
    public Button platesButton;
    public Button tireButton;
    public Button rimButton;
    public Button spoilersButton;

    private Color orgButtonColor = new (1f, 1f, 1f, 0f);

    //UI Texts.
    [Header("Upgrade Levels")]
    public GameObject speedUpgradeLevel;
    public GameObject speedUpgradeValue;
    public GameObject engineUpgradeLevel;
    public GameObject engineUpgradeValue;
    public GameObject handlingUpgradeLevel;
    public GameObject handlingUpgradeValue;
    public GameObject nosUpgradeLevel;
    public GameObject nosUpgradeValue;

    // UI Sliders.
    [Header("Upgrade Sliders")]
    public Slider defSpeed;
    public Slider speed;
    public Slider defEngine;
    public Slider engine;
    public Slider defHandling;
    public Slider handling;
    public Slider defNos;
    public Slider nos;

    private void Update()
    {

        //  Getting HR_ModApplier script of the player car.
        if (HPS_MainMenuHandler.Instance.currentApplier)
            currentApplier = HPS_MainMenuHandler.Instance.currentApplier;
        else
            currentApplier = null;

        // If no any player car, return.
        if (!currentApplier)
            return;

        // Setting interactable states of the buttons depending on upgrade managers. 
        //	Ex. If spoiler manager not found, spoiler button will be disabled.
        upgradeButton.interactable = currentApplier.UpgradeManager;
        if (!upgradeButton.interactable)
            upgradeButton.image.color = Color.white;
        bodyPaintButton.interactable = currentApplier.PaintManager;
        if (!bodyPaintButton.interactable)
            bodyPaintButton.image.color = Color.white;
        platesButton.interactable = currentApplier.PlateManager;
        if (!platesButton.interactable)
            platesButton.image.color = Color.white;
        tireButton.interactable = currentApplier.PlateManager;
        if (!tireButton.interactable)
            tireButton.image.color = Color.white;
        rimButton.interactable = currentApplier.PlateManager;
        if (!rimButton.interactable)
            rimButton.image.color = Color.white;
        spoilersButton.interactable = currentApplier.SpoilerManager;
        if (!spoilersButton.interactable)
            spoilersButton.image.color = Color.white;

        // Feeding upgrade level texts for enigne, brake, handling.
        if (currentApplier.UpgradeManager)
        {

            if (speedUpgradeLevel)
            {
                for (int i = 0; i < 5; i++)
                {
                    if (i < currentApplier.UpgradeManager.speedLevel)
                        speedUpgradeLevel.transform.GetChild(i).GetChild(0).GetComponent<Image>().color = Color.white;
                    else
                        speedUpgradeLevel.transform.GetChild(i).GetChild(0).GetComponent<Image>().color = new Color(1, 1, 1, 0);
                }
            }
            if (engineUpgradeLevel)
            {
                for (int i = 0; i < 5; i++)
                {
                    if (i < currentApplier.UpgradeManager.engineLevel)
                        engineUpgradeLevel.transform.GetChild(i).GetChild(0).GetComponent<Image>().color = Color.white;
                    else
                        engineUpgradeLevel.transform.GetChild(i).GetChild(0).GetComponent<Image>().color = new Color(1, 1, 1, 0);
                }
            }
            if (handlingUpgradeLevel)
            {
                for (int i = 0; i < 5; i++)
                {
                    if (i < currentApplier.UpgradeManager.handlingLevel)
                        handlingUpgradeLevel.transform.GetChild(i).GetChild(0).GetComponent<Image>().color = Color.white;
                    else
                        handlingUpgradeLevel.transform.GetChild(i).GetChild(0).GetComponent<Image>().color = new Color(1, 1, 1, 0);
                }
            }
            if (nosUpgradeLevel)
            {
                for (int i = 0; i < 5; i++)
                {
                    if (i < currentApplier.UpgradeManager.nosLevel)
                        nosUpgradeLevel.transform.GetChild(i).GetChild(0).GetComponent<Image>().color = Color.white;
                    else
                        nosUpgradeLevel.transform.GetChild(i).GetChild(0).GetComponent<Image>().color = new Color(1, 1, 1, 0);
                }
            }

        }

        //  Displaying stats of the current car if found.
        if (currentApplier)
        {
            defSpeed.value = Mathf.Lerp(.1f, 1f, currentApplier.UpgradeManager.carDatas.defaultSpeed / 400f);
            speed.value = Mathf.Lerp(.1f, 1f, currentApplier.CarController.maxspeed / 400f);
            speedUpgradeValue.GetComponent<TextMeshProUGUI>().text = $"{currentApplier.UpgradeManager.carDatas.defaultSpeed}" + (currentApplier.CarController.maxspeed - currentApplier.UpgradeManager.carDatas.defaultSpeed == 0 ? "" : $" +{currentApplier.CarController.maxspeed - currentApplier.UpgradeManager.carDatas.defaultSpeed}");
            defEngine.value = Mathf.Lerp(.1f, 1f, (currentApplier.UpgradeManager.carDatas.defaultEngineTorque) / 1000f);
            engine.value = Mathf.Lerp(.1f, 1f, (currentApplier.CarController.maxEngineTorque) / 1000f);
            engineUpgradeValue.GetComponent<TextMeshProUGUI>().text = $"{currentApplier.UpgradeManager.carDatas.defaultEngineTorque}" + (currentApplier.CarController.maxEngineTorque - currentApplier.UpgradeManager.carDatas.defaultEngineTorque == 0 ? "" : $" +{currentApplier.CarController.maxEngineTorque - currentApplier.UpgradeManager.carDatas.defaultEngineTorque}");
            defHandling.value = Mathf.Lerp(.1f, 1f, currentApplier.UpgradeManager.carDatas.defaultHandlingStrength / .5f);
            handling.value = Mathf.Lerp(.1f, 1f, currentApplier.CarController.steerHelperAngularVelStrength / .5f);
            handlingUpgradeValue.GetComponent<TextMeshProUGUI>().text = $"{currentApplier.UpgradeManager.carDatas.defaultHandlingStrength}" + (currentApplier.CarController.steerHelperAngularVelStrength - currentApplier.UpgradeManager.carDatas.defaultHandlingStrength == 0 ? "" : $" +{currentApplier.CarController.steerHelperAngularVelStrength - currentApplier.UpgradeManager.carDatas.defaultHandlingStrength}");
            defNos.value = Mathf.Lerp(.1f, 1f, currentApplier.UpgradeManager.carDatas.defaultNOS / 10f);
            nos.value = Mathf.Lerp(.1f, 1f, currentApplier.CarController.NitroTime / 10f);
            nosUpgradeValue.GetComponent<TextMeshProUGUI>().text = $"{currentApplier.UpgradeManager.carDatas.defaultNOS}" + (currentApplier.CarController.NitroTime - currentApplier.UpgradeManager.carDatas.defaultNOS == 0 ? "" : $" +{currentApplier.CarController.NitroTime - currentApplier.UpgradeManager.carDatas.defaultNOS}");
        }
        else
        {
            defSpeed.value = 0;
            speed.value = 0;
            defEngine.value = 0;
            engine.value = 0;
            defHandling.value = 0;
            handling.value = 0;
            defNos.value = 0;
            nos.value = 0;
        }

    }

    public bool AnyClassOpen() => upgradesClass.activeSelf || colorClass.activeSelf || plateClass.activeSelf || tireClass.activeSelf || rimClass.activeSelf || spoilerClass.activeSelf;

    public GameObject GetOpenClass()
    {
        if (upgradesClass.activeSelf) return upgradesClass;
        if (colorClass.activeSelf) return colorClass;
        if (plateClass.activeSelf) return plateClass;
        if (tireClass.activeSelf) return tireClass; 
        if (rimClass.activeSelf) return rimClass; 
        if (spoilerClass.activeSelf) return spoilerClass;
        return null;
    }

    /// <summary>
    /// Hide all class panel.
    /// </summary>
    public void HideClass()
    {
        mainsClass.SetActive(false);
        upgradesClass.SetActive(false);
        colorClass.SetActive(false);
        plateClass.SetActive(false);
        tireClass.SetActive(false);
        rimClass.SetActive(false);
        spoilerClass.SetActive(false);
        CheckButtonColors(null);

    }

    /// <summary>
    /// Opens up the taget class panel.
    /// </summary>
    /// <param name="activeClass"></param>
    public void ChooseClass(GameObject activeClass)
    {
        //mainsClass.SetActive(false);
        upgradesClass.SetActive(false);
        colorClass.SetActive(false);
        plateClass.SetActive(false);
        tireClass.SetActive(false);
        rimClass.SetActive(false);
        spoilerClass.SetActive(false);

        if (activeClass)
            activeClass.SetActive(true);

        CheckButtonColors(null);

    }

    /// <summary>
    /// Checks colors of the UI buttons. Ex. If paint class is enabled, color of the button will be green. 
    /// </summary>
    /// <param name="activeButton"></param>
    public void CheckButtonColors(Button activeButton)
    {

        upgradeButton.image.color = orgButtonColor;
        upgradeButton.enabled = true;
        bodyPaintButton.image.color = orgButtonColor;
        bodyPaintButton.enabled = true;
        platesButton.image.color = orgButtonColor;
        platesButton.enabled = true;
        tireButton.image.color = orgButtonColor;
        tireButton.enabled = true;
        rimButton.image.color = orgButtonColor;
        rimButton.enabled = true;
        spoilersButton.image.color = orgButtonColor;
        spoilersButton.enabled = true;

        if (activeButton)
        {
            //activeButton.image.color = new Color(0f, 1f, 0f);
            activeButton.image.color = new Color(0.7843137f, 0.7843137f, 0.7843137f, 0.5019608f);
            activeButton.enabled = false;
        }

    }

    /// <summary>
    /// Check purchase of all Modifiers
    /// </summary>
    public void CheckPurchaseOfModifiers()
    {
        if (!AnyClassOpen())
            return;

        GameObject openClass = GetOpenClass();

        for (int i = 0; i < openClass.GetComponentInChildren<ScrollRect>().content.childCount; i++)
            openClass.GetComponentInChildren<ScrollRect>().content.GetChild(i).SendMessage("CheckPurchase");

    }

    /// <summary>
    /// Sets auto rotation of the showrooom camera.
    /// </summary>
    /// <param name="state"></param>
    public void ToggleAutoRotation(bool state)
    {

        Camera.main.gameObject.GetComponent<HR_ShowroomCamera>().ToggleAutoRotation(state);

    }

    /// <summary>
    /// Sets horizontal angle of the showroom camera.
    /// </summary>
    /// <param name="hor"></param>
    public void SetHorizontal(float hor)
    {

        Camera.main.gameObject.GetComponent<HR_ShowroomCamera>().orbitX = hor;

    }

    /// <summary>
    /// Sets vertical angle of the showroom camera.
    /// </summary>
    /// <param name="ver"></param>
    public void SetVertical(float ver)
    {

        Camera.main.gameObject.GetComponent<HR_ShowroomCamera>().orbitY = ver;

    }

}
