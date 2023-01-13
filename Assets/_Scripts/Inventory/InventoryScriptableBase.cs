using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/New ")]
public class InventoryScriptableBase : ScriptableObject
{
    public Sprite itemIcon;
    public string itemName_Turkish;
    public string itemName_English;
    [TextArea()] public string itemDescritipon_Turkish;
    [TextArea()] public string itemDescritipon_English;

    public virtual void EquipItem ()
    {

    }

    public virtual void DisequipItem ()
    {

    }

    public virtual void UseItem ()
    {

    }
}
