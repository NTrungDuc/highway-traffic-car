using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ProgressMissionMode : MonoBehaviour
{
    private static ProgressMissionMode instance;
    public static ProgressMissionMode Instance { get { return instance; } }
    public Image fillProgressMissionMode;
    public GameObject content;
    public Text level;
    private void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(enableProgressBar(4));
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void ProgressBar(float currentDistance, float maxDistance)
    {
        float progress = currentDistance / maxDistance;
        fillProgressMissionMode.fillAmount = progress;
    }
    IEnumerator enableProgressBar(int time)
    {
        yield return new WaitForSeconds(time);
        if (HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.MissionMode_1 || HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.MissionMode_2)
        {
            content.SetActive(true);
            level.text = "LEVEL: " + PlayerPrefs.GetInt("levelMissionMode", 1).ToString();
        }
    }
    public void disableProgressBar()
    {
        content.SetActive(false);
    }

}
