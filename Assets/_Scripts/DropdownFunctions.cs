using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DropdownFunctions : MonoBehaviour
{
    [SerializeField] private SettingsUI.DropdownSettings dropdownSeting;

    private void Awake()
    {
        GetComponent<TMP_Dropdown>().onValueChanged.AddListener(OnDropdownChanged);
    }

    private void OnDropdownChanged(int newSetting)
    {
        ClientUI.SettingsInstance.OnChanged_Dropdown(newSetting, dropdownSeting);

    }
}
