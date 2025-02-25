using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditName : MonoBehaviour
{
    public Text namePlayer;
    private TouchScreenKeyboard keyboard;

    private void Start()
    {
        namePlayer.text = PlayerPrefs.GetString("PlayerName", "HYPERSOL");
    }
    void Update()
    {
        //if (keyboard != null)
        //{
        //    if (keyboard.status == TouchScreenKeyboard.Status.Visible)
        //    {
        //        namePlayer.text = keyboard.text;
        //    }
        //    if (keyboard.status == TouchScreenKeyboard.Status.Done)
        //    {
        //        PlayerPrefs.SetString("PlayerName", namePlayer.text);
        //        keyboard = null;
        //    }
        //}
        if (keyboard != null && !TouchScreenKeyboard.visible)
        {
            SavePlayerName();
            keyboard = null;
        }
    }

    public void OnTextClick()
    {
        keyboard = TouchScreenKeyboard.Open(namePlayer.text, TouchScreenKeyboardType.Default);
        StartCoroutine(KeyboardMonitor());
    }
    private IEnumerator KeyboardMonitor()
    {
        while (keyboard != null && keyboard.status == TouchScreenKeyboard.Status.Visible)
        {
            namePlayer.text = keyboard.text;
            yield return null;
        }

        if (keyboard != null &&
            (keyboard.status == TouchScreenKeyboard.Status.Done ||
             keyboard.status == TouchScreenKeyboard.Status.Canceled))
        {
            SavePlayerName();
        }

        keyboard = null;
    }
    private void SavePlayerName()
    {
        if (!string.IsNullOrEmpty(namePlayer.text))
        {
            PlayerPrefs.SetString("PlayerName", namePlayer.text);
            Debug.Log($"Player name saved: {namePlayer.text}");
        }
    }
}
