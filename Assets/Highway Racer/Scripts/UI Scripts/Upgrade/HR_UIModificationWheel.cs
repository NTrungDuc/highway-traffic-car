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
/// UI change wheel button.
/// </summary>
[AddComponentMenu("BoneCracker Games/Highway Racer/UI/HR UI Modification Wheel")]
public class HR_UIModificationWheel : MonoBehaviour {

    public int wheelIndex = 0;

    public int price = 5000;
    public bool purchased = false;
    public GameObject buyButton;
    public Text priceText;

    public AudioClip purchaseSound;

    private void OnEnable() {

        CheckPurchase();

    }

    public void CheckPurchase() {

        HR_VehicleUpgrade_WheelManager dm = FindObjectOfType<HR_VehicleUpgrade_WheelManager>();

        if (!dm)
            return;

        if (PlayerPrefs.HasKey(dm.transform.root.name + "Wheel" + wheelIndex.ToString()))
            purchased = true;
        else
            purchased = false;

        if (purchased) {

            if (buyButton)
                buyButton.SetActive(false);

            if (priceText)
                priceText.text = "";

        } else {

            if (buyButton)
                buyButton.SetActive(true);

            if (priceText)
                priceText.text = HR_Wheels.Instance.wheels[wheelIndex].price.ToString();

        }

    }

    public void Upgrade() {

        HR_VehicleUpgrade_WheelManager dm = FindObjectOfType<HR_VehicleUpgrade_WheelManager>();

        if (!dm)
            return;

        dm.UpdateWheel(wheelIndex);

        CheckPurchase();

    }

    public void Buy() {

        if (HR_API.GetCurrencyGold() >= price) {

            HR_API.ConsumeCurrencyGold(price);
            Upgrade();

            if (purchaseSound)
                RCC_Core.NewAudioSource(gameObject, purchaseSound.name, 0f, 0f, 1f, purchaseSound, false, true, true);

        } else {

            HR_UIInfoDisplayer.Instance.ShowInfo("Not Enough Coins", "You have to earn " + (price - HR_API.GetCurrencyGold()).ToString() + " more coins to purchase this wheel", HR_UIInfoDisplayer.InfoType.NotEnoughMoney);
            return;

        }

    }

}
