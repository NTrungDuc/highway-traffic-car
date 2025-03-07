﻿//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2014 - 2023 BoneCracker Games
// https://www.bonecrackergames.com
// Buğra Özdoğanlar
//
//----------------------------------------------

using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

public class RCC_EditorWindows : Editor {

    private static RCC_CarControllerV3 SelectedCar() {

        if (Selection.activeGameObject == null)
            return null;

        return Selection.activeGameObject.GetComponentInParent<RCC_CarControllerV3>();

    }

    #region Edit Settings
#if RCC_SHORTCUTS
    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Edit RCC Settings #s", false, -100)]
    public static void OpenRCCSettings() {
        Selection.activeObject = RCC_Settings.Instance;
    }
#else
    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Edit RCC Settings", false, -100)]
    public static void OpenRCCSettings() {
        Selection.activeObject = RCC_Settings.Instance;
    }
#endif
    #endregion

    #region Configure

    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Configure Ground Materials", false, -65)]
    public static void OpenGroundMaterialsSettings() {
        Selection.activeObject = RCC_GroundMaterials.Instance;
    }

    #endregion

    #region Add Cameras

    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Create/Cameras/Add Hood Camera To Vehicle", false, -50)]
    public static void CreateHoodCamera() {

        if (SelectedCar() == null) {

            EditorUtility.DisplayDialog("Select a vehicle controlled by Realistic Car Controller!", "Select a vehicle controlled by Realistic Car Controller!", "Close");

        } else {

            if (SelectedCar().gameObject.GetComponentInChildren<RCC_HoodCamera>()) {

                EditorUtility.DisplayDialog("Your Vehicle Has Hood Camera Already!", "Your vehicle has hood camera already!", "Close");
                Selection.activeGameObject = SelectedCar().gameObject.GetComponentInChildren<RCC_HoodCamera>().gameObject;
                return;

            }

            GameObject hoodCam = (GameObject)Instantiate(RCC_Settings.Instance.hoodCamera, SelectedCar().transform.position, SelectedCar().transform.rotation);
            hoodCam.name = RCC_Settings.Instance.hoodCamera.name;
            hoodCam.transform.SetParent(SelectedCar().transform, true);
            hoodCam.GetComponent<ConfigurableJoint>().connectedBody = SelectedCar().gameObject.GetComponent<Rigidbody>();
            hoodCam.GetComponent<ConfigurableJoint>().connectedMassScale = 0f;
            Selection.activeGameObject = hoodCam;

        }

    }

    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Create/Cameras/Add Hood Camera To Vehicle", true)]
    public static bool CheckCreateHoodCamera() {

        if (!Selection.activeGameObject)
            return false;

        if (Selection.gameObjects.Length > 1)
            return false;

        if (!Selection.activeTransform.gameObject.activeSelf)
            return false;

        return true;

    }

    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Create/Cameras/Add Wheel Camera To Vehicle", false, -50)]
    public static void CreateWheelCamera() {

        if (SelectedCar() == null) {

            EditorUtility.DisplayDialog("Select a vehicle controlled by Realistic Car Controller!", "Select a vehicle controlled by Realistic Car Controller!", "Close");

        } else {

            if (SelectedCar().gameObject.GetComponentInChildren<RCC_WheelCamera>()) {
                EditorUtility.DisplayDialog("Your Vehicle Has Wheel Camera Already!", "Your vehicle has wheel camera already!", "Close");
                Selection.activeGameObject = SelectedCar().gameObject.GetComponentInChildren<RCC_WheelCamera>().gameObject;
                return;
            }

            GameObject wheelCam = new GameObject("WheelCamera");
            wheelCam.transform.SetParent(SelectedCar().transform, false);
            wheelCam.AddComponent<RCC_WheelCamera>();
            Selection.activeGameObject = wheelCam;

        }

    }

    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Create/Cameras/Add Wheel Camera To Vehicle", true)]
    public static bool CheckCreateWheelCamera() {

        if (!Selection.activeGameObject)
            return false;

        if (Selection.gameObjects.Length > 1)
            return false;

        if (!Selection.activeTransform.gameObject.activeSelf)
            return false;

        return true;

    }
    #endregion

    #region Add Lights
    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Create/Lights/Add Lights To Vehicle/HeadLight", false, -50)]
    public static void CreateHeadLight() {

        if (SelectedCar() == null) {

            EditorUtility.DisplayDialog("Select a vehicle controlled by Realistic Car Controller!", "Select a vehicle controlled by Realistic Car Controller!", "Close");

        } else {

            GameObject lightsMain;

            if (!SelectedCar().transform.Find("Lights")) {

                lightsMain = new GameObject("Lights");
                lightsMain.transform.SetParent(SelectedCar().transform, false);

            } else {

                lightsMain = SelectedCar().transform.Find("Lights").gameObject;

            }

            GameObject headLight = Instantiate(RCC_Settings.Instance.headLights, lightsMain.transform.position, lightsMain.transform.rotation) as GameObject;
            headLight.name = RCC_Settings.Instance.headLights.name;
            headLight.transform.SetParent(lightsMain.transform);
            headLight.transform.localRotation = Quaternion.identity;
            headLight.transform.localPosition = new Vector3(0f, 0f, 2f);
            Selection.activeGameObject = headLight;

        }

    }

    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Create/Lights/Add Lights To Vehicle/HeadLight", true)]
    public static bool CheckHeadLight() {

        if (!Selection.activeGameObject)
            return false;

        if (Selection.gameObjects.Length > 1)
            return false;

        if (!Selection.activeTransform.gameObject.activeSelf)
            return false;

        return true;

    }

    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Create/Lights/Add Lights To Vehicle/Brake", false, -50)]
    public static void CreateBrakeLight() {

        if (SelectedCar() == null) {

            EditorUtility.DisplayDialog("Select a vehicle controlled by Realistic Car Controller!", "Select a vehicle controlled by Realistic Car Controller!", "Close");

        } else {

            GameObject lightsMain;

            if (!SelectedCar().transform.Find("Lights")) {

                lightsMain = new GameObject("Lights");
                lightsMain.transform.SetParent(SelectedCar().transform, false);

            } else {

                lightsMain = SelectedCar().transform.Find("Lights").gameObject;

            }

            GameObject brakeLight = Instantiate(RCC_Settings.Instance.brakeLights, lightsMain.transform.position, lightsMain.transform.rotation) as GameObject;
            brakeLight.name = RCC_Settings.Instance.brakeLights.name;
            brakeLight.transform.SetParent(lightsMain.transform);
            brakeLight.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            brakeLight.transform.localPosition = new Vector3(0f, 0f, -2f);
            Selection.activeGameObject = brakeLight;

        }

    }

    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Create/Lights/Add Lights To Vehicle/Brake", true)]
    public static bool CheckBrakeLight() {

        if (!Selection.activeGameObject)
            return false;

        if (Selection.gameObjects.Length > 1)
            return false;

        if (!Selection.activeTransform.gameObject.activeSelf)
            return false;

        return true;

    }

    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Create/Lights/Add Lights To Vehicle/Reverse", false, -50)]
    public static void CreateReverseLight() {

        if (SelectedCar() == null) {

            EditorUtility.DisplayDialog("Select a vehicle controlled by Realistic Car Controller!", "Select a vehicle controlled by Realistic Car Controller!", "Close");

        } else {

            GameObject lightsMain;

            if (!SelectedCar().transform.Find("Lights")) {

                lightsMain = new GameObject("Lights");
                lightsMain.transform.SetParent(SelectedCar().transform, false);

            } else {

                lightsMain = SelectedCar().transform.Find("Lights").gameObject;

            }

            GameObject reverseLight = Instantiate(RCC_Settings.Instance.reverseLights, lightsMain.transform.position, lightsMain.transform.rotation) as GameObject;
            reverseLight.name = RCC_Settings.Instance.reverseLights.name;
            reverseLight.transform.SetParent(lightsMain.transform);
            reverseLight.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            reverseLight.transform.localPosition = new Vector3(0f, 0f, -2f);
            Selection.activeGameObject = reverseLight;

        }

    }

    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Create/Lights/Add Lights To Vehicle/Reverse", true)]
    public static bool CheckReverseLight() {

        if (!Selection.activeGameObject)
            return false;

        if (Selection.gameObjects.Length > 1)
            return false;

        if (!Selection.activeTransform.gameObject.activeSelf)
            return false;

        return true;

    }

    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Create/Lights/Add Lights To Vehicle/Indicator", false, -50)]
    public static void CreateIndicatorLight() {

        if (SelectedCar() == null) {

            EditorUtility.DisplayDialog("Select a vehicle controlled by Realistic Car Controller!", "Select a vehicle controlled by Realistic Car Controller!", "Close");

        } else {

            GameObject lightsMain;

            if (!SelectedCar().transform.Find("Lights")) {

                lightsMain = new GameObject("Lights");
                lightsMain.transform.SetParent(SelectedCar().transform, false);

            } else {

                lightsMain = SelectedCar().transform.Find("Lights").gameObject;

            }

            GameObject indicatorLight = Instantiate(RCC_Settings.Instance.indicatorLights, lightsMain.transform.position, lightsMain.transform.rotation) as GameObject;
            Vector3 relativePos = SelectedCar().transform.InverseTransformPoint(indicatorLight.transform.position);
            indicatorLight.name = RCC_Settings.Instance.indicatorLights.name;
            indicatorLight.transform.SetParent(lightsMain.transform);

            if (relativePos.z > 0f)
                indicatorLight.transform.localRotation = Quaternion.identity;
            else
                indicatorLight.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

            indicatorLight.transform.localPosition = new Vector3(0f, 0f, -2f);
            Selection.activeGameObject = indicatorLight;

        }

    }

    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Create/Lights/Add Lights To Vehicle/Indicator", true)]
    public static bool CheckIndicatorLight() {

        if (!Selection.activeGameObject)
            return false;

        if (Selection.gameObjects.Length > 1)
            return false;

        if (!Selection.activeTransform.gameObject.activeSelf)
            return false;

        return true;

    }
    #endregion

    #region Add Exhausts
    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Create/Misc/Add Exhaust To Vehicle", false, -50)]
    public static void CreateExhaust() {

        if (SelectedCar() == null) {

            EditorUtility.DisplayDialog("Select a vehicle controlled by Realistic Car Controller!", "Select a vehicle controlled by Realistic Car Controller!", "Close");

        } else {

            GameObject exhaustsMain;

            if (!SelectedCar().transform.Find("Exhausts")) {
                exhaustsMain = new GameObject("Exhausts");
                exhaustsMain.transform.SetParent(SelectedCar().transform, false);
            } else {
                exhaustsMain = SelectedCar().transform.Find("Exhausts").gameObject;
            }

            GameObject exhaust = (GameObject)Instantiate(RCC_Settings.Instance.exhaustGas, SelectedCar().transform.position, SelectedCar().transform.rotation * Quaternion.Euler(0f, 180f, 0f));
            exhaust.name = RCC_Settings.Instance.exhaustGas.name;
            exhaust.transform.SetParent(exhaustsMain.transform);
            exhaust.transform.localPosition = new Vector3(1f, 0f, -2f);
            Selection.activeGameObject = exhaust;

        }

    }

    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Create/Misc/Add Exhaust To Vehicle", true)]
    public static bool CheckCreateExhaust() {

        if (!Selection.activeGameObject)
            return false;

        if (Selection.gameObjects.Length > 1)
            return false;

        if (!Selection.activeTransform.gameObject.activeSelf)
            return false;

        return true;

    }
    #endregion

    #region Add Mirrors
    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Create/Misc/Add Mirrors To Vehicle", false, -50)]
    public static void CreateBehavior() {

        if (SelectedCar() == null)
            EditorUtility.DisplayDialog("Select a vehicle controlled by Realistic Car Controller!", "Select a vehicle controlled by Realistic Car Controller!", "Close");
        else
            CreateMirrors(SelectedCar().gameObject);

    }

    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Create/Misc/Add Mirrors To Vehicle", true)]
    public static bool CheckCreateBehavior() {

        if (!Selection.activeGameObject)
            return false;

        if (Selection.gameObjects.Length > 1)
            return false;

        if (!Selection.activeTransform.gameObject.activeSelf)
            return false;

        return true;

    }
    #endregion

    #region Help
    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Help", false, 10000)]
    public static void Help() {

        EditorUtility.DisplayDialog("Contact", "Please include your invoice number while sending a contact form.", "Close");

        string url = "http://www.bonecrackergames.com/contact/";
        Application.OpenURL(url);

    }

    #endregion Help

    #region Static Methods
    public static void CreateMirrors(GameObject vehicle) {

        if (!vehicle.transform.GetComponentInChildren<RCC_Mirror>()) {

            GameObject mirrors = (GameObject)Instantiate(RCC_Settings.Instance.mirrors, vehicle.transform.position, vehicle.transform.rotation);
            mirrors.transform.SetParent(vehicle.GetComponent<RCC_CarControllerV3>().transform, true);
            mirrors.name = "Mirrors";
            Selection.activeGameObject = mirrors;
            EditorUtility.DisplayDialog("Created Mirrors!", "Created mirrors. Adjust their positions.", "Close");

        } else {

            EditorUtility.DisplayDialog("Vehicle Has Mirrors Already", "Vehicle has mirrors already!", "Close");

        }

    }
    #endregion

}
