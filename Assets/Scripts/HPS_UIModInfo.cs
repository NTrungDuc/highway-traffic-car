using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HPS_UIModInfo : MonoBehaviour
{
    [HideInInspector] public HR_ShowroomCamera.Focus focusMode;

    GameObject _camera;
    RectTransform _rectTransform;
    GameObject _selected;

    // Start is called before the first frame update
    void Start()
    {
        _camera = Camera.main.gameObject;
        _rectTransform = GetComponent<RectTransform>();
        Show(0);
    }

    public void Show(float value)
    {
        if (value == 0)
        {
            _rectTransform.localScale = Vector3.zero;
            return;
        }

        _rectTransform.position = new Vector3(-0.2f, 0.6f, -2f);
        _rectTransform.localScale = new Vector3(0.00075f, 0.00075f, 0.00075f);

        switch (focusMode)
        {
            case HR_ShowroomCamera.Focus.Rim:
                _rectTransform.sizeDelta = new Vector2(450f, 100f);
                GetComponentInChildren<Text>().text = "ACCEL +" + value.ToString() + "%";
                break;
            case HR_ShowroomCamera.Focus.Tire:
                _rectTransform.sizeDelta = new Vector2(525f, 100f);
                GetComponentInChildren<Text>().text = "HANDLING +" + value.ToString() + "%";
                break;
            case HR_ShowroomCamera.Focus.Spoiler:
                _rectTransform.position = new Vector3(0.8f, 1.25f, -1.6f);
                _rectTransform.sizeDelta = new Vector2(450f, 75f);
                _rectTransform.localScale = new Vector3(0.002f, 0.002f, 0.002f);
                GetComponentInChildren<Text>().text = "NITRO +" + value.ToString() + "%";
                break;
        }

        _selected = EventSystem.current.currentSelectedGameObject;
    }

    public void Hide() => _rectTransform.localScale = Vector3.zero;

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.rotation = Quaternion.LookRotation(transform.position - _camera.transform.position);
        if (_selected != EventSystem.current.currentSelectedGameObject)
            Show(0);
    }
}
