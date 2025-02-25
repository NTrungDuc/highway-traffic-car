using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnlockBox : MonoBehaviour
{
    [SerializeField] internal Image fillBox;
    [SerializeField] private Image BGBox;
    [SerializeField] private Image showReward;
    public ListReward[] listRewards;
    private int indexReward = 0;
    [SerializeField] private Text ProgressText;
    public float fillSpeed = 3f;
    public float targetFillAmount = 0.1f;
    //button
    [SerializeField] public GameObject BtnClaim;
    [SerializeField] public GameObject BtnNoThanks;

    private void OnEnable()
    {
        fillBox.fillAmount = PlayerPrefs.GetFloat("ProgressBox");
        ProgressText.text = (fillBox.fillAmount * 100).ToString() + "%";
        StartCoroutine(IncreaseFillAmount());
    }

    IEnumerator IncreaseFillAmount()
    {
        float currentProgress = PlayerPrefs.GetFloat("ProgressBox");
        float newProgress = currentProgress + targetFillAmount;
        if (newProgress > 1)
        {
            BGBox.enabled = true;
            fillBox.enabled = true;
            showReward.enabled = false;
            fillBox.fillAmount = 0;
            newProgress = 0.1f;
        }
        while (fillBox.fillAmount < newProgress)
        {
            fillBox.fillAmount += fillSpeed * Time.deltaTime;
            fillBox.fillAmount = Mathf.Min(fillBox.fillAmount, newProgress);
            ProgressText.text = (Mathf.RoundToInt(fillBox.fillAmount * 100)).ToString() + "%";
            yield return null;
        }
        if (fillBox.fillAmount == 1)
        {
            indexReward = PlayerPrefs.GetInt("IndexReward", 0);
            StartCoroutine(ShowReward(indexReward));
        }
        else
        {
            yield return new WaitForSeconds(2f);
            gameObject.SetActive(false);
        }
        PlayerPrefs.SetFloat("ProgressBox", fillBox.fillAmount);
    }

    private IEnumerator ShowReward(int i)
    {
        //show phan thuong duoc nhan
        yield return new WaitForSeconds(3f);
        ProgressText.enabled = false;
        BGBox.enabled = false;
        fillBox.enabled = false;
        showReward.enabled = true;
        showReward.sprite = listRewards[i].spriteItem;
        BtnClaim.SetActive(true);
        BtnNoThanks.SetActive(true);
    }

    public void ClaimReward()
    {
        if (indexReward < listRewards.Length - 1)
        {
            if(listRewards[indexReward].type == RewardType.CarItem)
                PlayerPrefs.SetInt(listRewards[indexReward].nameItem, 1);
            else if (listRewards[indexReward].type == RewardType.Gold)
                HR_API.AddCurrencyGold(int.Parse(listRewards[indexReward].nameItem));
            else
                //nhan kim cuong
            indexReward += 1;
        }
        else
        {
            indexReward = listRewards.Length - 1;
            HR_API.AddCurrencyGold(int.Parse(listRewards[indexReward].nameItem));
        }
        PlayerPrefs.SetInt("IndexReward", indexReward);
        gameObject.SetActive(false);
        BtnClaim.SetActive(false);
        BtnNoThanks.SetActive(false);
    }

    public void DisablePanel()
    {
        gameObject.SetActive(false);
    }
}
[System.Serializable]
public class ListReward
{
    public RewardType type;
    public string nameItem;
    public Sprite spriteItem;
}
public enum RewardType
{
    CarItem,
    Gold,
    Diamond
}
