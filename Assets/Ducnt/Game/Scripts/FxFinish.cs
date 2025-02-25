using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FxFinish : MonoBehaviour
{
    [SerializeField] private ParticleSystem[] fxFinishs;
    bool isEnable = false;

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (other.gameObject.CompareTag("Player"))
    //    {
    //        fxFinish.SetActive(true);
    //    }
    //}

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(EnableFxFinish(true));
        }
    }

    IEnumerator EnableFxFinish(bool v)
    {
        if (isEnable)
            yield return null;
        isEnable = v;
        int count = 0;
        while (count < fxFinishs.Length - 1)
        {
            fxFinishs[count].gameObject.SetActive(v);
            fxFinishs[count + 1].gameObject.SetActive(v);
            yield return new WaitForSecondsRealtime(.5f);
            count += 2;
        }
    }
}
