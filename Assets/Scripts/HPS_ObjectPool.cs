using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class HPS_ObjectPool : MonoBehaviour
{
    public static List<PooledObjectInfo> ObjectPools = new();

    private GameObject _objectPoolEmptyHolder;

    private static GameObject _gameObjectsEmpty;
    private static GameObject _gameObjectsUIEmpty;

    public enum PoolType
    {
        GameObject,
        UIGameObject,
        None
    }

    public static PoolType PoolingType;

    void Awake()
    {
        SetupEmpties();
    }

    private void SetupEmpties()
    {
        _objectPoolEmptyHolder = new GameObject("Pooled Objects");

        _gameObjectsEmpty = new GameObject("GameObjects");
        _gameObjectsEmpty.transform.SetParent(_objectPoolEmptyHolder.transform);

        _gameObjectsUIEmpty = new GameObject("UI GameObjects");
        _gameObjectsUIEmpty.layer = 5;
        var canvas = _gameObjectsUIEmpty.AddComponent<Canvas>();
        canvas.sortingOrder = 1;
        canvas.pixelPerfect = true;
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var canvasScaler = _gameObjectsUIEmpty.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920, 1080);
        canvasScaler.matchWidthOrHeight = 0.5f;
        _gameObjectsUIEmpty.transform.SetParent(_objectPoolEmptyHolder.transform);
    }

    public static GameObject SpawnObject(GameObject objectToSpawn, Vector3 spawnPosition, Quaternion spawnRotation, PoolType poolType = PoolType.None)
    {
        PooledObjectInfo pool = ObjectPools.Find(x => x.LookupString == objectToSpawn.name);

        // If the pool is null, create a new pool
        if (pool == null)
        {
            pool = new PooledObjectInfo { LookupString = objectToSpawn.name };
            ObjectPools.Add(pool);
        }

        GameObject spawnableObj = pool.InactiveObjects.FirstOrDefault();

        if (spawnableObj == null)
        {
            GameObject parentObject = SetParentObject(poolType);

            spawnableObj = Instantiate(objectToSpawn, spawnPosition, spawnRotation);
            
            if (parentObject != null)
                spawnableObj.transform.SetParent(parentObject.transform);
        }
        else
        {
            spawnableObj.transform.position = spawnPosition;
            spawnableObj.transform.rotation = spawnRotation;
            pool.InactiveObjects.Remove(spawnableObj);
            spawnableObj.SetActive(true);
        }

        return spawnableObj;
    }

    public static GameObject SpawnObject(GameObject objectToSpawn, Transform parentTransform)
    {
        PooledObjectInfo pool = ObjectPools.Find(x => x.LookupString == objectToSpawn.name);

        // If the pool is null, create a new pool
        if (pool == null)
        {
            pool = new PooledObjectInfo { LookupString = objectToSpawn.name };
            ObjectPools.Add(pool);
        }

        GameObject spawnableObj = pool.InactiveObjects.FirstOrDefault();

        if (spawnableObj == null)
            spawnableObj = Instantiate(objectToSpawn, parentTransform);
        else
        {
            pool.InactiveObjects.Remove(spawnableObj);
            spawnableObj.SetActive(true);
        }

        return spawnableObj;
    }

    public static void ReturnObjectToPool(GameObject obj)
    {
        string goName = obj.name.Substring(0, obj.name.Length - 7);

        PooledObjectInfo pool = ObjectPools.Find(x => x.LookupString == goName);
        if (pool == null)
        {
            Debug.LogError("No pool found for object: " + obj.name);
            return;
        }
        obj.SetActive(false);
        pool.InactiveObjects.Add(obj);
    }

    private static GameObject SetParentObject(PoolType poolType)
    {
        switch (poolType)
        {
            case PoolType.GameObject:
                return _gameObjectsEmpty;
            case PoolType.UIGameObject:
                return _gameObjectsUIEmpty;
            case PoolType.None:
            default:
                return null;
        }
    }
}

public class PooledObjectInfo
{
    public string LookupString;
    public List<GameObject> InactiveObjects = new();
}
