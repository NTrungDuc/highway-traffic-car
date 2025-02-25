using System.Collections;
using UnityEngine;

public class HPS_UIRouletteReward : MonoBehaviour
{
    public int goldValue;
    public Vector3 gotoPosition;

    private RectTransform rectTransform;

    void OnEnable()
    {
        rectTransform = GetComponent<RectTransform>();
        rectTransform.localScale = 1.5f * Vector3.one;
        StartCoroutine(Pop());
    }

    private IEnumerator Pop()
    {
        float radius = Random.Range(100, 200);
        float angle = Random.Range(0, 2 * Mathf.PI);

        Vector2 targetPosition = rectTransform.position + new Vector3(radius * Mathf.Cos(angle), radius * Mathf.Sin(angle), 0f); // Random position in a circle
        float duration = 1.0f; // Duration of the movement
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            rectTransform.position = Vector2.Lerp(rectTransform.position, targetPosition, (elapsedTime / duration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        rectTransform.position = targetPosition;

        yield return new WaitForSeconds(0.5f);

        elapsedTime = 0;
        targetPosition = gotoPosition;
        Vector3 scale = rectTransform.localScale;

        while (elapsedTime < duration)
        {
            rectTransform.position = Vector2.Lerp(rectTransform.position, targetPosition, (elapsedTime / duration));
            rectTransform.localScale = Vector3.Lerp(scale, Vector3.one, (elapsedTime / duration));
            elapsedTime += Time.deltaTime;

            if (Vector3.Distance(rectTransform.position, targetPosition) < 5f)
                break;

            yield return null;
        }

        HPS_ObjectPool.ReturnObjectToPool(gameObject);

        switch (gameObject.name.Substring(0, gameObject.name.Length - 7))
        {
            case "Gold":
                PlayerPrefs.SetInt("CurrencyGold", PlayerPrefs.GetInt("CurrencyGold") + goldValue);
                break;
            case "Diamond":
                PlayerPrefs.SetInt("CurrencyDiamond", PlayerPrefs.GetInt("CurrencyDiamond") + 1);
                break;
        }

    }
}
