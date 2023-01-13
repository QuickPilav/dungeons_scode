using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerClassAmy : PlayerClassBase
{
    [SerializeField] private int healthAdditionPerRandomKill = 5;
    [Space]
    [SerializeField] private float ultimateHealRange = 10.8f;
    [SerializeField] private int ultimateHealAmount = 20;

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
        if (ply.photonView.IsMine)
        {
            ply.OnKilledEnemy += OnKilledEnemy;
        }
    }

    public override void PassiveAbilityUpdate (InputPayload input)
    {
    }

    public override void ActiveAbilityStart ()
    {
        Debug.Log("YAKINDAKÝ HERKESE CAN VERÝYORUM!");

#if UNITY_EDITOR

        for (int i = 0; i < 360; i += 10)
        {
            Vector3 angledRay = Quaternion.AngleAxis(i, Vector3.up) * Vector3.forward;
            Debug.DrawRay(ply.transform.position, angledRay * ultimateHealRange, Color.magenta, 10f);
        }

#endif
        Collider[] cols = Physics.OverlapSphere(ply.transform.position, ultimateHealRange, LayerManager.OnlyPlayers);

        foreach (var item in cols)
        {
            Debug.Log(item.name);
        }

        foreach (PlayerController targetPly in Array.ConvertAll(cols, delegate(Collider col) { return col.GetComponent<PlayerController>(); }))
        {
            Debug.Log($"Yakýnda {targetPly.ClassHandler.CurrentClass} bulundu, ve {ultimateHealAmount} can ekleniyor!");
            targetPly.photonView.RPC(nameof(targetPly.AddHealthRpc), Photon.Pun.RpcTarget.All, ultimateHealAmount);
        }

        ActiveAbilityEnd();
    }

    private void OnKilledEnemy (EnemyAI ai)
    {
        if(UnityEngine.Random.Range(0,101) < 20)
        {
            ply.Health += healthAdditionPerRandomKill;
        }
    }

    public override void ActiveAbilityUpdate (InputPayload input)
    {
    }
}
