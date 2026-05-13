using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PowerMeterUI : MonoBehaviour
{
    [SerializeField] private Slider _slider;
    [SerializeField] private float oscillateSpeed;
    [SerializeField] private GameObject _sliderObj;
    private bool isActive = false;
    private float direction = 1f;

    public void Activate()
    {
        _sliderObj.SetActive(true);
        isActive = true;
        _slider.value = 0f;
    }

    private void FixedUpdate()
    {
        if (isActive)
        {
            _slider.value += oscillateSpeed * direction;

            if (_slider.value >= 1f)
            {
                _slider.value = 1f;
                direction = -1f;
            }
            else if (_slider.value <= 0f)
            {
                _slider.value = 0f;
                direction = 1f;
            }
        }
    }

    public float Release()
    {
        isActive = false;
        return _slider.value;
    }

    public void Deactivate()
    {
        _slider.value = 0f;
        _sliderObj.SetActive(false);
    }

}