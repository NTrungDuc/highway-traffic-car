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
/// Manager for Plates.
/// </summary>
[AddComponentMenu("BoneCracker Games/Highway Racer/Upgrade/HR Plate Manager")]
public class HR_VehicleUpgrade_PlateManager : MonoBehaviour {

    private Transform[] plate;       //  All plates.
    private int selectedIndex = -1;     //  Last selected plate index.

    public void Initialize() {

        //  Getting all plates
        plate = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
            plate[i] = transform.GetChild(i);

        CheckPlates();

    }

    /// <summary>
    /// Disabling all other plates, and enabling only selected plate.
    /// </summary>
    public void CheckPlates() {

        for (int i = 0; i < plate.Length; i++)
            plate[i].gameObject.SetActive(false);

        selectedIndex = PlayerPrefs.GetInt(transform.root.name + "Plate", -1);

        if (selectedIndex != -1)
            plate[selectedIndex].gameObject.SetActive(true);

    }

    /// <summary>
    /// Unlocks target plate index and saves it.
    /// </summary>
    /// <param name="index"></param>
    /// <param name="saved"></param>
    public void Upgrade(int index, bool saved = true) {

        selectedIndex = index;

        for (int i = 0; i < plate.Length; i++)
            plate[i].gameObject.SetActive(false);

        if (index != -1)
            plate[index].gameObject.SetActive(true);

        if (saved)
        {
            PlayerPrefs.SetInt(transform.root.name + "Plate" + selectedIndex, 1);
            PlayerPrefs.SetInt(transform.root.name + "Plate", selectedIndex);
        }

    }

    public void DisableAll() {

        //  Getting all upgradable plates.
        plate = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
            plate[i] = transform.GetChild(i);

        for (int i = 0; i < plate.Length; i++)
            plate[i].gameObject.SetActive(false);

    }

}
