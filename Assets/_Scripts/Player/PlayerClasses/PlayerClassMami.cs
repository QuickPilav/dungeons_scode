using UnityEngine;

[System.Serializable]
public class PlayerClassMami : PlayerClassBase
{
    public override void OnStateUpdateOwner (InputPayload input)
    {
    }

    public override void OnStateUpdateNormal ()
    {
    }

    public override void OnStateExit ()
    {
    }

    public override void OnDrawGizmos ()
    {
    }

    public override void PassiveAbilityStart ()
    {
        Debug.Log("gem multiplier ayarlandý!");
        ply.GemMultiplier = 2;
    }

    public override void PassiveAbilityUpdate (InputPayload input)
    {
    }

    public override void ActiveAbilityStart ()
    {
        ActiveAbilityEnd();
        classHandler.CanNotSetAbilityBar = true;
        ply.Inventory.AddItem(ItemSystem.Items.MamiUlti, out InventorySlot slot, 30, 0, isDroppable: false);
        (slot as WeaponSlot).OnCurrentBulletsChanged += RemoveItemOnBulletsEnd;
        ply.Inventory.SwitchSlot(slot.SlotIndex);
    }

    public void RemoveItemOnBulletsEnd (InventorySlot slot, int currentBulletsChanged)
    {
        if (currentBulletsChanged != 0)
            return;
        
        classHandler.CanNotSetAbilityBar = false;

        ply.Inventory.RemoveItem(slot.SlotIndex);
    }

    public override void ActiveAbilityUpdate (InputPayload input)
    {
    }
}
