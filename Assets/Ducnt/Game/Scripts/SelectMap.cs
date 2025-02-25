using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectMap : MonoBehaviour
{
    [SerializeField] private List<Button> btnSelectMap;
    private void Start()
    {
        foreach (Button btn in btnSelectMap)
        {
            btn.onClick.AddListener(() => OnClickButton(btn));
        }
    }
    void OnClickButton(Button clickedButton)
    {
        foreach (Button btn in btnSelectMap)
        {
            if (btn == clickedButton)
            {
                btn.GetComponent<Image>().color = Color.green;
            }
            else
            {
                btn.GetComponent<Image>().color = Color.white;
            }
        }
    }
}
