using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapUI : StaticInstance<MinimapUI>
{
    public static bool IsMinimapOpen 
    { 
        get => isMinimapOpen;
        set
        {
            if(Instance != null)
            {
                Instance.minimap.SetActive(value);
            }

            isMinimapOpen = value;
        }
    }
    private static bool isMinimapOpen;
    
    [SerializeField] private GameObject minimap;

    private void Start()
    {
        IsMinimapOpen = false;
    }

    private void Update()
    {
        if(Input.GetButtonDown("Minimap"))
        {
            IsMinimapOpen = !IsMinimapOpen;
        }
    }
}
