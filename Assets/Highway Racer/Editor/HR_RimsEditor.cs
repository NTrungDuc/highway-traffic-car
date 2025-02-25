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

[CustomEditor(typeof(HR_Rims))]
public class HR_RimsEditor : Editor
{

    HR_Rims prop;

    Vector2 scrollPos;
    List<HR_Rims.Rims> playerCars = new List<HR_Rims.Rims>();

    Color orgColor;

    public override void OnInspectorGUI()
    {

        prop = (HR_Rims)target;
        serializedObject.Update();

        orgColor = GUI.color;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Rims Editor", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("This editor will keep update necessary .asset files in your project. Don't change directory of the ''Resources/HR_Assets''.", EditorStyles.helpBox);
        EditorGUILayout.Space();

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);

        EditorGUIUtility.labelWidth = 110f;
        //EditorGUIUtility.fieldWidth = 10f;

        GUILayout.Label("Rims", EditorStyles.boldLabel);

        for (int i = 0; i < prop.rims.Length; i++)
        {

            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.Space();

            if (prop.rims[i].rim)
                EditorGUILayout.LabelField(prop.rims[i].rim.name, EditorStyles.boldLabel);

            EditorGUILayout.Space();
            prop.rims[i].rim = (GameObject)EditorGUILayout.ObjectField("Rim Prefab", prop.rims[i].rim, typeof(GameObject), false);
            EditorGUILayout.Space();

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();

            if (prop.rims[i].price <= 0)
                prop.rims[i].unlocked = true;

            prop.rims[i].unlocked = EditorGUILayout.ToggleLeft("Unlocked", prop.rims[i].unlocked, GUILayout.Width(150f)); prop.rims[i].price = EditorGUILayout.IntField("Price", prop.rims[i].price, GUILayout.Width(150f)); GUI.color = Color.red; if (GUILayout.Button("Remove")) { RemoveWheel(i); }
            GUI.color = orgColor;

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();

        }

        GUI.color = Color.cyan;

        if (GUILayout.Button("Create Rim"))
        {

            AddNewWheel();

        }

        if (GUILayout.Button("--< Return To General Settings"))
        {

            OpenGeneralSettings();

        }

        GUI.color = orgColor;

        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Highway Racer" + HR_Version.version + "\nCreated by Buğra Özdoğanlar\nBoneCrackerGames", EditorStyles.centeredGreyMiniLabel, GUILayout.MaxHeight(50f));

        if (GUI.changed)
            EditorUtility.SetDirty(prop);

        serializedObject.ApplyModifiedProperties();

    }

    void AddNewWheel()
    {

        playerCars.Clear();
        playerCars.AddRange(prop.rims);
        HR_Rims.Rims newCar = new HR_Rims.Rims();
        playerCars.Add(newCar);
        prop.rims = playerCars.ToArray();

    }

    void RemoveWheel(int index)
    {

        playerCars.Clear();
        playerCars.AddRange(prop.rims);
        playerCars.RemoveAt(index);
        prop.rims = playerCars.ToArray();

    }

    void OpenGeneralSettings()
    {

        Selection.activeObject = HR_HighwayRacerProperties.Instance;

    }

}
