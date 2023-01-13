using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemUI : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI costText;

    private ShopItemScriptable shopItem;
    private int shopIndex;

    public int ShopIndex { get => shopIndex; }
    public RectTransform Rect { get => rect; }
    public ShopItemScriptable ShopItem { get => shopItem; }

    private RectTransform rect;

    public void Initialize (int shopIndex)
    {
        this.shopIndex = shopIndex;
        rect = GetComponent<RectTransform>();
    }

    public void SetItem (ShopItemScriptable shopItem)
    {
        this.shopItem = shopItem;

        itemIcon.sprite = shopItem.itemScriptable.itemIcon;
        costText.text = shopItem.cost.ToString();

        SaveSocket.OnSettingsChanged.SubscribeToEvent(OnSettingsChanged);
    }

    private void OnDestroy()
    {
        SaveSocket.OnSettingsChanged.UnsubscribeToEvent(OnSettingsChanged);
    }

    private void OnSettingsChanged (SettingsSave save)
    {
        if(save.language == Language.English)
        {
            itemNameText.text = shopItem.itemScriptable.itemName_English;
        }
        else
        {
            itemNameText.text = shopItem.itemScriptable.itemName_Turkish;
        }
    }

    public void OnSelected_Btn ()
    {
        ShopUI.Instance.SelectedShopItem = this;
    }
}
