#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class HPS_OnBuildProcessor : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        HPS_MainMenuHandler.Instance.storesMenu.SetActive(true);
        HPS_MainMenuHandler.Instance.rouletteButton.SetActive(true);    
        HPS_MainMenuHandler.Instance.modsSelectionMenu.SetActive(true);
        HPS_MainMenuHandler.Instance.sceneSelectionMenu.SetActive(true);
    }

}
#endif
