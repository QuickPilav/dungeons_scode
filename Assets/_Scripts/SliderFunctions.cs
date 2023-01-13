using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderFunctions : MonoBehaviour
{
    [SerializeField] private SettingsUI.SliderSettings sliderSeting;

    private void Awake()
    {
        GetComponent<Slider>().onValueChanged.AddListener(OnSliderChanged);
    }

    private void OnSliderChanged(float newSetting)
    {
        ClientUI.SettingsInstance.OnChanged_Slider(newSetting, sliderSeting);

    }
}
