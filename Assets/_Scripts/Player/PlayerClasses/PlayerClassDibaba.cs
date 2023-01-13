using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerClassDibaba : PlayerClassBase
{
    [SerializeField] private float setFireOnEverySeconds = 30;
    [SerializeField] private float waitUntilCooldown = 1f;

    private WaitForSeconds waiter;
    private WaitForSeconds cooldownCountdownWaiter;
    private bool onCooldown;
    private bool startedCooldownCountdown;

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
        if (!ply.photonView.IsMine)
            return;
        
        
#if UNITY_EDITOR
        setFireOnEverySeconds = 0.5f;
#endif
        waiter = new WaitForSeconds(setFireOnEverySeconds);
        cooldownCountdownWaiter = new WaitForSeconds(waitUntilCooldown);
        ply.OnHitEnemy += Ply_OnHitEnemy;
    }

    private void Ply_OnHitEnemy (EnemyAI ai, int damage)
    {
        if (onCooldown)
            return;

        ai.SetOnFire(ply.photonView.Controller, 10, 1);

        if (startedCooldownCountdown)
            return;

        ply.StartCoroutine(enumerator());

        IEnumerator enumerator ()
        {
            startedCooldownCountdown = true;

            yield return cooldownCountdownWaiter;

            onCooldown = true;

            yield return waiter;

            onCooldown = false;
            startedCooldownCountdown = false;
        }
    }

    public override void PassiveAbilityUpdate (InputPayload input)
    {
    }

    public override void ActiveAbilityStart ()
    {
        if (!ply.Inventory.Contains(ItemSystem.Items.HandGrenade, out InventorySlot slot))
        {
            Debug.Log("Bomba ekleniyor!!!");
            ply.Inventory.AddItem(ItemSystem.Items.HandGrenade, out slot, isDroppable: false);
            slot.OnCurrentBulletsChanged += OnGrenadeThrown;
        }
        else
        {
            Debug.Log("Bomba zaten varmýþ!!!");
        }
        ply.Inventory.SwitchSlot(slot.SlotIndex);
    }

    private void OnGrenadeThrown (InventorySlot slot, int currentBulletsChanged)
    {
        Debug.Log("On bomba atýldý!");
        ActiveAbilityEnd();
    }

    public override void ActiveAbilityUpdate (InputPayload input)
    {
    }
}
