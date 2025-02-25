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
using TMPro;

/// <summary>
/// UI upgrade button.
/// </summary>
[AddComponentMenu("BoneCracker Games/Highway Racer/UI/HR UI Modification Upgrade")]
public class HR_UIModificationUpgrade : MonoBehaviour {

    public UpgradeClass upgradeClass;
    public enum UpgradeClass { Speed, Engine, Handling, NOS }

    public int upgradePrice = 2500;
    private bool fullyUpgraded = false;

    public Button buyButton;
    public TextMeshProUGUI priceLabel;
    public Image priceImage;

    private void OnEnable() {

        CheckPurchase();

    }

    public void Upgrade() {

        int playerCoins = HR_API.GetCurrencyGold();

        if (!fullyUpgraded)
            Buy();

        StartCoroutine(CheckPurchase2());

    }

    private IEnumerator CheckPurchase2() {

        yield return new WaitForFixedUpdate();
        CheckPurchase();

    }

    private void CheckPurchase() {

        HR_VehicleUpgrade_UpgradeManager dm = FindObjectOfType<HR_VehicleUpgrade_UpgradeManager>();

        if (!dm)
            return;

        switch (upgradeClass) {

            case UpgradeClass.Speed:
                if (dm.speedLevel >= 5)
                    fullyUpgraded = true;
                else
                    fullyUpgraded = false;
                break;
            case UpgradeClass.Engine:
                if (dm.engineLevel >= 5)
                    fullyUpgraded = true;
                else
                    fullyUpgraded = false;
                break;
            case UpgradeClass.Handling:
                if (dm.handlingLevel >= 5)
                    fullyUpgraded = true;
                else
                    fullyUpgraded = false;
                break;
            case UpgradeClass.NOS:
                if (dm.nosLevel >= 5)
                    fullyUpgraded = true;
                else
                    fullyUpgraded = false;
                break;

        }

        if (!fullyUpgraded) {

            if (!priceImage.gameObject.activeSelf)
                priceImage.gameObject.SetActive(true);

            if (priceLabel.text != upgradePrice.ToString())
                priceLabel.text = upgradePrice.ToString();

        } else {

            if (priceImage.gameObject.activeSelf)
                priceImage.gameObject.SetActive(false);

            if (priceLabel.text != "<margin-left=25>MAX")
                priceLabel.text = "<margin-left=25>MAX";

        }


        buyButton.interactable = HPS_API.OwnedVehicle(dm.carDatas.carName);
        buyButton.interactable = buyButton.interactable && !fullyUpgraded ? true : false;

    }

    private void Buy() {

        HR_VehicleUpgrade_UpgradeManager dm = FindObjectOfType<HR_VehicleUpgrade_UpgradeManager>();

        if (!dm)
            return;

        int playerCoins = HR_API.GetCurrencyGold();

        if (playerCoins >= upgradePrice) {

            switch (upgradeClass) {

                case UpgradeClass.Engine:
                    if (dm.engineLevel < 5) {
                        dm.UpgradeEngine();
                        HR_API.ConsumeCurrencyGold(upgradePrice);
                    }
                    break;
                case UpgradeClass.Handling:
                    if (dm.handlingLevel < 5) {
                        dm.UpgradeHandling();
                        HR_API.ConsumeCurrencyGold(upgradePrice);
                    }
                    break;
                case UpgradeClass.NOS:
                    if (dm.nosLevel < 5) {
                        dm.UpgradeNOS();
                        HR_API.ConsumeCurrencyGold(upgradePrice);
                    }
                    break;
                case UpgradeClass.Speed:
                    if (dm.speedLevel < 5) {
                        dm.UpgradeSpeed();
                        HR_API.ConsumeCurrencyGold(upgradePrice);
                    }
                    break;

            }

        } else {

            HR_UIInfoDisplayer.Instance.ShowInfo("Not Enough Coins", "You have to earn " + (upgradePrice - HR_API.GetCurrencyGold()).ToString() + " more coins to purchase this upgrade", HR_UIInfoDisplayer.InfoType.NotEnoughMoney);
            return;

        }

    }

}
