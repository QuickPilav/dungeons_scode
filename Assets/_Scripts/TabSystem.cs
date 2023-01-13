using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TabSystem : MonoBehaviour
{
    [SerializeField] private Transform tabButtonsHolder;
    [SerializeField] private GameObject[] tabs;
    [SerializeField] private int awakeTab = 0;
    private void Awake()
    {
        ShowTab(awakeTab);
    }

    public void ShowTab (int tabIndex)
    {
        foreach (var item in tabs)
        {
            item.SetActive(false);
        }

        tabs[tabIndex].SetActive(true);
    }
}
