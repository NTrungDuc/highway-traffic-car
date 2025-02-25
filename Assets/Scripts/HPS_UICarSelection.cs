using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HPS_UICarSelection : MonoBehaviour
{
    public int carIndex = 0;

    void OnEnable()
    {
        Invoke("CheckSelection", 0f);
    }

    private void CheckSelection()
    {
        if (PlayerPrefs.HasKey("SelectedPlayerCarIndex") && PlayerPrefs.GetInt("SelectedPlayerCarIndex") == carIndex)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(gameObject);
        }
    }

}
