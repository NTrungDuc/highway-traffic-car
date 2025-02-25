//----------------------------------------------
//           	   Highway Racer
//
// Copyright © 2014 - 2023 BoneCracker Games
// http://www.bonecrackergames.com
//
//----------------------------------------------

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if PHOTON_UNITY_NETWORKING
using Photon;
using Photon.Pun;
using Photon.Realtime;
#endif

#if PHOTON_UNITY_NETWORKING && BCG_HR_PHOTON

/// <summary>
/// Modification applier for vehicles. Needs to be attached to the vehicle.
/// 7 Upgrade managers for paint, wheel, upgrade, neon, decal, spoiler, and siren.
/// </summary>
[AddComponentMenu("BoneCracker Games/Highway Racer/Player/HR Mod Applier")]
public class HR_ModApplier : MonoBehaviourPunCallbacks, IPunObservable {

    #region All upgrade managers

    private HR_VehicleUpgrade_UpgradeManager upgradeManager;
    public HR_VehicleUpgrade_UpgradeManager UpgradeManager {

        get {

            if (upgradeManager == null)
                upgradeManager = GetComponentInChildren<HR_VehicleUpgrade_UpgradeManager>();

            return upgradeManager;

        }

    }

    private HR_VehicleUpgrade_PaintManager paintManager;
    public HR_VehicleUpgrade_PaintManager PaintManager {

        get {

            if (paintManager == null)
                paintManager = GetComponentInChildren<HR_VehicleUpgrade_PaintManager>();

            return paintManager;

        }

    }

    private HR_VehicleUpgrade_WheelManager wheelManager;
    public HR_VehicleUpgrade_WheelManager WheelManager {

        get {

            if (wheelManager == null)
                wheelManager = GetComponentInChildren<HR_VehicleUpgrade_WheelManager>();

            return wheelManager;

        }

    }

    private HR_VehicleUpgrade_SpoilerManager spoilerManager;
    public HR_VehicleUpgrade_SpoilerManager SpoilerManager {

        get {

            if (spoilerManager == null)
                spoilerManager = GetComponentInChildren<HR_VehicleUpgrade_SpoilerManager>();

            return spoilerManager;

        }

    }

    private HR_VehicleUpgrade_SirenManager sirenManager;
    public HR_VehicleUpgrade_SirenManager SirenManager {

        get {

            if (sirenManager == null)
                sirenManager = GetComponentInChildren<HR_VehicleUpgrade_SirenManager>();

            return sirenManager;

        }

    }

    #endregion

    private RCC_CarControllerV3 carController;
    public RCC_CarControllerV3 CarController {

        get {

            if (!carController)
                carController = GetComponent<RCC_CarControllerV3>();

            return carController;

        }

    }

    //  Local and networked indexes.
    private int spoilerIndex = -1;
    private int N_spoilerIndex = -1;
    private int neonIndex = -1;
    private int N_neonIndex = -1;
    private int sirenIndex = -1;
    private int N_sirenIndex = -1;
    private Color paint = new Color(1f, 1f, 1f, 1f);
    private Color N_paint = new Color(1f, 1f, 1f, 0f);
    private int wheelIndex = -1;
    private int N_wheelIndex = -1;

    private int decal_FIndex = -1;
    private int decal_BIndex = -1;
    private int decal_LIndex = -1;
    private int decal_RIndex = -1;

    private int N_decal_FIndex = -1;
    private int N_decal_BIndex = -1;
    private int N_decal_LIndex = -1;
    private int N_decal_RIndex = -1;

    private bool initialized = false;

    private void Awake() {

        //  Make sure all visual parts are disabled at start.

        if (SpoilerManager)
            SpoilerManager.DisableAll();

        if (SirenManager)
            SirenManager.DisableAll();

    }

    public override void OnEnable() {

        //  Initialize all upgrade managers if we own this vehicle and stream corresponding indexes.
        //  If we are not owner of this vehicle, receive all indexes and enable corresponding indexes.

        bool networkVehicle = false;
        bool networkVehicleMine = false;

        if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.InRoom)
            networkVehicle = true;

        if (networkVehicle && photonView.IsMine)
            networkVehicleMine = true;

        if (networkVehicle && !networkVehicleMine)
            return;

        if (SpoilerManager)
            SpoilerManager.Initialize();

        if (SirenManager)
            SirenManager.Initialize();

        if (WheelManager)
            WheelManager.Initialize();

        if (PaintManager)
            PaintManager.Initialize();

        if (UpgradeManager)
            UpgradeManager.Initialize();

        if (SpoilerManager) {

            foreach (Transform item in SpoilerManager.transform) {

                if (item != SpoilerManager.transform && item.gameObject.activeSelf)
                    spoilerIndex = item.GetSiblingIndex();

            }

        }

        if (SirenManager) {

            foreach (Transform item in SirenManager.transform) {

                if (item != SirenManager.transform && item.gameObject.activeSelf)
                    sirenIndex = item.GetSiblingIndex();

            }

        }

        if (PaintManager) {

            HR_VehicleUpgrade_Paint[] paints = PaintManager.transform.GetComponentsInChildren<HR_VehicleUpgrade_Paint>();

            foreach (HR_VehicleUpgrade_Paint item in paints) {

                if (item.bodyRenderer)
                    paint = item.bodyRenderer.materials[item.index].color;

            }

        }

        if (WheelManager)
            wheelIndex = PlayerPrefs.GetInt(transform.root.name + "SelectedWheel", -1);

    }

    private void FixedUpdate() {

        if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.InRoom && photonView && !photonView.IsMine) {

            if (initialized)
                return;

            if (SpoilerManager && N_spoilerIndex != -1)
                SpoilerManager.transform.GetChild(N_spoilerIndex).gameObject.SetActive(true);

            if (SirenManager && N_sirenIndex != -1)
                SirenManager.transform.GetChild(N_sirenIndex).gameObject.SetActive(true);

            if (PaintManager) {

                HR_VehicleUpgrade_Paint[] paints = PaintManager.transform.GetComponentsInChildren<HR_VehicleUpgrade_Paint>();

                foreach (HR_VehicleUpgrade_Paint item in paints) {

                    if (item.bodyRenderer && N_paint != new Color(1f, 1f, 1f, 0f))
                        item.bodyRenderer.materials[item.index].color = N_paint;

                }

            }

            if (WheelManager) {

                if (N_wheelIndex != -1)
                    RCC_Customization.ChangeWheels(GetComponent<RCC_CarControllerV3>(), HR_Wheels.Instance.wheels[N_wheelIndex].wheel, true);

            }

            initialized = true;

        }

    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {

        if (stream.IsWriting) {

            stream.SendNext(spoilerIndex);
            stream.SendNext(neonIndex);
            stream.SendNext(sirenIndex);
            stream.SendNext(paint.r);
            stream.SendNext(paint.g);
            stream.SendNext(paint.b);
            stream.SendNext(wheelIndex);
            stream.SendNext(decal_FIndex);
            stream.SendNext(decal_BIndex);
            stream.SendNext(decal_LIndex);
            stream.SendNext(decal_RIndex);

        } else if (stream.IsReading) {

            N_spoilerIndex = (int)stream.ReceiveNext();
            N_neonIndex = (int)stream.ReceiveNext();
            N_sirenIndex = (int)stream.ReceiveNext();
            N_paint.r = (float)stream.ReceiveNext();
            N_paint.g = (float)stream.ReceiveNext();
            N_paint.b = (float)stream.ReceiveNext();
            N_wheelIndex = (int)stream.ReceiveNext();
            N_decal_FIndex = (int)stream.ReceiveNext();
            N_decal_BIndex = (int)stream.ReceiveNext();
            N_decal_LIndex = (int)stream.ReceiveNext();
            N_decal_RIndex = (int)stream.ReceiveNext();

        }

    }

}

#else

/// <summary>
/// Modification applier for vehicles. Needs to be attached to the vehicle.
/// 7 Upgrade managers for paint, wheel, upgrade, neon, decal, spoiler, and siren.
/// </summary>
[AddComponentMenu("BoneCracker Games/Highway Racer/Player/HR Mod Applier")]
public class HR_ModApplier : MonoBehaviour {

#region All upgrade managers

    private HR_VehicleUpgrade_UpgradeManager upgradeManager;
    public HR_VehicleUpgrade_UpgradeManager UpgradeManager {

        get {

            if (upgradeManager == null)
                upgradeManager = GetComponentInChildren<HR_VehicleUpgrade_UpgradeManager>();

            return upgradeManager;

        }

    }

    private HR_VehicleUpgrade_PaintManager paintManager;
    public HR_VehicleUpgrade_PaintManager PaintManager {

        get {

            if (paintManager == null)
                paintManager = GetComponentInChildren<HR_VehicleUpgrade_PaintManager>();

            return paintManager;

        }

    }

    private HR_VehicleUpgrade_PlateManager plateManager;
    public HR_VehicleUpgrade_PlateManager PlateManager
    {
        get
        {
            if (plateManager == null)
                plateManager = GetComponentInChildren<HR_VehicleUpgrade_PlateManager>();

            return plateManager;
        }
    }

    private HR_VehicleUpgrade_RimManager rimManager;
    public HR_VehicleUpgrade_RimManager RimManager
    {
        get
        {
            if (rimManager == null)
                rimManager = GetComponentInChildren<HR_VehicleUpgrade_RimManager>();

            return rimManager;
        }
    }

    private HR_VehicleUpgrade_TireManager tireManager;
    public HR_VehicleUpgrade_TireManager TireManager
    {
        get
        {
            if (tireManager == null)
                tireManager = GetComponentInChildren<HR_VehicleUpgrade_TireManager>();

            return tireManager;
        }
    }

    private HR_VehicleUpgrade_WheelManager wheelManager;
    public HR_VehicleUpgrade_WheelManager WheelManager {

        get {

            if (wheelManager == null)
                wheelManager = GetComponentInChildren<HR_VehicleUpgrade_WheelManager>();

            return wheelManager;

        }

    }

    private HR_VehicleUpgrade_SpoilerManager spoilerManager;
    public HR_VehicleUpgrade_SpoilerManager SpoilerManager {

        get {

            if (spoilerManager == null)
                spoilerManager = GetComponentInChildren<HR_VehicleUpgrade_SpoilerManager>();

            return spoilerManager;

        }

    }

    private HR_VehicleUpgrade_SirenManager sirenManager;
    public HR_VehicleUpgrade_SirenManager SirenManager {

        get {

            if (sirenManager == null)
                sirenManager = GetComponentInChildren<HR_VehicleUpgrade_SirenManager>();

            return sirenManager;

        }

    }

#endregion

    private RCC_CarControllerV3 carController;
    public RCC_CarControllerV3 CarController {

        get {

            if (!carController)
                carController = GetComponent<RCC_CarControllerV3>();

            return carController;

        }

    }

    private void Awake() {

        if (SpoilerManager)
            SpoilerManager.DisableAll();

        if (SirenManager)
            SirenManager.DisableAll();

        if (PlateManager)
            PlateManager.DisableAll();



    }

    private void OnEnable() {

        if (SpoilerManager)
            SpoilerManager.Initialize();

        if (SirenManager)
            SirenManager.Initialize();

        if (RimManager)
            RimManager.Initialize();

        if (TireManager)
            TireManager.Initialize();

        if (WheelManager)
            WheelManager.Initialize();

        if (PaintManager)
            PaintManager.Initialize();

        if (PlateManager)
            PlateManager.Initialize();

        if (UpgradeManager)
            UpgradeManager.Initialize();

    }

}

#endif