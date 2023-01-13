using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerClassSissy : PlayerClassBase
{
    private float runningFor;
    [SerializeField] private float runAbilityFillMultiplier = 2f;
    [Space]
    [SerializeField] private float katanaActiveTime = 30f;

    private WaitForSeconds katanacaActiveWaiter;
    public override void Initialize (PlayerController ply, PlayerClassHandler classHandler)
    {
        base.Initialize(ply, classHandler);

#if UNITY_EDITOR
        runAbilityFillMultiplier = 5f;
#endif

        katanacaActiveWaiter = new WaitForSeconds(katanaActiveTime);
    }

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
        if(ply.NormalizedVelocity == Vector3.zero)
            return;

        runningFor += Time.deltaTime * runAbilityFillMultiplier;

        while (runningFor >= 1f)
        {
            classHandler.AbilityBar += 1;
            runningFor -= 1f;
        }
        
    }

    public override void ActiveAbilityStart ()
    {
        ActiveAbilityEnd();
        classHandler.CanNotSetAbilityBar = true;

        ply.Inventory.AddItem(ItemSystem.Items.Katana, out InventorySlot slot, isDroppable: false);
        ply.Inventory.SwitchSlot(slot.SlotIndex);

        ply.StartCoroutine(enumerator());

        IEnumerator enumerator ()
        {
            yield return katanacaActiveWaiter;
            ply.Inventory.RemoveItem(slot.SlotIndex);
            classHandler.CanNotSetAbilityBar = false;
        }
    }

    

    public override void ActiveAbilityUpdate (InputPayload input)
    {
    }
}
