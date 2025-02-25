using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
public class InforAI : MonoBehaviour
{
    public static InforAI instance;
    public DataCarAI dataAI;
    public Image NationalFlagPlayer;
    public TextMeshProUGUI textVictory;
    public TextMeshProUGUI textLost;
    public Image NationalFlag;
    public TextMeshProUGUI fullName;
    public TextMeshProUGUI win;
    public TextMeshProUGUI Lost;
    public GameObject AI;
    string path = "Flag";

    private static List<int> randomNumbers; // Biến tĩnh để duy trì danh sách trong suốt thời gian game
    private static int currentIndex = 0; // Biến tĩnh để giữ vị trí trong danh sách
    private void OnEnable()
    {
        StartCoroutine(DelayAIInfor());
    }
    // Start is called before the first frame update
    void Start()
    {
        NationalFlagPlayer.sprite = Resources.Load<Sprite>($"{path}/{PlayerPrefs.GetString("countryCode")}");
        textVictory.text = PlayerPrefs.GetInt("numberWin").ToString();
        textLost.text = PlayerPrefs.GetInt("numberLost").ToString();
        if(PlayerPrefs.GetInt("IsFirstTime") == 0)
        {
            InitializeRandomNumbers();
            PlayerPrefs.SetInt("IsFirstTime", 1);
            UnityEngine.Debug.Log("viet_1204");
        }

    }

    // Update is called once per frame
    void Update()
    {
    }

    private void InitializeRandomNumbers()
    {
        randomNumbers = new List<int>();
        for (int i = 0; i < dataAI.dataCarAi.Count; i++)
        {
            randomNumbers.Add(i);
        }

        Shuffle(randomNumbers); // Xáo trộn danh sách
    }

    private void Shuffle<T>(List<T> list)
    {
        System.Random rand = new System.Random(System.DateTime.Now.Millisecond);
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = rand.Next(i + 1);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    private int GetNextRandomNumber()
    {
        if (randomNumbers.Count == 0) // Nếu danh sách trống, reset lại danh sách
        {
            InitializeRandomNumbers(); // Khởi tạo lại danh sách
        }


        int randomIndex = Random.Range(0, randomNumbers.Count); // Lấy chỉ số ngẫu nhiên từ danh sách còn lại
        int number = randomNumbers[randomIndex]; // Lấy số từ danh sách
        randomNumbers.RemoveAt(randomIndex); // Loại bỏ số đã chọn khỏi danh sách
        return number; // Trả về số đã chọn
    }


    IEnumerator DelayAIInfor()
    {
        yield return new WaitForSeconds(1f);
        AI.SetActive(true);
        int number = GetNextRandomNumber(); // Lấy số random không trùng lặp
        NationalFlag.sprite = dataAI.dataCarAi[number].nationalFlag;
        var numberName = Random.Range(0, 9);
        fullName.text = dataAI.dataCarAi[number].fullName[numberName];
        PlayerPrefs.SetInt("NumberDataAI", number);
        PlayerPrefs.SetInt("NumberDataAIName", numberName);
        var numberWinOrLost = Random.Range(1, 100);
        if(numberWinOrLost % 2 == 0)
        {
            win.text = Random.Range(0, 9).ToString();
            Lost.text = Random.Range(0, 9).ToString();
        }
        else
        {
            win.text = Random.Range(10, 99).ToString();
            Lost.text = Random.Range(10, 99).ToString();
        }

    }
}
