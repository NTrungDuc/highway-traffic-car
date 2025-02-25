using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public ItemType Type;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {            
            HR_PlayerHandler playerHandler = other.transform.root.gameObject.GetComponent<HR_PlayerHandler>();
            if (!playerHandler.isCheckLostOrWin || !playerHandler.gameWin || !playerHandler.crashed)
            {
                if (Type == ItemType.Gold)
                {
                    playerHandler.gold += 1;
                    HR_GamePlayHandler.Instance.GetItems(0);
                }
                else if(Type == ItemType.Diamond)
                {
                    playerHandler.diamond += 1;
                    HR_GamePlayHandler.Instance.GetItems(1);
                }
                else if (Type == ItemType.Clock)
                {
                    playerHandler.timeLeft += 15;
                    HR_GamePlayHandler.Instance.GetItems(2);
                }
                gameObject.SetActive(false);
            }
            
        }
        if(other.gameObject.CompareTag("AIMuti"))
        {
            gameObject.SetActive(false);
        }
    }
}
public enum ItemType
{
    Gold,
    Diamond,
    Clock
}
