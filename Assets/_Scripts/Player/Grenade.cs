using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GrenadeType
{
    SmokeGrenade,
    HandGrenade
}

public class Grenade : ItemInHand
{
    [SerializeField] private GrenadeType grenadeType;

    public override void Fire ()
    {
        if (!ws.IsMine)
            return;

        base.Fire();

        inv.CurrentSlot.OnCurrentBulletsChanged?.Invoke(inv.CurrentSlot, 0);

        switch (grenadeType)
        {
            case GrenadeType.SmokeGrenade:

                Debug.Log("smoke bombasý atýldý!");
                SpawnManager.Instance.SpawnSmokeGrenadeProjectile(transform.position, CameraSystem.Instance.MousePos);

                break;
            case GrenadeType.HandGrenade:

                Debug.Log("el bombasý atýldý!");
                SpawnManager.Instance.SpawnHandGrenadeProjectile(transform.position, CameraSystem.Instance.MousePos);

                break;
        }

        ws.RemoveCurrentItem();
    }
}
