using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IboAxeHitbox : MonoBehaviour
{
    private int damage;
    private PlayerController ply;
    private void Awake ()
    {
        damage = GetComponentInParent<IboAxe>().RotationDamage;
        ply = GetComponentInParent<PlayerController>();
    }

    private void OnTriggerEnter (Collider col)
    {
        if (col.transform.TryGetComponent(out IDamagable dmg) && !dmg.IsDead && !dmg.IsPlayer)
        {
            dmg.PhotonView.RPC(nameof(dmg.TakeDamageRpc), dmg.PhotonView.Owner, ply.photonView.Controller, damage, DamageType.Katana);
        }
    }
}
