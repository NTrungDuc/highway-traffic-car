//----------------------------------------------
//           	   Highway Racer
//
// Copyright © 2014 - 2023 BoneCracker Games
// http://www.bonecrackergames.com
//
//----------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Showroom camera used on main menu.
/// </summary>
[AddComponentMenu("BoneCracker Games/Highway Racer/Camera/HR Showroom Camera")]
public class HR_ShowroomCamera : MonoBehaviour {

    public Transform target;        //  Camera target. Usually our spawn point.
    public float distance = 8f;     //  Z Distance.
    [Space]
    public bool orbitingNow = true;     //  Auto orbiting now?
    public float orbitSpeed = 5f;       //  Auto orbiting speed.
    [Space]
    public float minX = 0f;     //  Minimum X degree.
    public float maxX = 0f;         //  Maximum X degree.
    [Space]
    public float minY = 5f;     //  Minimum Y degree.
    public float maxY = 35f;        //  Maximum Y degree.
    [Space]
    public float dragSpeed = 10f;       //  Drag speed.
    public float orbitX = 0f;       //  Orbit X.
    public float orbitY = 0f;       //  Orbit Y.
    [Space]
    public Vector3 offset;

    public enum Focus { Default, Rim, Tire, Plate, Spoiler }

    private float side = 1;
    private Focus focusMode;
    private float defaultFOV;
    private Vector3 defaultTargetPos;
    private HPS_UIModInfo modInfo;

    void Start()
    {
        defaultFOV = Camera.main.fieldOfView;    
        defaultTargetPos = target.position;
    }

    private void LateUpdate() {

        // If there is no target, return.
        if (!target)
            return;

        //if (focusMode != Focus.Default) {
        if (focusMode == Focus.Spoiler) {

            if (orbitX < minX) {

                side = 1;
                orbitX = minX;

            }

            else if (orbitX >= minX && orbitX <= maxX)

                orbitX += side * Time.deltaTime * orbitSpeed;

            else if (orbitX > maxX) {

                side = -1;
                orbitX = maxX;

            }
        }

        // If auto orbiting is enabled, increase orbitX slowly with orbitSpeed factor.
        if (orbitingNow && focusMode == Focus.Default)
            orbitX += side * Time.deltaTime * orbitSpeed;

        //  Clamping orbit Y.
        orbitY = ClampAngle(orbitY, minY, maxY);

        if (focusMode == Focus.Tire || focusMode == Focus.Rim || focusMode == Focus.Plate)
            orbitX = ClampAngle(orbitX, minX, maxX);

        // Calculating rotation and position of the camera.
        Quaternion rotation = Quaternion.Euler(orbitY, orbitX, 0);
        Vector3 position = rotation * new Vector3(0f, 0f, -distance) + target.transform.position + offset;

        // Setting position and rotation of the camera.
        transform.SetPositionAndRotation(position, rotation);

    }

    private float ClampAngle(float angle, float min, float max) {

        if (angle < -360)
            angle += 360;
        if (angle > 360)
            angle -= 360;

        return Mathf.Clamp(angle, min, max);

    }

    public void SetFocus(int mode) {

        if (!modInfo)
            modInfo = FindObjectOfType<HPS_UIModInfo>();

        Focus on = modInfo.focusMode = focusMode = (Focus)mode;

        offset = Vector3.zero;
        Camera.main.fieldOfView = defaultFOV;
        target.position = defaultTargetPos;

        switch (on) {

            case Focus.Rim:
            case Focus.Tire:
                orbitX = 30f;
                orbitY = 2f;
                minX = 30f;
                maxX = 90f;
                offset = new Vector2(0.5f, -0.5f);
                target.position = new Vector3(target.position.x, target.position.y, -1.25f);
                Camera.main.fieldOfView = 12.5f;
                break;
            case Focus.Plate:
                orbitX = -22.5f;
                orbitY = 5f;
                minX = -45f;
                maxX = 0f;
                offset = new Vector2(0f, -0.5f);
                target.position = new Vector3(0.5f, 0.625f, -1f);
                Camera.main.fieldOfView = 12.5f;
                break;
            case Focus.Spoiler:
                orbitY = 12.5f;
                minX = 300f;
                maxX = 360f;
                break;
            case Focus.Default:
                minX = 0f;
                maxX = 0f;
                break;
        }
    }

    public void ToggleAutoRotation(bool state) {

        orbitingNow = state;

    }

    public void OnDrag(PointerEventData pointerData) {

        // Receiving drag input from UI.
        orbitX += pointerData.delta.x * dragSpeed * .02f;
        //orbitY -= pointerData.delta.y * dragSpeed * .02f;

    }

}