using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class EnemyDamageHitbox : MonoBehaviour
{
    [SerializeField] private int damage;

    private EnemyAI enemy;
    private void Awake ()
    {
        enemy = GetComponentInParent<EnemyAI>();
    }

    private void OnTriggerEnter (Collider col)
    {
        if (enemy == null || enemy.IsDead)
        {
            Destroy(gameObject);
            return;
        }

        Debug.Log($"hasar verilecek þey {col.name}");
        if(col.transform.TryGetComponent(out IDamagable dmg) && !dmg.IsDead )
        {
            dmg.PhotonView.RPC(nameof(dmg.TakeDamageRpc), dmg.PhotonView.Controller, null, damage, DamageType.Enemy);
        }
    }
}
