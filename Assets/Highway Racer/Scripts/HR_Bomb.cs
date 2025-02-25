//----------------------------------------------
//           	   Highway Racer
//
// Copyright © 2014 - 2023 BoneCracker Games
// http://www.bonecrackergames.com
//
//----------------------------------------------

using UnityEngine;
using System.Collections;

/// <summary>
/// Bomb with timer and SFX.
/// </summary>
[AddComponentMenu("BoneCracker Games/Highway Racer/Misc/HR Bomb")]
public class HR_Bomb : MonoBehaviour {

    //  Player.
    private HR_PlayerHandler player;
    private HR_PlayerHandler Player {

        get {

            if (player == null)
                player = GetComponentInParent<HR_PlayerHandler>();

            return player;

        }

    }

    private Light bombLight;        //  Light.
    private float bombTimer = 0f;     //  Timer.

    private AudioSource bombTimerAudioSource;       //  SFX.
    private AudioClip BombTimerAudiclip { get { return HR_HighwayRacerProperties.Instance.bombTimerAudioClip; } }

    private void Awake() {

        //  If game mode is bomb, enable it, Otherwise disable it.
        if (HR_GamePlayHandler.Instance) {

            if (HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.Bomb)
                gameObject.SetActive(true);
            else
                gameObject.SetActive(false);

        } else {

            gameObject.SetActive(false);
            return;

        }

        //  Getting player handler and creating light with SFX.
        bombTimerAudioSource = HR_CreateAudioSource.NewAudioSource(gameObject, "Bomb Timer AudioSource", 0f, 0f, .25f, BombTimerAudiclip, false, false, false);
        bombLight = GetComponentInChildren<Light>();
        bombLight.enabled = true;
        bombLight.intensity = 0f;

    }

    private void FixedUpdate() {

        //  If no player found, return.
        if (!Player)
            return;

        //  If bomb is not triggered, return.
        if (!Player.bombTriggered)
            return;

        //  Adjusting signal light timer.
        bombTimer += Time.fixedDeltaTime * Mathf.Lerp(5f, 1f, Player.bombHealth / 100f);

        //  Light.
        if (bombTimer >= .5f)
            bombLight.intensity = Mathf.Lerp(bombLight.intensity, 0f, Time.fixedDeltaTime * 50f);
        else
            bombLight.intensity = Mathf.Lerp(bombLight.intensity, 1f, Time.fixedDeltaTime * 50f);

        if (bombTimer >= 1f) {

            bombTimer = 0f;
            bombTimerAudioSource.Play();

        }

    }

}
