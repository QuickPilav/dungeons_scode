using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Shop/New Shop Item")]
public class ShopItemScriptable : ScriptableObject
{
    public ItemSystem.Items itemSelling;
    public InventoryScriptableBase itemScriptable;

    public int cost = 100;

    public int currentBullets = -1;
    public int bulletsLeft = -1;
}
