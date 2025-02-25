using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowBonusText : MonoBehaviour
{
    public Text textBonus;
    int coinBonus;
    int diamondBonus;
    private void OnEnable()
    {
        HR_GamePlayHandler.OnGetTimeBonus += OnBonusTimeLeft;
        HR_GamePlayHandler.OnGetCoin += OnGetCoin;
        HR_GamePlayHandler.OnGetDiamond += OnGetDiamond;
    }
    void OnBonusTimeLeft()
    {
        StartCoroutine(DisableBonusText("time"));
    }
    void OnGetCoin()
    {
        StartCoroutine(DisableBonusText("coin"));
    }
    void OnGetDiamond()
    {
        StartCoroutine(DisableBonusText("diamond"));
    }
    IEnumerator DisableBonusText(string itemType)
    {
        switch (itemType)
        {
            case "coin":
                coinBonus += 1;
                textBonus.text = "+" + coinBonus + " Coin";
                yield return new WaitForSeconds(1f);
                textBonus.text = "";
                break;
            case "diamond":
                diamondBonus += 1;
                textBonus.text = "+" + diamondBonus + " Diamond";
                yield return new WaitForSeconds(1f);
                textBonus.text = "";
                break;
            case "time":
                textBonus.text = "+15 Time";
                yield return new WaitForSeconds(1f);
                textBonus.text = "";
                break;
        }
        yield return new WaitForSeconds(5f);
        coinBonus = 0;
        diamondBonus = 0;

    }
    private void OnDisable()
    {
        HR_GamePlayHandler.OnGetTimeBonus -= OnBonusTimeLeft;
        HR_GamePlayHandler.OnGetCoin -= OnGetCoin;
        HR_GamePlayHandler.OnGetDiamond -= OnGetDiamond;
    }
}
