using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LockMode : MonoBehaviour
{
    [SerializeField] private GameObject[] listLockMode;
    [SerializeField] private Button[] buttonsMode;
    [SerializeField] private GameObject[] ButtonGo;

    public static bool isEnable { get; private set; }

    private void OnEnable()
    {
        if (PlayerPrefs.GetFloat("CurrentRankPoint", 0) >= 1200)
        {
            //mo khoa dual mode
            listLockMode[0].SetActive(false);
        }
        if (PlayerPrefs.GetFloat("CurrentRankPoint", 0) >= 1600)
        {
            //mo khoa endless mode
            listLockMode[1].SetActive(false);
        }
        if (PlayerPrefs.GetFloat("CurrentRankPoint", 0) >= 3600)
        {
            //mo khoa challenge va escape mode
        }
        for (int i = 0; i < listLockMode.Length; i++)
        {
            if (listLockMode[i].transform.parent.gameObject.activeSelf)
            {
                activeButtonGo(i);
            }
        }
    }

    public void activeButtonGo(int indexButton)
    {
        ButtonGo[indexButton].SetActive(!listLockMode[indexButton].activeSelf);
    }
}
