//----------------------------------------------
//           	   Highway Racer
//
// Copyright © 2014 - 2023 BoneCracker Games
// http://www.bonecrackergames.com
//
//----------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Slides the UI when enabled.
/// </summary>
[AddComponentMenu("BoneCracker Games/Highway Racer/UI/HR UI Button Slide Animation")]
public class HR_UIButtonSlideAnimation : MonoBehaviour {

    public SlideFrom slideFrom;     //  Which direction from slide?
    public enum SlideFrom { Left, Right, Top, Buttom }

    public bool actWhenEnabled = false;     //  Slide when enabled.
    public bool playSound = true;       //  Play audio when sliding.

    private RectTransform rectTransform;      //  Original position of the UI.
    private Vector2 originalPosition = new Vector2(0f, 0f);
    public bool actNow = false;     //  Acting now?
    public bool endedAnimation = false;     //  Ended now?
    public HR_UIButtonSlideAnimation playWhenThisEnds;        //  Trigger this animation on end.

    private AudioSource slidingAudioSource;     //  Audio source.

    private void Awake() {

        //  Getting original position of the UI.
        rectTransform = GetComponent<RectTransform>();
        originalPosition = rectTransform.anchoredPosition;

        //  Setting offset.
        SetOffset();

    }

    private void OnEnable() {

        //  Setting offset and moving back to the original position.
        if (actWhenEnabled) {

            endedAnimation = false;

            SetOffset();
            Animate();

        }

    }

    /// <summary>
    /// Setting offset of the UI first.
    /// </summary>
    private void SetOffset() {

        switch (slideFrom) {

            case SlideFrom.Left:
                rectTransform.anchoredPosition = new Vector2(-1000f, originalPosition.y);
                break;
            case SlideFrom.Right:
                rectTransform.anchoredPosition = new Vector2(1000f, originalPosition.y);
                break;
            case SlideFrom.Top:
                rectTransform.anchoredPosition = new Vector2(originalPosition.x, 1000f);
                break;
            case SlideFrom.Buttom:
                rectTransform.anchoredPosition = new Vector2(originalPosition.x, -1000f);
                break;

        }

    }

    /// <summary>
    /// Sliding animation.
    /// </summary>
    public void Animate() {

        //  Finding audiosource and playing audioclip. Otherwise create a new audio source.
        if (GameObject.Find(HR_HighwayRacerProperties.Instance.labelSlideAudioClip.name))
            slidingAudioSource = GameObject.Find(HR_HighwayRacerProperties.Instance.labelSlideAudioClip.name).GetComponent<AudioSource>();
        else
            slidingAudioSource = HR_CreateAudioSource.NewAudioSource(Camera.main.gameObject, HR_HighwayRacerProperties.Instance.labelSlideAudioClip.name, 0f, 0f, 1f, HR_HighwayRacerProperties.Instance.labelSlideAudioClip, false, false, true);

        //  Make sure audio source is not affected by pause and volume.
        slidingAudioSource.ignoreListenerPause = true;

        actNow = true;

    }
    public void ResetValueAnimation()
    {
        actNow = false;
        endedAnimation = false;
    }
    private void Update() {

        if (!actNow || endedAnimation)
            return;

        if (playWhenThisEnds != null && !playWhenThisEnds.endedAnimation)
            return;

        if (slidingAudioSource && !slidingAudioSource.isPlaying && playSound)
            slidingAudioSource.Play();

        rectTransform.anchoredPosition = Vector2.MoveTowards(rectTransform.anchoredPosition, originalPosition, Time.unscaledDeltaTime * 5000f);

        if (Vector2.Distance(rectTransform.anchoredPosition, originalPosition) < .05f) {

            if (slidingAudioSource && slidingAudioSource.isPlaying && playSound)
                slidingAudioSource.Stop();

            rectTransform.anchoredPosition = originalPosition;

            HR_UICountAnimation countAnimation = GetComponentInChildren<HR_UICountAnimation>();

            if (countAnimation) {

                if (!countAnimation.actNow)
                    countAnimation.Count();

            } else {

                endedAnimation = true;

            }

        }

        if (endedAnimation && !actWhenEnabled)
            enabled = false;

    }

    public void OnDisable() {

        if (slidingAudioSource)
            slidingAudioSource.Stop();

    }

    public void OnDrestroy() {

        if (slidingAudioSource)
            slidingAudioSource.Stop();

    }

}
