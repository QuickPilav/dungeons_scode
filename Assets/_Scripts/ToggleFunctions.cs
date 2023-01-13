using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleFunctions : MonoBehaviour
{
    [SerializeField] private SettingsUI.ToggleSettings toggleSeting;

    private void Awake()
    {
        GetComponent<Toggle>().onValueChanged.AddListener(OnToggleChanged);
    }

    public void OnToggleChanged(bool newSetting)
    {
        ClientUI.SettingsInstance.OnChanged_Toggle(newSetting, toggleSeting);
    }

}
