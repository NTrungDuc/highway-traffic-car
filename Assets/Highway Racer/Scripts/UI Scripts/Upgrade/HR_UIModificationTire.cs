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
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// UI change tire button.
/// </summary>
[AddComponentMenu("BoneCracker Games/Highway Racer/UI/HR UI Modification Tire")]
public class HR_UIModificationTire : MonoBehaviour {


    public int tireIndex = 0;

    public int price = 750;
    public bool purchased = false;
    public Image mask;
    public GameObject buyButton;

    public AudioClip purchaseSound;

    private Button button;
    private Animator animator;

    private void OnEnable()
    {

        if (!button)
            button = GetComponent<Button>();

        if (!animator)
            animator = GetComponent<Animator>();

        CheckPurchase(true);

    }

    public void CheckPurchase(bool ignoreAnimation) {

        HR_VehicleUpgrade_TireManager dm = FindObjectOfType<HR_VehicleUpgrade_TireManager>();

        if (!dm)
            return;

        if (PlayerPrefs.HasKey(dm.transform.root.name + "Tire" + tireIndex.ToString()))
            purchased = true;
        else
            purchased = false;

        if (tireIndex == 0)
            purchased = true;

        if (purchased) {

            if (buyButton)
                buyButton.SetActive(false);

            if (HPS_MainMenuHandler.Instance.buyCarButton)
                HPS_MainMenuHandler.Instance.buyCarButton.SetActive(false);

            if (PlayerPrefs.GetInt(dm.transform.root.name + "Tire", 0) == tireIndex)
            {
                dm.UpdateTire(tireIndex);
                mask.color = Color.white;
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(gameObject);
                StartCoroutine(WaitUntilUnselected());
                GetComponent<Button>().onClick.Invoke();
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
                    HPS_MainMenuHandler.Instance.buyCarButton.GetComponentInChildren<TextMeshProUGUI>().text = $"{HR_Tires.Instance.tires[tireIndex].price}";
                    HPS_MainMenuHandler.Instance.buyCarButton.transform.GetChild(1).gameObject.SetActive(true);
                    HPS_MainMenuHandler.Instance.buyCarButton.GetComponent<Button>().onClick.RemoveAllListeners();
                    HPS_MainMenuHandler.Instance.buyCarButton.GetComponent<Button>().onClick.AddListener(() => Buy());
                }
            }

            if (animator && !ignoreAnimation)
                StartCoroutine(ButtonPressed("Pressed"));

            if (EventSystem.current.currentSelectedGameObject == gameObject)
            {
                mask.color = Color.white;
                dm.UpdateTire(tireIndex, false);
                StartCoroutine(WaitUntilUnselected());
            }

        }

    }

    private void OnDisable() {

        HR_VehicleUpgrade_TireManager dm = FindObjectOfType<HR_VehicleUpgrade_TireManager>();
        if (!dm)
            return;

        //if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject)
        {
            mask.color = new Color(1, 1, 1, 0);
            dm.UpdateTire(PlayerPrefs.GetInt(dm.transform.root.name + "Tire", 0), false);
            StopAllCoroutines();
        }
    }

    IEnumerator WaitUntilUnselected()
    {
        yield return new WaitUntil(() => EventSystem.current.currentSelectedGameObject != gameObject);
        mask.color = new Color(1, 1, 1, 0);
        yield return null;
    }

    IEnumerator ButtonPressed(string stateName)
    {
        animator.SetBool(Animator.StringToHash(stateName), true);
        yield return new WaitForSeconds(0.1f);
        animator.SetBool(Animator.StringToHash(stateName), false);
        yield return null;
    }

    public void Upgrade() {

        HR_VehicleUpgrade_TireManager dm = FindObjectOfType<HR_VehicleUpgrade_TireManager>();

        if (!dm || EventSystem.current.currentSelectedGameObject == gameObject && PlayerPrefs.HasKey(dm.transform.root.name + "Tire" + tireIndex.ToString()))
            return;

        dm.UpdateTire(tireIndex);

        CheckPurchase(false);

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
