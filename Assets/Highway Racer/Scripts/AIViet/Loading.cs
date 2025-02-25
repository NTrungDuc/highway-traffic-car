using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Loading : MonoBehaviour
{
    public Slider loading;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        loading.value += 0.07f;
        loading.value = Mathf.Clamp(loading.value, loading.minValue, loading.maxValue);
        //Debug.Log("Slider Value: " + loading.value);
        if (loading.value == loading.maxValue )
            SceneManager.LoadScene(PlayerPrefs.GetString("nameScene"));
    }
}
