//----------------------------------------------
//           	   Highway Racer
//
// Copyright © 2014 - 2023 BoneCracker Games
// http://www.bonecrackergames.com
//
//----------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[AddComponentMenu("BoneCracker Games/Highway Racer/UI/HR UI Graphics Button")]
public class HR_UIGraphicsButton : MonoBehaviour {

    public GraphicsLevel _graphicsLevel;
    public enum GraphicsLevel { Low, Medium, High }

    private Button button;
    private Color defButtonColor;

    void Awake() {

        button = GetComponent<Button>();
        defButtonColor = button.image.color;

    }

    public void OnClick() {

        switch (_graphicsLevel) {

            case GraphicsLevel.Low:
                QualitySettings.SetQualityLevel(0);
                break;
            case GraphicsLevel.Medium:
                QualitySettings.SetQualityLevel(1);
                break;
            case GraphicsLevel.High:
                QualitySettings.SetQualityLevel(2);
                break;

        }

    }

    private void Update() {

        button.image.color = defButtonColor;
        Color activeColor = new Color(.667f, 1f, 0f);

        if (QualitySettings.GetQualityLevel() == 0 && _graphicsLevel == GraphicsLevel.Low)
            button.image.color = activeColor;

        if (QualitySettings.GetQualityLevel() == 1 && _graphicsLevel == GraphicsLevel.Medium)
            button.image.color = activeColor;

        if (QualitySettings.GetQualityLevel() == 2 && _graphicsLevel == GraphicsLevel.High)
            button.image.color = activeColor;

    }

}
