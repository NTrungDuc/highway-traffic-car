using UnityEditor;
using UnityEngine;

namespace HT.DEBUG.Editor
{
    public class LocalizationWindow : EditorWindow
    {
        private Localization _localization;
        private int _currentEdit = 0;
        private Vector2 _scroll;

        [MenuItem("Window/DebugController Localization")]
        private static void Open()
        {
            LocalizationWindow window = GetWindow<LocalizationWindow>();
            window.titleContent.text = "Localization";
            window.Show();
        }

        private void OnEnable()
        {
            _localization = Resources.Load<Localization>("LocalizationData");
        }

        private void OnGUI()
        {
            if (_localization)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Language: " + _localization.Language);
                GUILayout.FlexibleSpace();
                GUILayout.Label("Change: ");
                _localization.Language = (LanguageType)EditorGUILayout.EnumPopup(_localization.Language);
                if (GUILayout.Button("Reset", "Minibutton"))
                {
                    ResetLocalization();
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                if (GUILayout.Toggle(_currentEdit == 0, "Edit English Config", "Button"))
                {
                    _currentEdit = 0;
                }
                GUILayout.EndHorizontal();

                _scroll = GUILayout.BeginScrollView(_scroll);
                if (_currentEdit == 0)
                {
                    for (int i = 0; i < _localization.LanguageKey.Count; i++)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("[" + i + "] " + _localization.LanguageKey[i] + ": ", GUILayout.Width(position.width / 2));
                        _localization.LanguageEnglish[i] = GUILayout.TextField(_localization.LanguageEnglish[i]);
                        GUILayout.EndHorizontal();
                    }
                }
                GUILayout.EndScrollView();
            }
            else
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("丢失本地化配置文件，请重新导入资源包！");
                GUILayout.EndHorizontal();
            }
        }

        private void ResetLocalization()
        {
            _localization.LanguageKey.Clear();
            _localization.LanguageEnglish.Clear();

            _localization.LanguageKey.Add("DEBUGGER");
            _localization.LanguageEnglish.Add("DEBUGGER");

            _localization.LanguageKey.Add("FPS");
            _localization.LanguageEnglish.Add("FPS");

            _localization.LanguageKey.Add("Console");
            _localization.LanguageEnglish.Add("Console");

            _localization.LanguageKey.Add("Scene");
            _localization.LanguageEnglish.Add("Scene");

            _localization.LanguageKey.Add("Memory");
            _localization.LanguageEnglish.Add("Memory");

            _localization.LanguageKey.Add("DrawCall");
            _localization.LanguageEnglish.Add("DrawCall");

            _localization.LanguageKey.Add("System");
            _localization.LanguageEnglish.Add("System");

            _localization.LanguageKey.Add("Screen");
            _localization.LanguageEnglish.Add("Screen");

            _localization.LanguageKey.Add("Quality");
            _localization.LanguageEnglish.Add("Quality");

            _localization.LanguageKey.Add("Time");
            _localization.LanguageEnglish.Add("Time");

            _localization.LanguageKey.Add("Environment");
            _localization.LanguageEnglish.Add("Environment");

            _localization.LanguageKey.Add("Clear");
            _localization.LanguageEnglish.Add("Clear");

            _localization.LanguageKey.Add("Info");
            _localization.LanguageEnglish.Add("Info");

            _localization.LanguageKey.Add("Warning");
            _localization.LanguageEnglish.Add("Warning");

            _localization.LanguageKey.Add("Error");
            _localization.LanguageEnglish.Add("Error");

            _localization.LanguageKey.Add("Fatal");
            _localization.LanguageEnglish.Add("Fatal");

            _localization.LanguageKey.Add("Refresh");
            _localization.LanguageEnglish.Add("Refresh");

            _localization.LanguageKey.Add("GameObjects");
            _localization.LanguageEnglish.Add("GameObjects");

            _localization.LanguageKey.Add("Search");
            _localization.LanguageEnglish.Add("Search");

            _localization.LanguageKey.Add("Add Component");
            _localization.LanguageEnglish.Add("Add Component");

            _localization.LanguageKey.Add("Delete Component");
            _localization.LanguageEnglish.Add("Delete Component");

            _localization.LanguageKey.Add("Debug Camera");
            _localization.LanguageEnglish.Add("Debug Camera");

            _localization.LanguageKey.Add("Depth Topping");
            _localization.LanguageEnglish.Add("Depth Topping");

            _localization.LanguageKey.Add("Memory Information");
            _localization.LanguageEnglish.Add("Memory Information");

            _localization.LanguageKey.Add("Total Memory");
            _localization.LanguageEnglish.Add("Total Memory");

            _localization.LanguageKey.Add("Used Memory");
            _localization.LanguageEnglish.Add("Used Memory");

            _localization.LanguageKey.Add("Free Memory");
            _localization.LanguageEnglish.Add("Free Memory");

            _localization.LanguageKey.Add("Total Mono Memory");
            _localization.LanguageEnglish.Add("Total Mono Memory");

            _localization.LanguageKey.Add("Used Mono Memory");
            _localization.LanguageEnglish.Add("Used Mono Memory");

            _localization.LanguageKey.Add("Uninstall unused resources");
            _localization.LanguageEnglish.Add("Uninstall unused resources");

            _localization.LanguageKey.Add("Garbage Collection");
            _localization.LanguageEnglish.Add("Garbage Collection");

            _localization.LanguageKey.Add("DrawCall Information");
            _localization.LanguageEnglish.Add("DrawCall Information");

            _localization.LanguageKey.Add("DrawCalls");
            _localization.LanguageEnglish.Add("DrawCalls");

            _localization.LanguageKey.Add("Batches");
            _localization.LanguageEnglish.Add("Batches");

            _localization.LanguageKey.Add("Static Batched DrawCalls");
            _localization.LanguageEnglish.Add("Static Batched DrawCalls");

            _localization.LanguageKey.Add("Static Batches");
            _localization.LanguageEnglish.Add("Static Batches");

            _localization.LanguageKey.Add("Dynamic Batched DrawCalls");
            _localization.LanguageEnglish.Add("Dynamic Batched DrawCalls");

            _localization.LanguageKey.Add("Dynamic Batches");
            _localization.LanguageEnglish.Add("Dynamic Batches");

            _localization.LanguageKey.Add("Triangles");
            _localization.LanguageEnglish.Add("Triangles");

            _localization.LanguageKey.Add("Vertices");
            _localization.LanguageEnglish.Add("Vertices");

            _localization.LanguageKey.Add("System Information");
            _localization.LanguageEnglish.Add("System Information");

            _localization.LanguageKey.Add("Operating System");
            _localization.LanguageEnglish.Add("Operating System");

            _localization.LanguageKey.Add("System Memory");
            _localization.LanguageEnglish.Add("System Memory");

            _localization.LanguageKey.Add("Processor");
            _localization.LanguageEnglish.Add("Processor");

            _localization.LanguageKey.Add("Number Of Processor");
            _localization.LanguageEnglish.Add("Number Of Processor");

            _localization.LanguageKey.Add("Graphics Device Name");
            _localization.LanguageEnglish.Add("Graphics Device Name");

            _localization.LanguageKey.Add("Graphics Device Type");
            _localization.LanguageEnglish.Add("Graphics Device Type");

            _localization.LanguageKey.Add("Graphics Memory");
            _localization.LanguageEnglish.Add("Graphics Memory");

            _localization.LanguageKey.Add("Graphics DeviceID");
            _localization.LanguageEnglish.Add("Graphics DeviceID");

            _localization.LanguageKey.Add("Graphics Device Vendor");
            _localization.LanguageEnglish.Add("Graphics Device Vendor");

            _localization.LanguageKey.Add("Graphics Device Vendor ID");
            _localization.LanguageEnglish.Add("Graphics Device Vendor ID");

            _localization.LanguageKey.Add("Device Model");
            _localization.LanguageEnglish.Add("Device Model");

            _localization.LanguageKey.Add("Device Name");
            _localization.LanguageEnglish.Add("Device Name");

            _localization.LanguageKey.Add("Device Type");
            _localization.LanguageEnglish.Add("Device Type");

            _localization.LanguageKey.Add("Device Unique Identifier");
            _localization.LanguageEnglish.Add("Device Unique Identifier");

            _localization.LanguageKey.Add("Screen Information");
            _localization.LanguageEnglish.Add("Screen Information");

            _localization.LanguageKey.Add("DPI");
            _localization.LanguageEnglish.Add("DPI");

            _localization.LanguageKey.Add("Resolution");
            _localization.LanguageEnglish.Add("Resolution");

            _localization.LanguageKey.Add("Device Resolution");
            _localization.LanguageEnglish.Add("Device Resolution");

            _localization.LanguageKey.Add("Device Sleep");
            _localization.LanguageEnglish.Add("Device Sleep");

            _localization.LanguageKey.Add("Never Sleep");
            _localization.LanguageEnglish.Add("Never Sleep");

            _localization.LanguageKey.Add("System Setting");
            _localization.LanguageEnglish.Add("System Setting");

            _localization.LanguageKey.Add("Screen Capture");
            _localization.LanguageEnglish.Add("Screen Capture");

            _localization.LanguageKey.Add("Full Screen");
            _localization.LanguageEnglish.Add("Full Screen");

            _localization.LanguageKey.Add("Quality Information");
            _localization.LanguageEnglish.Add("Quality Information");

            _localization.LanguageKey.Add("Graphics Quality");
            _localization.LanguageEnglish.Add("Graphics Quality");

            _localization.LanguageKey.Add("Lower");
            _localization.LanguageEnglish.Add("Lower");

            _localization.LanguageKey.Add("Upgrade");
            _localization.LanguageEnglish.Add("Upgrade");

            _localization.LanguageKey.Add("Time Information");
            _localization.LanguageEnglish.Add("Time Information");

            _localization.LanguageKey.Add("Current Time");
            _localization.LanguageEnglish.Add("Current Time");

            _localization.LanguageKey.Add("Elapse Time");
            _localization.LanguageEnglish.Add("Elapse Time");

            _localization.LanguageKey.Add("Time Scale");
            _localization.LanguageEnglish.Add("Time Scale");

            _localization.LanguageKey.Add("Multiple");
            _localization.LanguageEnglish.Add("Multiple");

            _localization.LanguageKey.Add("Environment Information");
            _localization.LanguageEnglish.Add("Environment Information");

            _localization.LanguageKey.Add("Product Name");
            _localization.LanguageEnglish.Add("Product Name");

            _localization.LanguageKey.Add("Product Identifier");
            _localization.LanguageEnglish.Add("Product Identifier");

            _localization.LanguageKey.Add("Product Version");
            _localization.LanguageEnglish.Add("Product Version");

            _localization.LanguageKey.Add("Product DataPath");
            _localization.LanguageEnglish.Add("Product DataPath");

            _localization.LanguageKey.Add("Company Name");
            _localization.LanguageEnglish.Add("Company Name");

            _localization.LanguageKey.Add("Unity Version");
            _localization.LanguageEnglish.Add("Unity Version");

            _localization.LanguageKey.Add("Has Pro License");
            _localization.LanguageEnglish.Add("Has Pro License");

            _localization.LanguageKey.Add("Internet State");
            _localization.LanguageEnglish.Add("Internet State");

            _localization.LanguageKey.Add("NotReachable");
            _localization.LanguageEnglish.Add("NotReachable");

            _localization.LanguageKey.Add("ReachableViaLocalAreaNetwork");
            _localization.LanguageEnglish.Add("ReachableViaLocalAreaNetwork");

            _localization.LanguageKey.Add("ReachableViaCarrierDataNetwork");
            _localization.LanguageEnglish.Add("ReachableViaCarrierDataNetwork");

            _localization.LanguageKey.Add("Quit");
            _localization.LanguageEnglish.Add("Quit");
        }
    }
}
