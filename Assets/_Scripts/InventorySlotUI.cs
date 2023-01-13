using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    public RectTransform Rect { get => rect; }

    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private Image itemIcon;
    private RectTransform rect;

    private void Awake ()
    {
        rect = GetComponent<RectTransform>();
    }

    public void SetItem (string itemName, int itemAmount)
    {
        var item = ResourceManager.GetItem(itemName);
        itemIcon.sprite = item.itemIcon;
        if(item is InventoryScriptableWeapon)
        {
            itemNameText.text = itemAmount.ToString();
        }
        else
        {
            itemNameText.text = itemAmount > 0 ? itemAmount.ToString() : string.Empty;
        }
    }
}
