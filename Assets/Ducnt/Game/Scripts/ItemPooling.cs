using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPooling : MonoBehaviour
{
    #region SINGLETON PATTERN
    public static ItemPooling instance;
    public static ItemPooling Instance
    {
        get
        {
            if (instance == null)
                instance = GameObject.FindObjectOfType<ItemPooling>();
            return instance;
        }
    }
    #endregion
    public itemType type;
    private Transform reference;
    public Transform[] lines;
    private bool animateNow
    {
        get
        {
            return HR_GamePlayHandler.Instance.gameStarted;
        }
    }

    public Coin[] coins;

    [System.Serializable]
    public class Coin
    {
        public GameObject coinPrefab;
        public int frequency = 1;
    }

    private List<GameObject> _coinPool = new List<GameObject>();
    public float PositionStartSpawn=0;
    public float distanceSpawn = 0;
    void Start()
    {
        if (type == itemType.Clock)
        {
            if(HR_GamePlayHandler.Instance.mode== HR_GamePlayHandler.Mode.MissionMode_1 || HR_GamePlayHandler.Instance.mode == HR_GamePlayHandler.Mode.MissionMode_2)
                gameObject.SetActive(true);
            else
                gameObject.SetActive(false);
        }
        if (gameObject.activeSelf)
        {
            reference = Camera.main.transform;
            CreateCoins();
            SpawnCoinFirst(_coinPool[0]);
        }
    }

    void Update()
    {
        if (animateNow)
            AnimateCoins();
    }

    void CreateCoins()
    {
        for (int i = 0; i < coins.Length; i++)
        {
            for (int k = 0; k < coins[i].frequency; k++)
            {
                GameObject go = (GameObject)Instantiate(coins[i].coinPrefab, Vector3.zero, Quaternion.identity);
                _coinPool.Add(go);
                go.SetActive(false);
            }
        }
    }

    void AnimateCoins()
    {
        for (int i = 0; i < _coinPool.Count; i++)
        {
            if (reference.transform.position.z > (_coinPool[i].transform.position.z + 15))
            {
                ReAlignCoin(_coinPool[i]);
            }
        }
    }
    void SpawnCoinFirst(GameObject coin)
    {
        if (!coin.activeSelf)
            coin.SetActive(true);
        int randomLine = Random.Range(0, lines.Length);
        coin.transform.position = new Vector3(lines[randomLine].position.x, 1f,
                                              ((PositionStartSpawn * 1000) + 100));
    }
    void ReAlignCoin(GameObject coin)
    {
        if (!coin.activeSelf)
            coin.SetActive(true);
        foreach(Transform Child in coin.transform)
        {
            if(!Child.gameObject.activeSelf)
                Child.gameObject.SetActive(true);
        }
        int randomLine = Random.Range(0, lines.Length);
        float offsetZ = distanceSpawn;
        coin.transform.position = new Vector3(lines[randomLine].position.x, 1f,
                                              (reference.transform.position.z + offsetZ));

        if (CheckIfClipping(coin.GetComponent<BoxCollider>()))
            coin.SetActive(false);
    }

    bool CheckIfClipping(BoxCollider coinBound)
    {
        for (int i = 0; i < _coinPool.Count; i++)
        {
            if (!coinBound.transform.IsChildOf(_coinPool[i].transform) && _coinPool[i].activeSelf)
            {
                if (HR_BoundsExtension.ContainBounds(coinBound.transform, coinBound.bounds,
                                                     _coinPool[i].GetComponent<BoxCollider>().bounds))
                    return true;
            }
        }
        return false;
    }
}
public enum itemType
{
    Gold,
    Diamond,
    Clock
}
