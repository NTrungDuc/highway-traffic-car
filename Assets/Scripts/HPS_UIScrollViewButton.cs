using PaintCore;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HPS_UIScrollViewButton : MonoBehaviour
{
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;
    [SerializeField] private HPS_UINoDragScrollRect scrollView;

    float slideTimer = 0f;
    float slideTime = 0.2f;
    private bool isLeft;
    private bool isPreviousLeft;
    private bool ignoreSoundEffect = true;
    private AudioSource clickSound;

    void Start()
    {
        switch (transform.parent.name)
        {
            case "Quality":
                scrollView.horizontalScrollbar.value = 0.5f;

                switch (Mathf.Round((((float)PlayerPrefs.GetInt("QualityLevel", 0)) / (scrollView.content.childCount - 1)) * 10f) * 0.1f)
                {
                    case 0:
                        rightButton.onClick.Invoke();
                        break;
                    case 0.5f:
                        HR_UIOptionsManager.Instance.SetQuality("Medium");
                        break;
                    case 1:
                        leftButton.onClick.Invoke();
                        break;
                }
                break;
            case "Control":
                scrollView.horizontalScrollbar.value = 0;

                switch ((float)PlayerPrefs.GetInt("ControllerType", 0))
                {
                    case 0:
                        HR_UIOptionsManager.Instance.SetControllerType("Touchscreen");
                        break;
                    case 1:
                        rightButton.onClick.Invoke();
                        break;
                }
                break;
        }

        ignoreSoundEffect = false;
    }

    public void Slide(bool isLeft)
    {
        isPreviousLeft = this.isLeft;
        this.isLeft = isLeft;

        if (!ignoreSoundEffect)
        {
            clickSound = HR_CreateAudioSource.NewAudioSource(Camera.main.gameObject, HR_HighwayRacerProperties.Instance.buttonClickAudioClip.name, 0f, 0f, 1f, HR_HighwayRacerProperties.Instance.buttonClickAudioClip, false, true, true);
            clickSound.ignoreListenerPause = true;
        }

        if (scrollView.content.childCount < 3)
            StartCoroutine(AnimateSlide(true));
        else
            StartCoroutine(AnimateSlide());
    }

    private IEnumerator AnimateSlide(bool isLowerThan3 = false)
    {
        float targetValue = isLowerThan3 ? isLeft ? 1 : 0 : isLeft ? Mathf.Round((scrollView.horizontalScrollbar.value + 0.5f) * 10f) * 0.1f : Mathf.Round((scrollView.horizontalScrollbar.value - 0.5f) * 10f) * 0.1f;

        if (!isLowerThan3)
        {
            leftButton.interactable = rightButton.interactable = false;

            while (slideTimer < slideTime)
            {
                slideTimer += Time.fixedDeltaTime;
                scrollView.horizontalScrollbar.value = Mathf.Lerp(0.5f, targetValue, slideTimer / slideTime);
                yield return null;
            }

            Shuffle(isLowerThan3, targetValue);
            scrollView.horizontalScrollbar.value = 0.5f;
            scrollView.enabled = false;

            float t = 0;

            while (t < 0.1f)
            {
                t += Time.fixedDeltaTime;
                yield return null;
            }
            t = 0;
            scrollView.enabled = true;
            while (t < 0.15f)
            {
                t += Time.fixedDeltaTime;
                yield return null;
            }
            rightButton.interactable = leftButton.interactable = true;
        }
        else
        {
            leftButton.interactable = rightButton.interactable = false;

            Shuffle(isLowerThan3, targetValue);

            while (slideTimer < slideTime)
            {
                slideTimer += Time.fixedDeltaTime;
                scrollView.horizontalScrollbar.value = Mathf.Lerp(targetValue + (isLeft ? -1 : 1), targetValue, slideTimer / slideTime);
                yield return null;
            }

            scrollView.horizontalScrollbar.value = targetValue;

            float t = 0;
            while (t < 0.25f)
            {
                t += Time.fixedDeltaTime;
                yield return null;
            }
            leftButton.interactable = rightButton.interactable = true;
        }

        slideTimer = 0;

        switch (transform.parent.name)
        {
            case "Quality":
                HR_UIOptionsManager.Instance.SetQuality(scrollView.content.GetChild((int)(scrollView.horizontalScrollbar.value * (scrollView.content.childCount - 1))).name);
                break;
            case "Control":
                HR_UIOptionsManager.Instance.SetControllerType(scrollView.content.GetChild((int)scrollView.horizontalScrollbar.value).name);
                break;
        }
    }

    private void Shuffle(bool isLowerThan3, float targetValue)
    {
        if (!isLowerThan3)
        {
            if (isLeft)
                scrollView.content.GetChild(0).SetAsLastSibling();
            else
                scrollView.content.GetChild(scrollView.content.childCount - 1).SetAsFirstSibling();
        }
        else
        {
            switch (scrollView.horizontalScrollbar.value)
            {
                case 0:
                    if (isPreviousLeft == isLeft)
                    {
                        scrollView.content.GetChild(1).SetAsFirstSibling();
                        if (scrollView.horizontalScrollbar.value != targetValue)
                            scrollView.horizontalScrollbar.value = 1;
                    }
                    break;
                case 1:
                    if (isPreviousLeft == isLeft)
                    {
                        scrollView.content.GetChild(0).SetAsLastSibling();
                        if (scrollView.horizontalScrollbar.value != targetValue)
                            scrollView.horizontalScrollbar.value = 0;
                    }
                    break;
            }
        }
    }

}