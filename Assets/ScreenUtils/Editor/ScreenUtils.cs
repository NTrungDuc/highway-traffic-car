using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ScreenUtils
{
    [MenuItem("SS/Capture")]
    public static void Capture()
    {
        string dirPath = Application.dataPath.Substring(0, Application.dataPath.Length - 6) + "Screenshots";
        if (!System.IO.Directory.Exists(dirPath))
        {
            System.IO.Directory.CreateDirectory(dirPath);
        }

        string filePath = string.Format("Screenshots/{0}x{1}-{2}.png", Screen.width, Screen.height, System.DateTime.Now.ToString("MM-dd-yyyy-hh-mm-ss-fff-tt"));
        ScreenCapture.CaptureScreenshot(filePath);

        Debug.Log("Captured: " + filePath);
    }
}