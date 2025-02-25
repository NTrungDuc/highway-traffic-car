using System.Collections.Generic;
using UnityEngine;
using System;

namespace HT.DEBUG
{
    [Serializable]
    public class Localization : ScriptableObject
    {
        public LanguageType Language = LanguageType.English;
        [SerializeField]
        public List<string> LanguageKey = new List<string>();
        [SerializeField]
        public List<string> LanguageEnglish = new List<string>();

        public string LanguageValue(int index)
        {
            if (index >= 0 && index < LanguageKey.Count)
            {
                if (Language == LanguageType.English)
                {
                    if (index >= 0 && index < LanguageEnglish.Count)
                    {
                        return LanguageEnglish[index];
                    }
                    else
                    {
                        return LanguageKey[index];
                    }
                }
                else
                {
                    return LanguageKey[index];
                }
            }
            else
            {
                return "<None>";
            }
        }
    }

    public enum LanguageType
    {
        English
    }
}
