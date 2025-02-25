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
using UnityEngine.EventSystems;
using UnityEngine.UI;

[AddComponentMenu("BoneCracker Games/Highway Racer/UI/HR UI Horn")]
public class HR_UIHorn : MonoBehaviour, IPointerDownHandler {

    [SerializeField] Image button;

    bool isTurnedOn = false;

    private void OnEnable() {

        if (!RCC_Settings.Instance.mobileControllerEnabled) {

            gameObject.SetActive(false);
            return;

        }

    }

    public void OnPointerDown(PointerEventData eventData) {

        isTurnedOn = RCC_SceneManager.Instance.activePlayerVehicle.highBeamHeadLightsOn = !isTurnedOn;
        button.color = isTurnedOn ? Color.yellow: Color.white;

    }

}
