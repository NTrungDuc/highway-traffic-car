using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SoundCarManager : ScriptableObject
{
    public static SoundCarManager instance;
    public static SoundCarManager Instance {
        get
        {

            if (instance == null)
                instance = Resources.Load("SoundCarManager") as SoundCarManager;

            return instance;

        }

    }
    public AudioClip coinSound;
    public AudioClip destroySound;
    public AudioClip winSound;
    public AudioClip nitroSound;

}
