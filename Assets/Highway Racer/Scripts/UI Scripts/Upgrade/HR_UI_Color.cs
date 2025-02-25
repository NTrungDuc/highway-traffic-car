//----------------------------------------------
//           	   Highway Racer
//
// Copyright © 2014 - 2023 BoneCracker Games
// http://www.bonecrackergames.com
//
//----------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[AddComponentMenu("BoneCracker Games/Highway Racer/UI/HR UI Modification Color")]
public class HR_UI_Color : MonoBehaviour {

    public Color color = Color.white;
    public int price = 5000;

    public bool purchased = false;
    public Image mask;
    public GameObject buyButton;
    public Text priceText;

    public AudioClip purchaseSound;

    private Button button;

    private void OnEnable() {

        if (!button)
            button = GetComponent<Button>();

        CheckPurchase();

    }

    public void CheckPurchase() {

        HR_VehicleUpgrade_PaintManager pm = FindObjectOfType<HR_VehicleUpgrade_PaintManager>();
        HR_VehicleUpgrade_SpoilerManager sm = FindObjectOfType<HR_VehicleUpgrade_SpoilerManager>();

        if (!pm || !sm)
            return;

        if (PlayerPrefs.HasKey(pm.transform.root.name + "BodyColor" + color.ToString()))
            purchased = true;
        else
            purchased = false;

        if (purchased) {

            if (buyButton)
                buyButton.SetActive(false);

            if (HPS_MainMenuHandler.Instance.buyCarButton)
                HPS_MainMenuHandler.Instance.buyCarButton.SetActive(false);

            if (priceText)
                priceText.text = "";

            if (RCC_PlayerPrefsX.GetColor(pm.transform.root.name + "BodyColor", HR_HighwayRacerProperties.Instance._defaultBodyColor) == color)
            {
                pm.Paint(color);
                sm.Paint(color);
                mask.color = Color.white;
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(gameObject);
                StartCoroutine(WaitUntilUnselected());
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => Upgrade());

        } else {

            if (buyButton)
            {
                buyButton.SetActive(true);

                //if (EventSystem.current.currentSelectedGameObject == gameObject && HPS_API.OwnedVehicle(dm.transform.root.name.Replace("(Clone)", "")))
                {
                    HPS_MainMenuHandler.Instance.buyCarButton.SetActive(true);
                    HPS_MainMenuHandler.Instance.buyCarButton.GetComponentInChildren<TextMeshProUGUI>().text = $"{price}";
                    HPS_MainMenuHandler.Instance.buyCarButton.transform.GetChild(1).gameObject.SetActive(true);
                    HPS_MainMenuHandler.Instance.buyCarButton.GetComponent<Button>().onClick.RemoveAllListeners();
                    HPS_MainMenuHandler.Instance.buyCarButton.GetComponent<Button>().onClick.AddListener(() => Buy());
                }
            }

            if (priceText)
                priceText.text = price.ToString();

            if (EventSystem.current.currentSelectedGameObject == gameObject)
            {
                mask.color = Color.white;
                pm.Paint(color, false);
                sm.Paint(color);
                StartCoroutine(WaitUntilUnselected());
            }

        }

    }

    private void OnDisable() {

        HR_VehicleUpgrade_PaintManager pm = FindObjectOfType<HR_VehicleUpgrade_PaintManager>();
        HR_VehicleUpgrade_SpoilerManager sm = FindObjectOfType<HR_VehicleUpgrade_SpoilerManager>();

        if (!pm || !sm)
                return;

        mask.color = new Color(1, 1, 1, 0);

        pm.Paint(RCC_PlayerPrefsX.GetColor(pm.transform.root.name + "BodyColor"), false);
        sm.Paint(RCC_PlayerPrefsX.GetColor(pm.transform.root.name + "BodyColor"));

        StopAllCoroutines();
    }

    IEnumerator WaitUntilUnselected()
    {
        yield return new WaitUntil(() => EventSystem.current.currentSelectedGameObject != gameObject);
        mask.color = new Color(1, 1, 1, 0);
        yield return null;
    }

    public void Upgrade() {

        HR_VehicleUpgrade_PaintManager pm = FindObjectOfType<HR_VehicleUpgrade_PaintManager>();
        HR_VehicleUpgrade_SpoilerManager sm = FindObjectOfType<HR_VehicleUpgrade_SpoilerManager>();

        if (!pm || !sm)
            return;

        pm.Paint(color);
        sm.Paint(color);

        CheckPurchase();

    }

    public void Buy() {

        if (HR_API.GetCurrencyGold() >= price) {

            HR_API.ConsumeCurrencyGold(price);
            Upgrade();

            if (purchaseSound)
                RCC_Core.NewAudioSource(gameObject, purchaseSound.name, 0f, 0f, 1f, purchaseSound, false, true, true);

        } else {

            HR_UIInfoDisplayer.Instance.ShowInfo("Not Enough Coins", "You have to earn " + (price - HR_API.GetCurrencyGold()).ToString() + " more coins to purchase this paint", HR_UIInfoDisplayer.InfoType.NotEnoughMoney);
            return;

        }

    }

}
