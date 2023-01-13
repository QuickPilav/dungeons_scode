using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class ShopUI : StaticInstance<ShopUI>
{
    public bool IsShopItemSelected { get => selectedShopIndex >= 0 && selectedShopIndex < shopItems.Count; }
    public bool IsShopOpen
    {
        get => isShopOpen;
        set
        {
            isShopOpen = value;
            shopMenu.SetActive(value);
            ClientUI.Instance.RecalculateIsPaused();
            ClientUI.SetCursor(value);

            if (value)
            {
                SelectedShopItem = shopItems[selectedShopIndex];
            }
        }
    }

    public ShopItemUI SelectedShopItem
    {
        get
        {
            return shopItems[selectedShopIndex];
        }
        set
        {
            selectedShopIndex = value.ShopIndex;

            StartCoroutine(itemsScroll.FocusOnItemCoroutine(value.Rect, 5f));
            selectedItemShower.position = value.Rect.position;

            itemIcon.sprite = value.ShopItem.itemScriptable.itemIcon;
            itemCostText.text = value.ShopItem.cost.ToString();
            if (SaveSocket.CurrentSave.settings.language == Language.English)
            {
                itemDescriptionText.text = value.ShopItem.itemScriptable.itemDescritipon_English;
                itemNameText.text = value.ShopItem.itemScriptable.itemName_English;
            }
            else
            {
                itemDescriptionText.text = value.ShopItem.itemScriptable.itemDescritipon_Turkish;
                itemNameText.text = value.ShopItem.itemScriptable.itemName_Turkish;
            }

        }
    }
    private int selectedShopIndex;

    [SerializeField] private GameObject shopMenu;
    [Space]
    [SerializeField] private ShopItemUI shopUIPrefab;
    [SerializeField] private Transform shopUIParent;
    [SerializeField] private RectTransform selectedItemShower;
    [SerializeField] private ScrollRect itemsScroll;
    [Space]
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemDescriptionText;
    [SerializeField] private TextMeshProUGUI itemCostText;
    [SerializeField] private Image itemIcon;

    private List<ShopItemUI> shopItems;

    private bool isShopOpen;

    public void Initialize ()
    {
        shopItems = new List<ShopItemUI>();

        int i = 0;
        foreach (var item in ResourceManager.ShopScriptableBases)
        {
            var spawned = Instantiate(shopUIPrefab, shopUIParent);
            spawned.Initialize(i);
            spawned.SetItem(item.Value);
            shopItems.Add(spawned);
            i++;
        }
        selectedItemShower.SetAsLastSibling();
        IsShopOpen = true;

        SelectedShopItem = shopItems[0];

        IsShopOpen = false;
    }

    public void BuyItem_Btn ()
    {
        if (!IsShopItemSelected)
            return;


        PlayerController ply = PlayerController.ClientInstance.Value;
        var selectedShopItem = SelectedShopItem;
        
#if !UNITY_EDITOR //eðer editorde isek beleþe alabilelim her þeyi!
        if (ply.GemsHave < selectedShopItem.ShopItem.cost)
        {
            Debug.LogWarning("not enough gems!");
            return;
        }
#endif
        

        if(!ply.Inventory.AddItem(selectedShopItem.ShopItem.itemSelling,out _,selectedShopItem.ShopItem.currentBullets, selectedShopItem.ShopItem.bulletsLeft))
        {
            Debug.LogWarning("Envanter dolu!");
            return;
        }
        ply.GemsHave -= selectedShopItem.ShopItem.cost;

    }

}
