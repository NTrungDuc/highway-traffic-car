//----------------------------------------------
//           	   Highway Racer
//
// Copyright © 2014 - 2023 BoneCracker Games
// http://www.bonecrackergames.com
//
//----------------------------------------------

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[AddComponentMenu("BoneCracker Games/Highway Racer/UI/HR UI Modification Plate")]
public class HR_UI_Plate : MonoBehaviour
{
    public int index = 0;
    public int price = 10000;

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

        HR_VehicleUpgrade_PlateManager dm = FindObjectOfType<HR_VehicleUpgrade_PlateManager>();

        if (!dm)
            return;

        if (PlayerPrefs.HasKey(dm.transform.root.name + "Plate" + index.ToString()))
            purchased = true;
        else
            purchased = false;

        if (index == -1)
            purchased = true;

        if (purchased)
        {

            if (buyButton)
                buyButton.SetActive(false);

            if (HPS_MainMenuHandler.Instance.buyCarButton)
                HPS_MainMenuHandler.Instance.buyCarButton.SetActive(false);

            if (priceText)
                priceText.text = "";

            if (PlayerPrefs.GetInt(dm.transform.root.name + "Plate", -1) == index)
            {
                dm.Upgrade(index);
                mask.color = Color.white;
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(gameObject);
                StartCoroutine(WaitUntilUnselected());
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => Upgrade());

        }
        else
        {

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
                dm.Upgrade(index, false);
                StartCoroutine(WaitUntilUnselected());
            }

        }

    }

    private void OnDisable() {

        HR_VehicleUpgrade_PlateManager dm = FindObjectOfType<HR_VehicleUpgrade_PlateManager>();
        if (!dm)
            return;

        mask.color = new Color(1, 1, 1, 0);

        dm.Upgrade(PlayerPrefs.GetInt(dm.transform.root.name + "Plate", -1), false);

        StopAllCoroutines();
    }

    IEnumerator WaitUntilUnselected()
    {
        yield return new WaitUntil(() => EventSystem.current.currentSelectedGameObject != gameObject);
        mask.color = new Color(1, 1, 1, 0);
        yield return null;
    }

    public void Upgrade() {

        HR_VehicleUpgrade_PlateManager dm = FindObjectOfType<HR_VehicleUpgrade_PlateManager>();

        if (!dm)
            return;

        dm.Upgrade(index);

        CheckPurchase();

    }

    public void Buy() {

        if (HR_API.GetCurrencyGold() >= price)
        {

            HR_API.ConsumeCurrencyGold(price);
            Upgrade();

            if (purchaseSound)
                RCC_Core.NewAudioSource(gameObject, purchaseSound.name, 0f, 0f, 1f, purchaseSound, false, true, true);

        }
        else
        {

            HR_UIInfoDisplayer.Instance.ShowInfo("Not Enough Coins", "You have to earn " + (price - HR_API.GetCurrencyGold()).ToString() + " more coins to purchase this plate", HR_UIInfoDisplayer.InfoType.NotEnoughMoney);
            return;

        }

    }

}
