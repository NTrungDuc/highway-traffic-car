using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HPS_UIModeSelection : MonoBehaviour
{
    public GameObject image;
    public GameObject selectionBar;
    public GameObject way;
    public GameObject scene;

    private List<GameObject> modes;
    private List<GameObject> ways;
    private List<GameObject> scenes;
    private GameObject currSelectMode;

    private GameObject currSelectedMode;

    void Start()
    {
        modes = new();
        ways = new();
        scenes = new();

        for (int i = 0; i < selectionBar.transform.childCount; i++)
            modes.Add(selectionBar.transform.GetChild(i).gameObject);

        for (int i = 0; i < way.transform.childCount; i++)
            ways.Add(way.transform.GetChild(i).gameObject);

        for (int i = 0; i < scene.transform.childCount; i++)
            scenes.Add(scene.transform.GetChild(i).gameObject);

        ModeSelected(modes[PlayerPrefs.GetInt("SelectedMode", 0)]);
        WaySelected(ways[0]);
        SceneSelected(scenes[0]);
    }

    void OnEnable()
    {
        if (currSelectedMode)
            ModeSelected(currSelectedMode);
    }

    public void ModeSelected(GameObject selected)
    {
        for (int i = 0; i < image.transform.childCount; i++)
        {
            if (i == modes.IndexOf(selected))
            {
                if (i == 0)
                    HPS_MainMenuHandler.Instance.MissionModeMap();
                else if (i == 1)
                    HPS_MainMenuHandler.Instance.SelcetSceneModeDual();
                else if (i == 2)
                {
                    HPS_MainMenuHandler.Instance.selectCarButton.SetActive(true);
                    HPS_MainMenuHandler.Instance.goPlayButton.SetActive(false);
                }
                else
                {
                    HPS_MainMenuHandler.Instance.selectCarButton.SetActive(false);
                    HPS_MainMenuHandler.Instance.goPlayButton.SetActive(false);
                }

                currSelectedMode = modes[i];
                image.transform.GetChild(i).gameObject.SetActive(true);
                modes[i].transform.GetChild(0).GetComponent<Image>().color = Color.white;
            }
            else
            {
                image.transform.GetChild(i).gameObject.SetActive(false);
                modes[i].transform.GetChild(0).GetComponent<Image>().color = new Color(1, 1, 1, 0);
            }
        }
    }

    public void WaySelected(GameObject selected)
    {
        for (int i = 0; i < way.transform.childCount; i++)
        {
            if (i == ways.IndexOf(selected))
            {
                ways[i].transform.GetChild(0).GetComponent<Image>().color = Color.white;
                ways[i].transform.GetChild(2).GetComponent<TextMeshProUGUI>().color = Color.white;
            }
            else
            {
                ways[i].transform.GetChild(0).GetComponent<Image>().color = new Color(1, 1, 1, 0);
                ways[i].transform.GetChild(2).GetComponent<TextMeshProUGUI>().color = new Color(1, 1, 1, 0.5f);
            }
        }
    }

    public void SceneSelected(GameObject selected)
    {
        for (int i = 0; i < scene.transform.childCount; i++)
        {
            if (i == scenes.IndexOf(selected))
            {
                switch (i)
                {
                    case 0:
                        PlayerPrefs.SetString("SelectedScene", "HighwaySunny");
                        break;
                    case 1:
                        PlayerPrefs.SetString("SelectedScene", "HighwayAfternoon");
                        break;
                    case 2:
                        PlayerPrefs.SetString("SelectedScene", "HighwayNight");
                        break;
                    case 3:
                        PlayerPrefs.SetString("SelectedScene", "HighwayRainy");
                        break;
                }
                scenes[i].transform.GetChild(0).GetComponent<Image>().color = Color.white;
                scenes[i].transform.GetChild(1).GetComponent<Image>().color = new Color(1, 1, 1, 0);
                scenes[i].transform.GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
            }
            else
            {
                scenes[i].transform.GetChild(0).GetComponent<Image>().color = new Color(1, 1, 1, 0);
                scenes[i].transform.GetChild(1).GetComponent<Image>().color = Color.white;
                scenes[i].transform.GetComponentInChildren<TextMeshProUGUI>().color = new Color(1, 1, 1, 0.5f);
            }
        }
    }

}
