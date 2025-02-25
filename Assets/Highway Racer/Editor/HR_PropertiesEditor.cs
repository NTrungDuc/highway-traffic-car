//----------------------------------------------
//           	   Highway Racer
//
// Copyright © 2014 - 2023 BoneCracker Games
// http://www.bonecrackergames.com
//
//----------------------------------------------

using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(HR_HighwayRacerProperties))]
public class HR_PropertiesEditor : Editor {

    HR_HighwayRacerProperties prop;

    Vector2 scrollPos;
    GameObject[] playerCars;
    GameObject[] upgradableRims;
    GameObject[] upgradableTires;
    GameObject[] upgradableWheels;

    Color orgColor;

    private int _Width = 500;
    public int Width {
        get {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            Rect scale = GUILayoutUtility.GetLastRect();

            if (scale.width != 1) {
                _Width = System.Convert.ToInt32(scale.width);
            }

            return _Width;
        }
    }

    public override void OnInspectorGUI() {

        prop = (HR_HighwayRacerProperties)target;
        serializedObject.Update();

        orgColor = GUI.color;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Highway Racer Properties Editor Window", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("This editor will keep update necessary .asset files in your project. Don't change directory of the ''Resources/HR_Assets''.", EditorStyles.helpBox);
        EditorGUILayout.Space();

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);

        EditorGUIUtility.labelWidth = 180f;

        GUILayout.Label("General Settings", EditorStyles.boldLabel);

        prop._minimumSpeedForGainScore = EditorGUILayout.IntField("Min Speed For Gain Score", prop._minimumSpeedForGainScore);
        prop._minimumSpeedForHighSpeed = EditorGUILayout.IntField("Min Speed For High Speed", prop._minimumSpeedForHighSpeed);
        prop._minimumCollisionForGameOver = EditorGUILayout.IntField("Min Col For Game Over", prop._minimumCollisionForGameOver);

        EditorGUILayout.Space();

        GUILayout.Label("Default Config At First Initialization", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Default settings when there are no any saved PlayerPrefs data. At first init only on target platform.", MessageType.Info, true);

        EditorGUILayout.Space();
        prop._defaultEngineTorque = EditorGUILayout.FloatField("Default Engine Torque", prop._defaultEngineTorque, GUILayout.ExpandWidth(true));
        prop._defaultHandlingStrength = EditorGUILayout.FloatField("Default Handling Strength", prop._defaultHandlingStrength, GUILayout.ExpandWidth(true));
        prop._defaultSpeed = EditorGUILayout.FloatField("Default Speed", prop._defaultSpeed, GUILayout.ExpandWidth(true));
        prop._defaultNOS = EditorGUILayout.FloatField("Default Nitro Time", prop._defaultNOS, GUILayout.ExpandWidth(true));
        prop._defaultBodyColor = EditorGUILayout.ColorField("Default Body Color", prop._defaultBodyColor);
        EditorGUILayout.Space();

        prop._1MMoneyForTesting = EditorGUILayout.ToggleLeft("1M Money For Testing", prop._1MMoneyForTesting, GUILayout.ExpandWidth(true));
        prop._tiltCamera = EditorGUILayout.ToggleLeft("Camera Tilt", prop._tiltCamera, GUILayout.ExpandWidth(true));

        EditorGUILayout.Space();

        GUILayout.Label("Score Multipliers", EditorStyles.boldLabel);

        prop._totalDistanceMoneyMP = EditorGUILayout.IntField("Total Distance MP", prop._totalDistanceMoneyMP, GUILayout.ExpandWidth(true));
        prop._totalNearMissMoneyMP = EditorGUILayout.IntField("Total Near Miss MP", prop._totalNearMissMoneyMP, GUILayout.ExpandWidth(true));
        prop._totalOverspeedMoneyMP = EditorGUILayout.IntField("Total Over Speed MP", prop._totalOverspeedMoneyMP, GUILayout.ExpandWidth(true));
        prop._totalOppositeDirectionMP = EditorGUILayout.IntField("Total Opposite Direction MP", prop._totalOppositeDirectionMP, GUILayout.ExpandWidth(true));

        EditorGUILayout.Space();

        GUILayout.Label("Sound Effects", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("mainMenuClips"), new GUIContent("Main Menu Soundtracks"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("gameplayClips"), new GUIContent("GamePlay Soundtracks"), true);
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("buttonClickAudioClip"), new GUIContent("Button Click SFX"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("nearMissAudioClip"), new GUIContent("Near Miss SFX"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("labelSlideAudioClip"), new GUIContent("Label Slide SFX"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("countingPointsAudioClip"), new GUIContent("Counting Points SFX"));
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("bombTimerAudioClip"), new GUIContent("Bomb Timer Beep SFX"));
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("sirenAudioClip"), new GUIContent("Police Siren SFX"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("hornClip"), new GUIContent("Horn SFX"));

        EditorGUILayout.Space();

        GUI.color = new Color(.5f, 1f, 1f, 1f);
        GUILayout.Label("Select Main Controller Type", EditorStyles.boldLabel);

        if (GUILayout.Button("Switch Controller from RCC Settings"))
            Selection.activeObject = RCC_Settings.Instance;

        GUI.color = orgColor;

        EditorGUILayout.Space();

        GUILayout.Label("Selectable Player Cars", EditorStyles.boldLabel);

        playerCars = new GameObject[HR_PlayerCars.Instance.cars.Length];

        for (int i = 0; i < playerCars.Length; i++) {
            playerCars[i] = HR_PlayerCars.Instance.cars[i].playerCar;
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.ObjectField("Player Car " + i, playerCars[i], typeof(GameObject), false);
            EditorGUILayout.EndVertical();
        }

        GUI.color = Color.cyan;

        if (GUILayout.Button("Configure Player Cars")) {
            Selection.activeObject = Resources.Load("HR_PlayerCars") as HR_PlayerCars;
        }

        GUI.color = orgColor;

        EditorGUILayout.Space();

        GUILayout.Label("Upgradable Rims", EditorStyles.boldLabel);

        upgradableRims = new GameObject[HR_Rims.Instance.rims.Length];

        for (int i = 0; i < upgradableRims.Length; i++)
        {
            upgradableRims[i] = HR_Rims.Instance.rims[i].rim;
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.ObjectField("Upgradable Rims " + i, upgradableRims[i], typeof(GameObject), false);
            EditorGUILayout.EndVertical();
        }

        GUI.color = Color.cyan;

        if (GUILayout.Button("Configure Upgradable Rims"))
        {
            Selection.activeObject = Resources.Load("HR_Rims") as HR_Rims;
        }

        GUI.color = orgColor;

        EditorGUILayout.Space();

        GUILayout.Label("Upgradable Tires", EditorStyles.boldLabel);

        upgradableTires = new GameObject[HR_Tires.Instance.tires.Length];

        for (int i = 0; i < upgradableTires.Length; i++)
        {
            upgradableTires[i] = HR_Tires.Instance.tires[i].tire;
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.ObjectField("Upgradable Tires " + i, upgradableTires[i], typeof(GameObject), false);
            EditorGUILayout.EndVertical();
        }

        GUI.color = Color.cyan;

        if (GUILayout.Button("Configure Upgradable Tires"))
        {
            Selection.activeObject = Resources.Load("HR_Tires") as HR_Tires;
        }

        GUI.color = orgColor;

        EditorGUILayout.Space();

        GUILayout.Label("Upgradable Wheels", EditorStyles.boldLabel);

        upgradableWheels = new GameObject[HR_Wheels.Instance.wheels.Length];

        for (int i = 0; i < upgradableWheels.Length; i++) {
            upgradableWheels[i] = HR_Wheels.Instance.wheels[i].wheel;
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.ObjectField("Upgradable Wheels " + i, upgradableWheels[i], typeof(GameObject), false);
            EditorGUILayout.EndVertical();
        }

        GUI.color = Color.cyan;

        if (GUILayout.Button("Configure Upgradable Wheels")) {
            Selection.activeObject = Resources.Load("HR_Wheels") as HR_Wheels;
        }

        GUI.color = orgColor;

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("explosionEffect"), new GUIContent("Explosion Particle Effects"));

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("trafficCarsLayer"), new GUIContent("Layer Of The Traffic Cars"));

        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Highway Racer" + HR_Version.version + "\nCreated by Buğra Özdoğanlar\nBoneCrackerGames", EditorStyles.centeredGreyMiniLabel, GUILayout.MaxHeight(50f));

        if (GUI.changed)
            EditorUtility.SetDirty(prop);

        serializedObject.ApplyModifiedProperties();

    }

    void ResetToDefaults() {

        prop._minimumSpeedForGainScore = 80;
        prop._minimumSpeedForHighSpeed = 100;
        prop._minimumCollisionForGameOver = 5;
        prop._defaultBodyColor = Color.red + (Color.green / 2f);

        prop._1MMoneyForTesting = false;

        prop._totalDistanceMoneyMP = 120;
        prop._totalNearMissMoneyMP = 10;
        prop._totalOverspeedMoneyMP = 5;
        prop._totalOppositeDirectionMP = 5;

    }

}
