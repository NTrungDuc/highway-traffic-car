using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using static HPS_CarDatas;

[CustomEditor(typeof(HPS_CarDatas))]
public class HPS_CarDatasEditor : Editor
{

    HPS_CarDatas prop;

    Vector2 scrollPos;

    List<HPS_CarDatas.CarDatas> carDatas = new();

    Color orgColor;

    public override void OnInspectorGUI()
    {
        prop = (HPS_CarDatas)target;
        serializedObject.Update();

        orgColor = GUI.color;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Car Datas Editor", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);

        EditorGUIUtility.labelWidth = 120f;

        GUILayout.Label("Player Cars", EditorStyles.boldLabel);

        EditorGUI.indentLevel++;

        for (int i = 0; i < prop.cardatas.Length; i++)
        {

            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField(prop.cardatas[i].carName, EditorStyles.boldLabel);

            prop.cardatas[i].carName = EditorGUILayout.TextField("Player Car Name", prop.cardatas[i].carName, GUILayout.MaxWidth(475f));

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();

            GameObject car = (GameObject)EditorGUILayout.ObjectField("Player Car Prefab", AssetDatabase.LoadAssetAtPath("Assets/Highway Racer/Prefabs/Player Vehicles/" + prop.cardatas[i].carName + ".prefab", typeof(GameObject)), typeof(GameObject), false, GUILayout.MaxWidth(475f));

            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Edit RCC"))
                Selection.activeGameObject = car;

            if (car && car.GetComponent<RCC_CarControllerV3>())
            {

                if (car.GetComponent<HR_ModApplier>() == null)
                    car.AddComponent<HR_ModApplier>();

                if (car.GetComponent<HR_PlayerHandler>() == null)
                    car.AddComponent<HR_PlayerHandler>();

                EditorGUILayout.Space();

                if (GUI.changed)
                    EditorUtility.SetDirty(car);

            }
            else
                EditorGUILayout.HelpBox("Select A RCC Based Car", MessageType.Error);

            EditorGUILayout.Space();

            if (prop.cardatas != null && prop.cardatas[i] != null && car) {

                EditorGUILayout.BeginHorizontal();
                car.GetComponent<HR_PlayerHandler>().Class = car.GetComponent<HR_PlayerHandler>().Class = EditorGUILayout.TextField("Class", car.GetComponent<HR_PlayerHandler>().Class, GUILayout.MaxWidth(200f));
                car.GetComponent<HR_PlayerHandler>().star = car.GetComponent<HR_PlayerHandler>().star = EditorGUILayout.IntField("Star", car.GetComponent<HR_PlayerHandler>().star, GUILayout.MaxWidth(200f));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                car.GetComponent<HR_PlayerHandler>().currentEngineTorque = car.GetComponent<RCC_CarControllerV3>().maxEngineTorque = EditorGUILayout.FloatField("Engine", car.GetComponent<RCC_CarControllerV3>().maxEngineTorque, GUILayout.MaxWidth(200f));
                car.GetComponent<HR_PlayerHandler>().maxEngineTorque = car.GetComponent<HR_PlayerHandler>().maxEngineTorque = EditorGUILayout.FloatField("Max Engine", car.GetComponent<HR_PlayerHandler>().maxEngineTorque, GUILayout.MaxWidth(200f));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                car.GetComponent<HR_PlayerHandler>().currentBrakeTorque = car.GetComponent<RCC_CarControllerV3>().brakeTorque = EditorGUILayout.FloatField("Brake", car.GetComponent<RCC_CarControllerV3>().brakeTorque, GUILayout.MaxWidth(200f));
                car.GetComponent<HR_PlayerHandler>().maxBrakeTorque = car.GetComponent<HR_PlayerHandler>().maxBrakeTorque = EditorGUILayout.FloatField("Max Brake", car.GetComponent<HR_PlayerHandler>().maxBrakeTorque, GUILayout.MaxWidth(200f));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                car.GetComponent<HR_PlayerHandler>().currentHandlingStrength = car.GetComponent<RCC_CarControllerV3>().steerHelperAngularVelStrength = EditorGUILayout.FloatField("Handling", car.GetComponent<RCC_CarControllerV3>().steerHelperAngularVelStrength, GUILayout.MaxWidth(200f));
                car.GetComponent<HR_PlayerHandler>().maxHandlingStrength = car.GetComponent<HR_PlayerHandler>().maxHandlingStrength = EditorGUILayout.FloatField("Max Handling", car.GetComponent<HR_PlayerHandler>().maxHandlingStrength, GUILayout.MaxWidth(200f));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                car.GetComponent<HR_PlayerHandler>().currentSpeed = car.GetComponent<RCC_CarControllerV3>().maxspeed = EditorGUILayout.FloatField("Speed", car.GetComponent<RCC_CarControllerV3>().maxspeed, GUILayout.MaxWidth(200f));
                car.GetComponent<HR_PlayerHandler>().maxSpeed = car.GetComponent<HR_PlayerHandler>().maxSpeed = EditorGUILayout.FloatField("Max Speed", car.GetComponent<HR_PlayerHandler>().maxSpeed, GUILayout.MaxWidth(200f));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                car.GetComponent<HR_PlayerHandler>().currentNOS = car.GetComponent<RCC_CarControllerV3>().NitroTime = EditorGUILayout.FloatField("NoS", car.GetComponent<RCC_CarControllerV3>().NitroTime, GUILayout.MaxWidth(200f));
                car.GetComponent<HR_PlayerHandler>().maxNOS = car.GetComponent<HR_PlayerHandler>().maxNOS = EditorGUILayout.FloatField("Max NoS", car.GetComponent<HR_PlayerHandler>().maxNOS, GUILayout.MaxWidth(200f));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();

                EditorGUILayout.BeginHorizontal();
                car.GetComponent<HR_PlayerHandler>().price = car.GetComponent<HR_PlayerHandler>().price = EditorGUILayout.IntField("Price", car.GetComponent<HR_PlayerHandler>().price, GUILayout.MaxWidth(200f));
                car.GetComponent<HR_PlayerHandler>().unlocked = car.GetComponent<HR_PlayerHandler>().unlocked = EditorGUILayout.ToggleLeft("Unlocked", car.GetComponent<HR_PlayerHandler>().unlocked, GUILayout.MaxWidth(122f));
                EditorGUILayout.EndHorizontal();

                if (car.GetComponent<HR_PlayerHandler>().maxEngineTorque < 0f)
                    car.GetComponent<HR_PlayerHandler>().maxEngineTorque = 0;

                if (car.GetComponent<HR_PlayerHandler>().maxBrakeTorque < 0f)
                    car.GetComponent<HR_PlayerHandler>().maxBrakeTorque = 0f;

                if (car.GetComponent<HR_PlayerHandler>().maxHandlingStrength < 0f)
                    car.GetComponent<HR_PlayerHandler>().maxHandlingStrength = 0f;

                if (car.GetComponent<HR_PlayerHandler>().maxEngineTorque > 1000f)
                    car.GetComponent<HR_PlayerHandler>().maxEngineTorque = 1000;

                if (car.GetComponent<HR_PlayerHandler>().maxBrakeTorque > 5500f)
                    car.GetComponent<HR_PlayerHandler>().maxBrakeTorque = 5500f;

                if (car.GetComponent<HR_PlayerHandler>().maxHandlingStrength > 1f)
                    car.GetComponent<HR_PlayerHandler>().maxHandlingStrength = 1f;

                if (car.GetComponent<HR_PlayerHandler>().maxNOS > 160f)
                    car.GetComponent<HR_PlayerHandler>().maxNOS = 160f;

            }

            EditorGUILayout.BeginHorizontal();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("\u2191", GUILayout.MaxWidth(25f)))
                Up(i);

            if (GUILayout.Button("\u2193", GUILayout.MaxWidth(25f)))
                Down(i);

            GUI.color = Color.red;

            if (GUILayout.Button("X", GUILayout.MaxWidth(25f)))
                RemoveCar(i);

            GUI.color = orgColor;

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();
        }

        GUI.color = Color.cyan;

        if (GUILayout.Button("Create Player Car"))
            AddNewCar();

        if (GUILayout.Button("Load Car Data"))
            LoadCarDatas();

        GUI.color = orgColor;

        EditorGUILayout.EndScrollView();    

        EditorGUILayout.Space();

        serializedObject.ApplyModifiedProperties();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(prop);
            SaveCarDatas();
        }

    }

    void LoadCarDatas()
    {
        carDatas.Clear();
        var text = File.ReadAllText(Application.dataPath + "/Resources/HPS_CarDatas_Json.json");
        carDatas = JsonUtility.FromJson<List<CarDatas>>(File.ReadAllText(Application.dataPath + "/Resources/HPS_CarDatas_Json.json"));
    }

    void SaveCarDatas()
    {
        HPS_CarDatas prop = CreateInstance("HPS_CarDatas") as HPS_CarDatas;
        prop.cardatas = new CarDatas[this.prop.cardatas.Length];

        for (int i = 0; i < this.prop.cardatas.Length; i++)
        {
            prop.cardatas[i] = new CarDatas();
            GameObject car = (GameObject)EditorGUILayout.ObjectField("Player Car Prefab", AssetDatabase.LoadAssetAtPath("Assets/Highway Racer/Prefabs/Player Vehicles/" + this.prop.cardatas[i].carName + ".prefab", typeof(GameObject)), typeof(GameObject), false, GUILayout.MaxWidth(475f));
            GameObject Car = (GameObject)Resources.Load("Player Vehicles/" + Instance.cardatas[i].carName);
            if (car && Car)
            {
                prop.cardatas[i].carName = Car.name = car.name;
                prop.cardatas[i].carClass = Car.GetComponent<HR_PlayerHandler>().Class = car.GetComponent<HR_PlayerHandler>().Class;
                prop.cardatas[i].stars = Car.GetComponent<HR_PlayerHandler>().star = car.GetComponent<HR_PlayerHandler>().star;
                prop.cardatas[i].defaultEngineTorque = Car.GetComponent<HR_PlayerHandler>().currentEngineTorque = car.GetComponent<HR_PlayerHandler>().currentEngineTorque;
                prop.cardatas[i].defaultHandlingStrength = Car.GetComponent<HR_PlayerHandler>().currentHandlingStrength = car.GetComponent<HR_PlayerHandler>().currentHandlingStrength;
                prop.cardatas[i].defaultSpeed = Car.GetComponent<HR_PlayerHandler>().currentSpeed = car.GetComponent<HR_PlayerHandler>().currentSpeed;
                prop.cardatas[i].defaultNOS = Car.GetComponent<HR_PlayerHandler>().currentNOS = car.GetComponent<HR_PlayerHandler>().currentNOS;
                prop.cardatas[i].maxEngineTorque = Car.GetComponent<HR_PlayerHandler>().maxEngineTorque = car.GetComponent<HR_PlayerHandler>().maxEngineTorque;
                prop.cardatas[i].maxHandlingStrength = Car.GetComponent<HR_PlayerHandler>().maxHandlingStrength = car.GetComponent<HR_PlayerHandler>().maxHandlingStrength;
                prop.cardatas[i].maxSpeed = Car.GetComponent<HR_PlayerHandler>().maxSpeed = car.GetComponent<HR_PlayerHandler>().maxSpeed;
                prop.cardatas[i].maxNOS = Car.GetComponent<HR_PlayerHandler>().maxNOS = car.GetComponent<HR_PlayerHandler>().maxNOS;
                prop.cardatas[i].unlocked = Car.GetComponent<HR_PlayerHandler>().unlocked = car.GetComponent<HR_PlayerHandler>().unlocked;
                prop.cardatas[i].price = Car.GetComponent<HR_PlayerHandler>().price = car.GetComponent<HR_PlayerHandler>().price;
            }
        }

        File.WriteAllText(Application.dataPath + "/Resources/HPS_CarDatas_Json.json", JsonUtility.ToJson(prop, true));
    }

    void AddNewCar()
    {

        carDatas.Clear();
        carDatas.AddRange(prop.cardatas);
        CarDatas newCar = new CarDatas();
        carDatas.Add(newCar);
        prop.cardatas = carDatas.ToArray();
        PlayerPrefs.SetInt("SelectedPlayerCarIndex", 0);

    }

    void RemoveCar(int index)
    {

        carDatas.Clear();
        carDatas.AddRange(prop.cardatas);
        carDatas.RemoveAt(index);
        prop.cardatas = carDatas.ToArray();
        PlayerPrefs.SetInt("SelectedPlayerCarIndex", 0);

    }

    void Up(int index)
    {

        if (index <= 0)
            return;

        carDatas.Clear();
        carDatas.AddRange(prop.cardatas);

        CarDatas currentCar = carDatas[index];
        CarDatas previousCar = carDatas[index - 1];

        carDatas.RemoveAt(index);
        carDatas.RemoveAt(index - 1);

        carDatas.Insert(index - 1, currentCar);
        carDatas.Insert(index, previousCar);

        prop.cardatas = carDatas.ToArray();
        PlayerPrefs.SetInt("SelectedPlayerCarIndex", 0);

    }

    void Down(int index)
    {

        if (index >= prop.cardatas.Length - 1)
            return;

        carDatas.Clear();
        carDatas.AddRange(prop.cardatas);

        CarDatas currentCar = carDatas[index];
        CarDatas nextCar = carDatas[index + 1];

        carDatas.RemoveAt(index);
        carDatas.Insert(index, nextCar);

        carDatas.RemoveAt(index + 1);
        carDatas.Insert(index + 1, currentCar);

        prop.cardatas = carDatas.ToArray();
        PlayerPrefs.SetInt("SelectedPlayerCarIndex", 0);

    }

}