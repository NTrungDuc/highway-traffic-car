using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowUnlockBox : MonoBehaviour
{
    public GameObject ButtonGame;
    public GameObject UnlockBoxPanel;
    public float timeShow = 0;

    private void OnEnable()
    {
        StartCoroutine(ActiveUnlockBox());
    }

    public IEnumerator ActiveUnlockBox()
    {
        yield return new WaitForSeconds(timeShow);
        UnlockBoxPanel.SetActive(true);
        ButtonGame.SetActive(true);
    }

    private void OnDisable()
    {
        ButtonGame.SetActive(false);
    }
}
