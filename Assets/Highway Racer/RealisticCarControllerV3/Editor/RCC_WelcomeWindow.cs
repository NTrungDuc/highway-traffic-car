//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2014 - 2023 BoneCracker Games
// https://www.bonecrackergames.com
// Buğra Özdoğanlar
//
//----------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class RCC_WelcomeWindow : EditorWindow {

    public class ToolBar {

        public string title;
        public UnityEngine.Events.UnityAction Draw;

        /// <summary>
        /// Create New Toolbar
        /// </summary>
        /// <param name="title">Title</param>
        /// <param name="onDraw">Method to draw when toolbar is selected</param>
        public ToolBar(string title, UnityEngine.Events.UnityAction onDraw) {

            this.title = title;
            this.Draw = onDraw;

        }

        public static implicit operator string(ToolBar tool) {
            return tool.title;
        }

    }

    /// <summary>
    /// Index of selected toolbar.
    /// </summary>
    public int toolBarIndex = 0;

    /// <summary>
    /// List of Toolbars
    /// </summary>
    public ToolBar[] toolBars = new ToolBar[]{

        new ToolBar("Welcome", WelcomePageContent),
        new ToolBar("DOCS", Documentations)

    };

    public static Texture2D bannerTexture = null;

    private GUISkin skin;

    private const int windowWidth = 600;
    private const int windowHeight = 750;

    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Welcome Window", false, 10000)]
    public static void OpenWindow() {

        GetWindow<RCC_WelcomeWindow>(true);

    }

    private void OnEnable() {

        titleContent = new GUIContent("Realistic Car Controller");
        maxSize = new Vector2(windowWidth, windowHeight);
        minSize = maxSize;

        InitStyle();

    }

    private void InitStyle() {

        if (!skin)
            skin = Resources.Load("RCC_WindowSkin") as GUISkin;

        bannerTexture = (Texture2D)Resources.Load("Editor/RCCBanner", typeof(Texture2D));

    }

    private void OnGUI() {

        GUI.skin = skin;

        DrawHeader();
        DrawMenuButtons();
        DrawToolBar();
        DrawFooter();

        if (!EditorApplication.isPlaying)
            Repaint();

    }

    private void DrawHeader() {

        GUILayout.Label(bannerTexture, GUILayout.Height(120));

    }

    private void DrawMenuButtons() {

        GUILayout.Space(-10);
        toolBarIndex = GUILayout.Toolbar(toolBarIndex, ToolbarNames());

    }

    #region ToolBars

    public static void WelcomePageContent() {

        EditorGUILayout.BeginVertical("window");
        GUILayout.Label("Welcome!");
        EditorGUILayout.BeginHorizontal("box");
        GUILayout.Label("<b>Thank you for purchasing and using Realistic Car Controller. Please read the documentation before use. Also check out the online documentation for updated info. Have fun :)</b>");
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Separator();

        GUILayout.FlexibleSpace();

        GUI.color = Color.red;

        if (GUILayout.Button("Delete all demo contents from the project")) {

            if (EditorUtility.DisplayDialog("Warning", "You are about to delete all demo contents such as vehicle models, vehicle prefabs, vehicle textures, all scenes, scene models, scene prefabs, scene textures!", "Delete", "Cancel"))
                DeleteDemoContent();

        }

        GUI.color = Color.white;

        EditorGUILayout.EndVertical();

    }

    public static void Documentations() {

        EditorGUILayout.BeginVertical("window");

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.HelpBox("Latest online documentations for scripts, settings, setup, how to do, and API.", MessageType.Info);

        if (GUILayout.Button("Documentation"))
            Application.OpenURL(RCC_AssetPaths.documentations);

        if (GUILayout.Button("Youtube Tutorial Videos"))
            Application.OpenURL(RCC_AssetPaths.YTVideos);

        if (GUILayout.Button("Other Assets"))
            Application.OpenURL(RCC_AssetPaths.otherAssets);

        EditorGUILayout.EndVertical();

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndVertical();

    }

    #endregion

    private string[] ToolbarNames() {

        string[] names = new string[toolBars.Length];

        for (int i = 0; i < toolBars.Length; i++)
            names[i] = toolBars[i];

        return names;

    }

    private void DrawToolBar() {

        GUILayout.BeginArea(new Rect(4, 140, 592, 540));

        toolBars[toolBarIndex].Draw();

        GUILayout.EndArea();

        GUILayout.FlexibleSpace();

    }

    private void DrawFooter() {

        EditorGUILayout.BeginHorizontal("box");

        EditorGUILayout.LabelField("BoneCracker Games", EditorStyles.centeredGreyMiniLabel);
        EditorGUILayout.LabelField("Realistic Car Controller " + RCC_Version.version, EditorStyles.centeredGreyMiniLabel);
        EditorGUILayout.LabelField("Buğra Özdoğanlar", EditorStyles.centeredGreyMiniLabel);

        EditorGUILayout.EndHorizontal();

    }

    private static void ImportPackage(string package) {

        try {
            AssetDatabase.ImportPackage(package, true);
        } catch (Exception) {
            Debug.LogError("Failed to import package: " + package);
            throw;
        }

    }

    private static void DeleteDemoContent() {

        Debug.LogWarning("Deleting demo contents...");

        foreach (var item in RCC_AssetPaths.demoAssetPaths)
            FileUtil.DeleteFileOrDirectory(item);

        AssetDatabase.Refresh();

        Debug.LogWarning("Deleted demo contents!");
        EditorUtility.DisplayDialog("Deleted Demo Contents", "All demo contents have been deleted!", "Ok");

    }

}
