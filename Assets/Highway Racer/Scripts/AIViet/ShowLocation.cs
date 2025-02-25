using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowLocation : MonoBehaviour
{
    private static ShowLocation instance;
    public static ShowLocation Instance { get { return instance; } }
    public GameObject content;
    public Slider sliderPlayer;
    public Slider sliderAI;

    private void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        if(HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.AIRacing)
            StartCoroutine(DelayStart());
    }

    // Update is called once per frame
    void Update()
    {
        if (HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.AIRacing)
        {
            sliderPlayer.value = HR_GamePlayHandler.Instance.player.distance;
            sliderAI.value = HR_GamePlayHandler.Instance.carAIGameHR_AICar.distanceUI;
        }
            
    }
    IEnumerator DelayStart()
    {
        yield return new WaitForSeconds(4);
        content.SetActive(true);
    }
    public void activeProgressDualAI(bool active)
    {
        content.SetActive(active);
    }
}
