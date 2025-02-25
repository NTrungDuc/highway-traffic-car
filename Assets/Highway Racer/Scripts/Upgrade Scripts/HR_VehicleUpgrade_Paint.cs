//----------------------------------------------
//           	   Highway Racer
//
// Copyright © 2014 - 2023 BoneCracker Games
// http://www.bonecrackergames.com
//
//----------------------------------------------

using PaintCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Upgradable paint.
/// </summary>
[AddComponentMenu("BoneCracker Games/Highway Racer/Upgrade/HR Paint")]
public class HR_VehicleUpgrade_Paint : MonoBehaviour {

    public MeshRenderer bodyRenderer;       //  Target renderer for painting.
    public int index = 0;       //  Index of the target material.
    private Color color = Color.gray;        //  Default color.

    private void OnEnable() {

        //  Getting last saved color for this vehicle.
        color = RCC_PlayerPrefsX.GetColor(transform.root.name + "BodyColor", HR_HighwayRacerProperties.Instance._defaultBodyColor);

        //  Paint.
        if (bodyRenderer)
        {
            bodyRenderer.materials[index].color = color;
            //bodyRenderer.materials[index].SetColor("_EmissionColor", color * 0.75f);
        }

    }

    /// <summary>
    /// Paint the material with target color and save it.
    /// </summary>
    /// <param name="newColor"></param>
    /// <param name="saved"></param>
    public void UpdatePaint(Color newColor, bool saved = true) {

        if (bodyRenderer)
        {
            bodyRenderer.materials[index].color = newColor;
            //bodyRenderer.materials[index].SetColor("_EmissionColor", newColor * 0.75f);
        }

        if (saved)
        {
            RCC_PlayerPrefsX.SetColor(transform.root.name + "BodyColor", newColor);
            PlayerPrefs.SetInt(transform.root.name + "BodyColor" + newColor.ToString(), 1);
        }

    }

}
