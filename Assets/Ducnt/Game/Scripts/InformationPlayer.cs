using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InformationPlayer : MonoBehaviour
{
    public Image FlagPlayer;
    public Text rankPoint;
    public Slider progressRank;
    public Sprite[] ListRank;
    public Image iconRank;

    public Text namePlayer;
    public Text inputNamePlayer;
    public InputField inputName;
    public Text nameRank;
    public Image topBG;
    public GameObject InforPlayer;
    string path = "Flag";
    // Start is called before the first frame update
    void Start()
    {
        GetInformation();
    }
    private void FixedUpdate()
    {
        //if (InforPlayer.activeSelf)
        //    topBG.enabled = false;
        //else
        //    topBG.enabled = true;
    }
    void GetInformation()
    {
        if (Resources.Load<Sprite>($"{path}/{PlayerPrefs.GetString("countryCode")}") != null)
            FlagPlayer.sprite = Resources.Load<Sprite>($"{path}/{PlayerPrefs.GetString("countryCode")}");
        else
        {
            FlagPlayer.sprite = Resources.Load<Sprite>($"{path}/{"RacingFlags"}");
            PlayerPrefs.SetString("countryCode", "RacingFlags");
        }
        //rankPoint.text = PlayerPrefs.GetFloat("CurrentRankPoint").ToString();
        RankData rankData = DataRank.Instance.rankData;
        rankPoint.text = PlayerPrefs.GetFloat("CurrentRankPoint", 0).ToString("F0") + "/" + rankData.rankGames[PlayerPrefs.GetInt("IndexRankGame", 0) + 1].rankPoint;
        progressRank.value = PlayerPrefs.GetFloat("ProgressFillRank");
        iconRank.sprite = ListRank[PlayerPrefs.GetInt("indexRank", 0)];

        namePlayer.text = PlayerPrefs.GetString("PlayerName", "HYPERSOL");
        nameRank.text = PlayerPrefs.GetString("RankName", "Newbie I");
    }
    public void SubmitName()
    {
        namePlayer.text = inputNamePlayer.text;
        PlayerPrefs.SetString("PlayerName",namePlayer.text);
        inputName.text = "";
    }
}
