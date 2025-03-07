﻿//----------------------------------------------
//           	   Highway Racer
//
// Copyright © 2014 - 2023 BoneCracker Games
// http://www.bonecrackergames.com
//
//----------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("BoneCracker Games/Highway Racer/UI/HR UI Modification Siren")]
public class HR_UI_Siren : MonoBehaviour {

    public int index = 0;
    public int price = 5000;

    public bool purchased = false;
    public GameObject buyButton;
    public Text priceText;

    public AudioClip purchaseSound;

    private void OnEnable() {

        CheckPurchase();

    }

    public void CheckPurchase() {

        HR_VehicleUpgrade_SirenManager dm = FindObjectOfType<HR_VehicleUpgrade_SirenManager>();

        if (!dm)
            return;

        if (PlayerPrefs.HasKey(dm.transform.root.name + "SelectedSiren")) {

            if (PlayerPrefs.GetInt(dm.transform.root.name + "SelectedSiren", -1) == index)
                purchased = true;

        }

        if (index == -1)
            purchased = true;

        if (purchased) {

            if (buyButton)
                buyButton.SetActive(false);

            if (priceText)
                priceText.text = "";

        } else {

            if (buyButton)
                buyButton.SetActive(true);

            if (priceText)
                priceText.text = price.ToString();

        }

    }

    public void Upgrade() {

        HR_VehicleUpgrade_SirenManager dm = FindObjectOfType<HR_VehicleUpgrade_SirenManager>();

        if (!dm)
            return;

        dm.Upgrade(index);

        CheckPurchase();

    }

    public void Buy() {

        if (HR_API.GetCurrencyGold() >= price) {

            HR_API.ConsumeCurrencyGold(price);
            Upgrade();

            if (purchaseSound)
                RCC_Core.NewAudioSource(gameObject, purchaseSound.name, 0f, 0f, 1f, purchaseSound, false, true, true);

        } else {

            HR_UIInfoDisplayer.Instance.ShowInfo("Not Enough Coins", "You have to earn " + (price - HR_API.GetCurrencyGold()).ToString() + " more coins to purchase this siren", HR_UIInfoDisplayer.InfoType.NotEnoughMoney);
            return;

        }

    }

}
