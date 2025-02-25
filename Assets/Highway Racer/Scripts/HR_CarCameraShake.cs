//----------------------------------------------
//           	   Highway Racer
//
// Copyright © 2014 - 2021 BoneCracker Games
// http://www.bonecrackergames.com
//
//----------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HR_CarCameraShake : MonoBehaviour {

    private HR_CarCamera carCamera;
    private Rigidbody carRigid;

    public float maxShakeAmount = .1f;
    public float minShakeAmount = 0f;
    public float speedThreshold = 100f;

    private Vector3 initialPosition;

    private void Start() {

        carCamera = GetComponentInParent<HR_CarCamera>();
        initialPosition = transform.localPosition;

    }

    private void Update() {

        if (!carCamera)
            return;

        if (!carCamera.playerCar)
            return;

        if (!carRigid)
            carRigid = carCamera.playerCar.GetComponent<Rigidbody>();

        if (!carRigid)
            return;

        float shakeAmount = Mathf.Lerp(minShakeAmount, maxShakeAmount, carRigid.velocity.magnitude / speedThreshold);
        Vector3 newPos = initialPosition + Random.insideUnitSphere * shakeAmount;
        transform.localPosition = Vector3.Lerp(transform.localPosition, newPos, Time.deltaTime * 10f);

    }

}
