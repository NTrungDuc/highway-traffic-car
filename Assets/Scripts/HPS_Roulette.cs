using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

//From https://stackoverflow.com/questions/57973167/how-to-choose-when-fortune-wheel-stop-in-unity
public class HPS_Roulette : MonoBehaviour
{

    [Serializable]
    public class WeightedValue
    {
        public int Value;
        public float Weight;
        public int Prize;

        public WeightedValue(int value, float weight, int prize)
        {
            Value = value;
            Weight = weight;
            Prize = prize;
        }
    }

    private struct RandomInfo
    {
        public readonly int Index;
        public readonly int Value;
        public readonly IReadOnlyList<int> WeightedOptions;
        public readonly int AmountOfFullRotations;

        public RandomInfo(List<int> weightedOptions, int minRotations, int maxRotations)
        {
            WeightedOptions = weightedOptions;
            Index = Random.Range(0, WeightedOptions.Count - 1);
            Value = WeightedOptions[Index];
            AmountOfFullRotations = Random.Range(minRotations, maxRotations);
        }
    }

    private enum Prize
    {
        Gold,
        Diamond
    }   

    private List<KeyValuePair<Prize, WeightedValue>> PricesWithWeights = new List<KeyValuePair<Prize, WeightedValue>>
{
    //               Value | Weight TODO: Make sure these sum up to 100
    new KeyValuePair<Prize, WeightedValue>(Prize.Diamond, new WeightedValue(0,  2,  10)),
    new KeyValuePair<Prize, WeightedValue>(Prize.Gold   , new WeightedValue(1,  5,  10000)),
    new KeyValuePair<Prize, WeightedValue>(Prize.Gold   , new WeightedValue(2,  10, 5000)),
    new KeyValuePair<Prize, WeightedValue>(Prize.Diamond, new WeightedValue(3,  1,  15)),
    new KeyValuePair<Prize, WeightedValue>(Prize.Gold   , new WeightedValue(4,  15, 1000)),
    new KeyValuePair<Prize, WeightedValue>(Prize.Gold   , new WeightedValue(5,  3,  15000)),
    new KeyValuePair<Prize, WeightedValue>(Prize.Diamond, new WeightedValue(6,  1,  20)),
    new KeyValuePair<Prize, WeightedValue>(Prize.Gold   , new WeightedValue(7,  30, 500)),
    new KeyValuePair<Prize, WeightedValue>(Prize.Gold   , new WeightedValue(8,  2,  15000)),
    new KeyValuePair<Prize, WeightedValue>(Prize.Diamond, new WeightedValue(9,  20, 1)),
    new KeyValuePair<Prize, WeightedValue>(Prize.Diamond, new WeightedValue(10, 10, 5)),
    new KeyValuePair<Prize, WeightedValue>(Prize.Gold   , new WeightedValue(11, 1,  20000)),
};

    public int MinRotations = 3;
    public int MaxRotations = 6;
    public float SpinDuration = 5;

    public Button spinButton;
    public TextMeshProUGUI spinText;
    public GameObject roulette;
    public GameObject mask;
    public GameObject prize;
    public GameObject reward;
    public RectTransform gold;
    public RectTransform diamond;
    public Sprite[] rewards;
    public Color[] colors;
    public GameObject[] rewardEffects;

    private bool isSpinning;
    private bool hasDailySpin;
    private float anglePerItem;
    private AudioSource clickSound;
    private RandomInfo randomInfo;

    // you can't assign this directly since you want it weighted
    private readonly List<int> weightedList = new List<int>();

    // Start is called before the first frame update
    void Start()
    {
        isSpinning = false;
        anglePerItem = 360f / PricesWithWeights.Count;

        weightedList.Clear();

        // first fill the randomResults accordingly to the given wheights
        foreach (var kvp in PricesWithWeights)
        {
            // add kvp.Key to the list kvp.value times
            for (var i = 0; i < kvp.Value.Weight; i++)
                weightedList.Add(kvp.Value.Value);
        }
    }

    void OnEnable()
    {
        hasDailySpin = PlayerPrefs.GetInt("LastSpinDate") != DateTime.Now.Day ? true : false;
        spinText.text = hasDailySpin ? "SPIN (1/1)" : "Ads (0/1)";

        roulette.transform.eulerAngles = new Vector3(roulette.transform.eulerAngles.x, roulette.transform.eulerAngles.y, 0f);
    }

    public void Spin()
    {
        if (!isSpinning)
        {
            if (hasDailySpin)
                spinText.text = "Ads (0/1)";

            clickSound = HR_CreateAudioSource.NewAudioSource(Camera.main.gameObject, HR_HighwayRacerProperties.Instance.buttonClickAudioClip.name, 0f, 0f, 1f, HR_HighwayRacerProperties.Instance.buttonClickAudioClip, false, true, true);
            clickSound.ignoreListenerPause = true;
            prize.SetActive(false);
            reward.SetActive(false);
            StartCoroutine(SpinRoulette());
        }
    }

    private IEnumerator SpinRoulette()
    {
        randomInfo = new RandomInfo(weightedList, MinRotations, MaxRotations);

        var itemNumberAngle = randomInfo.Value * anglePerItem;
        var currentAngle = roulette.transform.eulerAngles.z;

        while (currentAngle >= 360)
            currentAngle -= 360;

        while (currentAngle < 0)
            currentAngle += 360;

        var targetAngle = -((360 - itemNumberAngle) + (360f * randomInfo.AmountOfFullRotations));

        yield return SpinRoulette(currentAngle, targetAngle, SpinDuration, randomInfo.Value);
    }

    private IEnumerator SpinRoulette(float fromAngle, float toAngle, float withinSeconds, int result)
    {
        isSpinning = true;
        spinButton.interactable = false;

        var passedTime = 0f;
        var vibratationTime = 1f;
        while (passedTime < withinSeconds)
        {
            var lerpFactor = Mathf.SmoothStep(0, 1, (Mathf.SmoothStep(0, 1, passedTime / withinSeconds)));

            roulette.transform.localEulerAngles = new Vector3(0.0f, 0.0f, Mathf.Lerp(fromAngle, toAngle, lerpFactor));
            passedTime += Time.deltaTime;
            vibratationTime -= Time.deltaTime;

            if (vibratationTime <= 0)
            {
                Vibration.VibratePop();
                vibratationTime = 1f;
            }

            yield return null;
        }

        isSpinning = false;
        prize.SetActive(true);
        reward.SetActive(true);

        StartCoroutine(RoulettePrize());

        yield return null;
    }

    private IEnumerator RoulettePrize()
    {
        int numOfEffects = 0;

        switch (PricesWithWeights[randomInfo.Value].Key)
        {
            case Prize.Gold:
                numOfEffects = PricesWithWeights[randomInfo.Value].Value.Prize <= 5000 ? PricesWithWeights[randomInfo.Value].Value.Prize / 500 : PricesWithWeights[randomInfo.Value].Value.Prize / 1000;
                break;
            case Prize.Diamond:
                numOfEffects = PricesWithWeights[randomInfo.Value].Value.Prize;
                break;
        }

        reward.GetComponent<Image>().sprite = rewards[(int)PricesWithWeights[randomInfo.Value].Key];
        Array.ForEach(reward.GetComponentsInChildren<TextMeshProUGUI>(), text => text.text = $"+ {PricesWithWeights[randomInfo.Value].Value.Prize}");
        Array.ForEach(reward.GetComponentsInChildren<TextMeshProUGUI>(), text => text.color = colors[(int)PricesWithWeights[randomInfo.Value].Key]);

        for (int i = 0; i < numOfEffects; i++)
        {
            var go = HPS_ObjectPool.SpawnObject(rewardEffects[(int)PricesWithWeights[randomInfo.Value].Key], reward.GetComponent<RectTransform>().position, Quaternion.identity, HPS_ObjectPool.PoolType.UIGameObject);
            go.GetComponent<HPS_UIRouletteReward>().goldValue = PricesWithWeights[randomInfo.Value].Value.Prize <= 5000 ? 500 : 1000;
            go.GetComponent<HPS_UIRouletteReward>().gotoPosition = PricesWithWeights[randomInfo.Value].Key == Prize.Gold ? gold.position : diamond.position;
            yield return new WaitForSeconds(0.25f);
        }

        PlayerPrefs.SetInt("LastSpinDate", DateTime.Now.Day);
        hasDailySpin = false;

        spinButton.interactable = true;

        yield return null;
    }

}
