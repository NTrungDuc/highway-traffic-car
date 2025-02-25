using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class SelectFlag : MonoBehaviour
{
    public Image FlagPlayer;
    private Image imageFlag;
    string path = "Flag";
    private void Start()
    {
        imageFlag = GetComponent<Image>();
    }
    public void ChooseFlag()
    {
        PlayerPrefs.SetString("countryCode", imageFlag.sprite.name);
        FlagPlayer.sprite = Resources.Load<Sprite>($"{path}/{PlayerPrefs.GetString("countryCode")}");
    }
}
