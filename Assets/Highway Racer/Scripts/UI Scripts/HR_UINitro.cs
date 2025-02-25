using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[AddComponentMenu("BoneCracker Games/Highway Racer/UI/HR UI Nitro")]
public class HR_UINitro : MonoBehaviour {

    [SerializeField] private Image fill;
    [SerializeField] private Animator animator;
    [SerializeField] private Button nitroButton;
    [SerializeField] private GameObject getNitro;
    [SerializeField] private Text durationCountdown;
    [SerializeField] private TextMeshProUGUI nitroCount;
    [SerializeField] private RuntimeAnimatorController buttonScale;
    [SerializeField] private RuntimeAnimatorController buttonScaling;

    public float cooldownTime = 15f;

    float t;
    float tMax;

    private void OnEnable()
    {

        if (!RCC_Settings.Instance.mobileControllerEnabled)
        {

            gameObject.SetActive(false);
            return;

        }

        nitroButton.onClick.RemoveAllListeners();

        if (PlayerPrefs.GetInt("NitroCount", 0) <= 0)
        {
            getNitro.SetActive(true);
            nitroButton.interactable = true;
            nitroCount.transform.parent.gameObject.SetActive(false);
            nitroButton.onClick.AddListener(() => NitroAds());
            animator.runtimeAnimatorController = buttonScaling;
        }
        else
        {
            getNitro.SetActive(false);
            nitroCount.transform.parent.gameObject.SetActive(true);
            nitroCount.text = PlayerPrefs.GetInt("NitroCount", 0).ToString();
            nitroButton.onClick.AddListener(() => Nitro());
            animator.runtimeAnimatorController = buttonScale;
        }

        StartCoroutine(Waiter());

    }

    private IEnumerator Waiter()
    {
        durationCountdown.text = "";
        nitroButton.interactable = false;
        yield return new WaitForSeconds(5);
        nitroButton.interactable = true;
        yield return null;
    }

    public void Nitro()
    {
        t = tMax = RCC_SceneManager.Instance.activePlayerVehicle.NitroTime + 1;

        if (RCC_SceneManager.Instance.activePlayerVehicle.canNitro && !RCC_SceneManager.Instance.activePlayerVehicle.useNitro)
        {
            int nitroCount = PlayerPrefs.GetInt("NitroCount", 0);
            if (nitroCount > 0)
            {
                StartCoroutine(NitroDuration());
                RCC_SceneManager.Instance.activePlayerVehicle.RCC_InputManager_OnNitro(cooldownTime);
            }
            nitroCount = nitroCount-- <= 0 ? 0 : nitroCount--;
            PlayerPrefs.SetInt("NitroCount", nitroCount);
            if (nitroCount > 0)
            {
                this.nitroCount.text = nitroCount.ToString();
                animator.runtimeAnimatorController = buttonScale;
            }
            else
            {
                getNitro.SetActive(true);
                this.nitroCount.transform.parent.gameObject.SetActive(false);
            }
        }
    }

    private IEnumerator NitroDuration()
    {
        nitroButton.interactable = false;
        while (t > tMax - 1)
        {
            t -= Time.deltaTime;
            fill.fillAmount = t / tMax;
            yield return null;
        }
        
        while (t > 0)
        {
            t -= Time.deltaTime;
            durationCountdown.text = t.ToString("N1");
            fill.fillAmount = t / tMax;
            yield return null;
        }

        t = 0;
        durationCountdown.text = "";

        if (PlayerPrefs.GetInt("NitroCount", 0) <= 0)
        {
            fill.fillAmount = 1;
            nitroButton.interactable = true;
            nitroButton.onClick.RemoveAllListeners();
            nitroButton.onClick.AddListener(() => NitroAds());
            animator.runtimeAnimatorController = buttonScaling;
            yield break;
        }

        while (t < cooldownTime)
        {
            t += Time.deltaTime;
            fill.fillAmount = t / cooldownTime;
            yield return null;
        }
        nitroButton.interactable = true;
    }

    private void NitroAds()
    {
        int nitroCount = PlayerPrefs.GetInt("NitroCount", 0);
        nitroCount += 2;
        PlayerPrefs.SetInt("NitroCount", nitroCount);
        this.nitroCount.text = nitroCount.ToString();
        getNitro.SetActive(false);
        this.nitroCount.transform.parent.gameObject.SetActive(true);
        this.nitroCount.text = nitroCount.ToString();
        nitroButton.onClick.RemoveAllListeners();
        nitroButton.onClick.AddListener(() => Nitro());
        animator.runtimeAnimatorController = buttonScale;
    }
}
