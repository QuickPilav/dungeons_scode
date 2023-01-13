using System;
using System.Collections;
using UnityEngine;

[System.Serializable]
public class PlayerClassVLonderz : PlayerClassBase
{
    public float ReloadMultiplier { get => reloadSpeedMultiplier; }

    [SerializeField, Range(0f, 1f)] private float reloadSpeedMultiplier = .4f;
    [Space]
    [SerializeField] private float invisibilityTime = 7f;
    [SerializeField, Range(0, 1f)] private float regainHealthPercentPerHit = .2f;

    private WaitForSeconds invisibilityWaiter;

    public override void Initialize (PlayerController ply, PlayerClassHandler classHandler)
    {
        base.Initialize(ply, classHandler);

        if (!ply.photonView.IsMine)
            return;


        invisibilityWaiter = new WaitForSeconds(invisibilityTime);
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
        if (!ply.photonView.IsMine)
            return;

        ply.OnHitEnemy += Ply_OnHitEnemy;
    }

    private void Ply_OnHitEnemy (EnemyAI ai,int damage)
    {
        if (!ply.IsInvisible)
            return;

        ply.Health += Mathf.FloorToInt(damage * regainHealthPercentPerHit);
    }

    public override void PassiveAbilityUpdate (InputPayload input)
    {
    }

    public override void ActiveAbilityStart ()
    {
        if(ply.IsInvisible)
            return;

        ActiveAbilityEnd();
        classHandler.CanNotSetAbilityBar = true;

        ply.IsInvisible = true;

        ply.StartCoroutine(enumerator());

        IEnumerator enumerator ()
        {
            yield return invisibilityWaiter;

            ply.IsInvisible = false;

            classHandler.CanNotSetAbilityBar = false;
        }
    }

    public override void ActiveAbilityUpdate (InputPayload input)
    {
    }
}
