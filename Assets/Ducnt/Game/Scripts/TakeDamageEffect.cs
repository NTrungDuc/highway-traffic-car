using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TakeDamageEffect : MonoBehaviour
{
    private static TakeDamageEffect instance;
    public static TakeDamageEffect Instance { get { return instance; } }

    [SerializeField] private GameObject BrokenGlass;
    internal bool isEnable = false;

    private void Awake()
    {
        instance = this;
    }

    public IEnumerator takeDamageEffect(bool isEnabled)
    {
        if (!isEnable)
        {
            ProgressMissionMode.Instance.disableProgressBar();
            if (BrokenGlass != null)
                BrokenGlass.SetActive(isEnabled);

            yield return new WaitForSecondsRealtime(1.8f);
            //Debug.Log("disable take damage effect");
            BrokenGlass.SetActive(!isEnabled);
            isEnable = true;
        }
    }
}
