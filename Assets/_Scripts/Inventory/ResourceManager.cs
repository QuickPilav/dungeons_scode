using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ResourceManager
{
    public static Dictionary<string, InventoryScriptableBase> InventoryScriptableBases
    {
        get
        {
            if (inventoryScriptableBases == null)
            {
                inventoryScriptableBases = new Dictionary<string, InventoryScriptableBase>();
                foreach (var item in Resources.LoadAll<InventoryScriptableBase>("Inventory"))
                {
                    inventoryScriptableBases.Add(item.name, item);
                }

            }

            return inventoryScriptableBases;
        }
    }
    private static Dictionary<string, InventoryScriptableBase> inventoryScriptableBases;

    public static InventoryScriptableBase GetItem (string itemName)
    {
        if (itemName == string.Empty)
            return null;

        InventoryScriptableBases.TryGetValue(itemName, out InventoryScriptableBase item);        
        return item;
    }




    public static Dictionary<string, PlayerClassScriptable> PlayerClassScriptableBases
    {
        get
        {
            if (playerClassScriptableBases == null)
            {
                playerClassScriptableBases = new Dictionary<string, PlayerClassScriptable>();
                foreach (var item in Resources.LoadAll<PlayerClassScriptable>("PlayerClasses"))
                {
                    playerClassScriptableBases.Add(item.name, item);
                }

            }

            return playerClassScriptableBases;
        }
    }
    private static Dictionary<string, PlayerClassScriptable> playerClassScriptableBases;

    public static PlayerClassScriptable GetPlayerClassScriptable(PlayerClass className)
    {
        PlayerClassScriptableBases.TryGetValue(className.ToString(), out PlayerClassScriptable item);
        return item;
    }



    public static Dictionary<string, ShopItemScriptable> ShopScriptableBases
    {
        get
        {
            if (shopScriptableBases == null)
            {
                shopScriptableBases = new Dictionary<string, ShopItemScriptable>();
                foreach (var item in Resources.LoadAll<ShopItemScriptable>("Shop"))
                {
                    shopScriptableBases.Add(item.name, item);
                }

            }

            return shopScriptableBases;
        }
    }
    private static Dictionary<string, ShopItemScriptable> shopScriptableBases;

    public static ShopItemScriptable GetShopItemScriptable (string itemName)
    {
        if (itemName == string.Empty)
            return null;

        ShopScriptableBases.TryGetValue(itemName, out ShopItemScriptable item);
        return item;
    }

    public static LanguageScriptable GetLanguageScriptable(Language language)
    {
        return Resources.LoadAll<LanguageScriptable>("Languages").Where(x => x.name == language.ToString()).First();
    }

    public static MissionBase[] GetMissionBases()
    {
        return Resources.LoadAll<MissionScriptable>("Missions").Select(x => x.GetMission()).ToArray();
    }
}
