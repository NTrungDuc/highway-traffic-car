using System.Collections;
using System.Collections.Generic;
using GoogleMobileAds.Sample;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HPS_UILoading : MonoBehaviour
{
    #region SINGLETON PATTERN
    private static HPS_UILoading instance;
    public static HPS_UILoading Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<HPS_UILoading>();
            return instance;
        }
    }
    #endregion

    public bool isLoading { get; private set; }

    public Slider loadingBar;
    public GameObject loadingScreen;

    void Start()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
    }

    private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        //if (arg0.buildIndex == 0 || arg0.buildIndex == 1)
        {
            isLoading = false;
            loadingBar.value = 0f;
            loadingScreen.SetActive(false);
        }
        //else
        //    StartCoroutine(DelaySceneLoaded());
    }

    private IEnumerator DelaySceneLoaded()
    {
        yield return new WaitForSeconds(2f);
        isLoading = false;
        loadingBar.value = 0f;
        loadingScreen.SetActive(false);
    }

    public IEnumerator LoadScene(int sceneIndex)
    {
        yield return null;

        isLoading = true;

        float time = 0f;
        Time.timeScale = 1;
        loadingScreen.SetActive(true);

        while (loadingBar.value < 0.15f)
        {
            loadingBar.value += Time.deltaTime;
            yield return null;
        }
        yield return new WaitForSeconds(0.5f);

        time = 0f;
        while (loadingBar.value < 0.9f)
        {
            loadingBar.value = Mathf.Lerp(0.15f, 0.9f, time / 0.25f);
            time += Time.deltaTime;
            yield return null;
        }
        yield return new WaitForSeconds(0.25f);

        time = 0f;
        while (loadingBar.value < 1)
        {
            loadingBar.value = Mathf.Lerp(0.9f, 1f, time / 0.25f);
            time += Time.deltaTime;
            yield return null;
        }

        SceneManager.LoadScene(sceneIndex);
    }

    public IEnumerator LoadScene(string sceneName)
    {
        yield return null;

        isLoading = true;

        float time = 0f;
        Time.timeScale = 1;
        loadingScreen.SetActive(true);
        BannerViewController.Instance.HideAd();
        
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        asyncOperation.allowSceneActivation = false;
        while (!asyncOperation.isDone)
        {
            while (loadingBar.value < 0.15f)
            {
                loadingBar.value += Time.deltaTime;
                yield return null;
            }
            yield return new WaitForSeconds(0.5f);

            time = 0f;
            while (loadingBar.value < 0.9f)
            {
                loadingBar.value = Mathf.Lerp(0.15f, 0.9f, time / 0.25f);
                time += Time.deltaTime;
                yield return null;
            }
            yield return new WaitForSeconds(0.25f);

            time = 0f;
            while (loadingBar.value < 1)
            {
                loadingBar.value = Mathf.Lerp(0.9f, 1f, time / 0.25f);
                time += Time.deltaTime;
                yield return null;
            }

            asyncOperation.allowSceneActivation = true;
            yield return null;
        }
    }
}
