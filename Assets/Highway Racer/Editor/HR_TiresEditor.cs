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

[CustomEditor(typeof(HR_Tires))]
public class HR_TiresEditor : Editor {

    HR_Tires prop;

    Vector2 scrollPos;
    List<HR_Tires.Tires> playerCars = new List<HR_Tires.Tires>();

    Color orgColor;

    public override void OnInspectorGUI()
    {

        prop = (HR_Tires)target;
        serializedObject.Update();

        orgColor = GUI.color;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Tires Editor", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("This editor will keep update necessary .asset files in your project. Don't change directory of the ''Resources/HR_Assets''.", EditorStyles.helpBox);
        EditorGUILayout.Space();

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);

        EditorGUIUtility.labelWidth = 110f;
        //EditorGUIUtility.fieldWidth = 10f;

        GUILayout.Label("Tires", EditorStyles.boldLabel);

        for (int i = 0; i < prop.tires.Length; i++)
        {

            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.Space();

            if (prop.tires[i].tire)
                EditorGUILayout.LabelField(prop.tires[i].tire.name, EditorStyles.boldLabel);

            EditorGUILayout.Space();
            prop.tires[i].tire = (GameObject)EditorGUILayout.ObjectField("Tire Prefab", prop.tires[i].tire, typeof(GameObject), false);
            EditorGUILayout.Space();

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();

            if (prop.tires[i].price <= 0)
                prop.tires[i].unlocked = true;

            prop.tires[i].unlocked = EditorGUILayout.ToggleLeft("Unlocked", prop.tires[i].unlocked, GUILayout.Width(150f)); prop.tires[i].price = EditorGUILayout.IntField("Price", prop.tires[i].price, GUILayout.Width(150f)); GUI.color = Color.red; if (GUILayout.Button("Remove")) { RemoveWheel(i); }
            GUI.color = orgColor;

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();

        }

        GUI.color = Color.cyan;

        if (GUILayout.Button("Create Tire"))
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
        playerCars.AddRange(prop.tires);
        HR_Tires.Tires newCar = new HR_Tires.Tires();
        playerCars.Add(newCar);
        prop.tires = playerCars.ToArray();

    }

    void RemoveWheel(int index)
    {

        playerCars.Clear();
        playerCars.AddRange(prop.tires);
        playerCars.RemoveAt(index);
        prop.tires = playerCars.ToArray();

    }

    void OpenGeneralSettings()
    {

        Selection.activeObject = HR_HighwayRacerProperties.Instance;

    }

}
