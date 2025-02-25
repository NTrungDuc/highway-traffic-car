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

public class HR_EngineSmoke : MonoBehaviour {

    private HR_PlayerHandler playerHandler;
    private ParticleSystem particles;

    private void Start() {

        playerHandler = GetComponentInParent<HR_PlayerHandler>();
        particles = GetComponent<ParticleSystem>();

    }

    private void Update() {

        if (!playerHandler || !particles)
            return;

        ParticleSystem.EmissionModule em = particles.emission;

        if (playerHandler.damage >= 60f) {

            if (!em.enabled)
                em.enabled = true;

        } else {

            if (em.enabled)
                em.enabled = false;

        }

    }
}
