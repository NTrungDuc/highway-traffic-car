using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RankUp : MonoBehaviour
{
    //public Image fillRankBar;
    public Slider progressRank;
    public Slider bonusRank;
    int indexRankGame;
    int indexRank = 0;
    public Image iconRank;
    public Image BGRank;
    public Sprite[] ListRank;
    public Text pointRank;
    public Text rankName;
    public bool isFinished;

    private void OnEnable()
    {
        iconRank.sprite = ListRank[PlayerPrefs.GetInt("indexRank", 0)];
        progressRank.value = PlayerPrefs.GetFloat("ProgressFillRank", 0);
        bonusRank.value = PlayerPrefs.GetFloat("ProgressFillRank", 0);
        indexRankGame = PlayerPrefs.GetInt("IndexRankGame", 0);
        indexRank = PlayerPrefs.GetInt("indexRank", 0);
    }
    private void FixedUpdate()
    {
        BGRank.transform.Rotate(0, 0, 30f * Time.fixedDeltaTime);
    }
    IEnumerator IncreaseFillRankBar(float currentPoint)
    {
        RankData rankData = DataRank.Instance.rankData;

        while (indexRankGame < rankData.rankGames.Length - 1)
        {
            float minThreshold = rankData.rankGames[indexRankGame].rankPoint;
            float maxThreshold = rankData.rankGames[indexRankGame + 1].rankPoint;

            float newProgress = (currentPoint - minThreshold) / (maxThreshold - minThreshold);

            while (currentPoint >= maxThreshold)
            {
                while (bonusRank.value < 1f)
                {
                    bonusRank.value += 0.3f * Time.deltaTime;
                    bonusRank.value = Mathf.Min(bonusRank.value, 1f);
                    PlayerPrefs.SetFloat("ProgressFillRank", bonusRank.value);
                    yield return null;
                }

                indexRankGame++;
                rankName.text = rankData.rankGames[indexRankGame].rankName;
                PlayerPrefs.SetString("RankName", rankName.text.ToString());
                if (indexRankGame >= rankData.rankGames.Length - 1)
                    break;

                bonusRank.value = 0f;
                progressRank.value = 0;
                PlayerPrefs.SetInt("IndexRankGame", indexRankGame);
                PlayerPrefs.SetFloat("ProgressFillRank", bonusRank.value);
                if (PlayerPrefs.GetInt("IndexRankGame") % 5 == 0 && PlayerPrefs.GetInt("IndexRankGame") != 0)
                {
                    indexRank += 1;
                    PlayerPrefs.SetInt("indexRank", indexRank);
                }
                iconRank.sprite = ListRank[PlayerPrefs.GetInt("indexRank", 0)];
                minThreshold = rankData.rankGames[indexRankGame].rankPoint;
                maxThreshold = rankData.rankGames[indexRankGame + 1].rankPoint;
                newProgress = (currentPoint - minThreshold) / (maxThreshold - minThreshold);
            }

            while (bonusRank.value < newProgress)
            {
                bonusRank.value += 0.3f * Time.deltaTime;
                bonusRank.value = Mathf.Min(bonusRank.value, newProgress);
                PlayerPrefs.SetFloat("ProgressFillRank", bonusRank.value);

                yield return null;
            }

            break;
        }

        if (indexRankGame >= rankData.rankGames.Length - 1)
        {
            bonusRank.value = 1f;

            rankName.text = rankData.rankGames[rankData.rankGames.Length - 1].rankName;
            PlayerPrefs.SetString("RankName", rankName.text.ToString());
            PlayerPrefs.SetInt("IndexRankGame", indexRankGame);
            PlayerPrefs.SetFloat("ProgressFillRank", bonusRank.value);
        }

        isFinished = true;
    }
    IEnumerator IncreaseRankPoint(float currentPoint, RankData rankData)
    {
        float rankValue = PlayerPrefs.GetFloat("CurrentRankPoint", 0);
        while (rankValue < currentPoint)
        {
            rankValue += 50f * Time.deltaTime;
            rankValue=Mathf.Min(rankValue, currentPoint);
            int indexRank;
            if (indexRankGame < rankData.rankGames.Length - 1)
                indexRank = indexRankGame + 1;
            else
                indexRank = rankData.rankGames.Length - 1;
            pointRank.text = rankValue.ToString("F0") + "/" + rankData.rankGames[indexRank].rankPoint;
            if(isFinished)
                pointRank.text = currentPoint.ToString("F0") + "/" + rankData.rankGames[indexRank].rankPoint;
            yield return null;
        }
        //yield return new WaitUntil(() => isFinished);
        //pointRank.text = currentPoint.ToString("F0") + "/" + rankData.rankGames[indexRankGame + 1].rankPoint;
    }
    public void GetCurrentRank(Text currentRank, float currentPoint)
    {
        RankData rankData = DataRank.Instance.rankData;

        bool isMaxRankReached = false;
        foreach (rankGame rank in rankData.rankGames)
        {
            if (currentPoint >= rank.rankPoint)
            {
                //currentRank.text = rank.rankName;
                if (rank == rankData.rankGames[rankData.rankGames.Length - 1])
                {
                    isMaxRankReached = true;
                }
            }
        }
        if (isMaxRankReached)
        {
            pointRank.text = PlayerPrefs.GetFloat("CurrentRankPoint", 0).ToString("F0") + "/" + rankData.rankGames[rankData.rankGames.Length - 1].rankPoint;
            currentRank.text = "Legend V";
        }
        else
        {
            pointRank.text = PlayerPrefs.GetFloat("CurrentRankPoint", 0).ToString("F0") + "/" + rankData.rankGames[indexRankGame + 1].rankPoint;
            rankName.text = rankData.rankGames[indexRankGame].rankName;
        }
        StartCoroutine(IncreaseFillRankBar(currentPoint));
        StartCoroutine(IncreaseRankPoint(currentPoint, rankData));
        PlayerPrefs.SetString("RankName", currentRank.text.ToString());
    }

}
