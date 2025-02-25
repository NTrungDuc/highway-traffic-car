using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "DataCarAI", menuName = "Inventory/Data")]
public class DataCarAI : ScriptableObject
{
    public List<DataCarAi> dataCarAi;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    [Serializable]
    public class DataCarAi
    {
        public Sprite nationalFlag;
        public List<string> fullName;
    }    
}
