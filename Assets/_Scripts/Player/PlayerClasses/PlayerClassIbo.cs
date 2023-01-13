using UnityEngine;

[System.Serializable]
public class PlayerClassIbo : PlayerClassBase
{
    public float AdditiveDamageMultiplierToProximity { get => additiveDamageMultiplierToProximity; }
    [SerializeField] private float additiveDamageMultiplierToProximity = 0.05f;

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
    }

    public override void PassiveAbilityUpdate (InputPayload input)
    {
    }

    public override void ActiveAbilityStart ()
    {
        ActiveAbilityEnd();
        classHandler.CanNotSetAbilityBar = true;

        ply.Inventory.AddItem(ItemSystem.Items.IboAxe, out InventorySlot slot, isDroppable: false);
        ply.Inventory.SwitchSlot(slot.SlotIndex);
    }

    public override void ActiveAbilityUpdate (InputPayload input)
    {
    }
}
