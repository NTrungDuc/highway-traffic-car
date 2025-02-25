//----------------------------------------------
//           	   Highway Racer
//
// Copyright © 2014 - 2023 BoneCracker Games
// http://www.bonecrackergames.com
//
//----------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
[AddComponentMenu("BoneCracker Games/Highway Racer/UI/HR UI Button Sound")]
public class HR_UIButtonSound : MonoBehaviour, IPointerClickHandler {

    private AudioSource clickSound;

    public void OnPointerClick(PointerEventData data) {

        if (Camera.main) {

            if (data.pointerCurrentRaycast.gameObject.TryGetComponent(out Button button) && button.interactable) {

                clickSound = HR_CreateAudioSource.NewAudioSource(Camera.main.gameObject, HR_HighwayRacerProperties.Instance.buttonClickAudioClip.name, 0f, 0f, 1f, HR_HighwayRacerProperties.Instance.buttonClickAudioClip, false, true, true);
                clickSound.ignoreListenerPause = true;
            }

        }

    }

}
