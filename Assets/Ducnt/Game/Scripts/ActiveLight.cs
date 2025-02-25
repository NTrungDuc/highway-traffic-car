using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveLight : MonoBehaviour
{
    public MeshRenderer mesh;
    public GameObject Light;
    bool active = true;
    public void ActiveLightCar()
    {
        mesh = HPS_MainMenuHandler.Instance.currentCar.gameObject.GetComponentsInChildren<MeshRenderer>()[1];
        Light = HPS_MainMenuHandler.Instance.currentCar.transform.GetChild(8).gameObject;
        if (active)
        {
            mesh.material.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
            mesh.material.EnableKeyword("_GLOSSYREFLECTIONS_OFF");
            mesh.material.SetFloat("_Glossiness", 0);
            Light.SetActive(false);
            active = false;
        }
        else
        {
            mesh.material.DisableKeyword("_SPECULARHIGHLIGHTS_OFF");
            mesh.material.DisableKeyword("_GLOSSYREFLECTIONS_OFF");
            mesh.material.SetFloat("_Glossiness", 1f);
            Light.SetActive(true);
            active = true;
        }
    }
}
