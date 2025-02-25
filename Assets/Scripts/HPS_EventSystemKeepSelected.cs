using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HPS_EventSystemKeepSelected : MonoBehaviour
{
    private EventSystem eventSystem;
    private GameObject lastSelected = null;
    void Start()
    {
        eventSystem = GetComponent<EventSystem>();
    }

    void Update()
    {
        if (eventSystem != null)
        {
            if (eventSystem.currentSelectedGameObject != null)
            {
                if (eventSystem.currentSelectedGameObject != lastSelected)
                    lastSelected = eventSystem.currentSelectedGameObject;
                else
                    eventSystem.SetSelectedGameObject(eventSystem.currentSelectedGameObject);
            }
            else
                eventSystem.SetSelectedGameObject(lastSelected);
        }
    }
}
