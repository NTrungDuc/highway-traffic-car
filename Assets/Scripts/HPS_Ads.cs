using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HPS_Ads : MonoBehaviour
{
    #region SINGLETON PATTERN
    private static HPS_Ads instance;
    public static HPS_Ads Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<HPS_Ads>();
            return instance;
        }
    }
    #endregion

    public GameObject leftBanner;
    public GameObject bottomBanner;

    void Start()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
    }

    private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        leftBanner.SetActive(false);
        //bottomBanner.SetActive(true);
    }

}
