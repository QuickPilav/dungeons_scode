using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyStateAttacking : EnemyStateBase
{
    public event Action OnAttackStarted;
    public event Action OnAttackEnded;

    public event Action OnPreAttackStarted;
    public event Action OnPreAttackEnded;


    [SerializeField] private float cooldown = 2f;
    [Space]
    [SerializeField] private float preAttackMoveSpeed = 3f;
    [SerializeField] private float preAttackRotateSpeed = 5f;
    [SerializeField] private float preAttackTime;
    [Space]
    [SerializeField] private float attackMoveSpeed = 7f;
    [SerializeField] private float attackRotateSpeed = 5f;
    [SerializeField] private float attackTime;
    [Space]
    [SerializeField] private Collider[] hitboxes;
    [SerializeField, Range(0f, 1f)] private float hitboxNormalizedActivationMin;
    [SerializeField, Range(0f, 1f)] private float hitboxNormalizedActivationMax;

    private Transform target;
    private float cooldownTimer = 0f;

    public override bool NonCurretStateUpdate => true;

    public override void Initialize (EnemyAI ai)
    {
        base.Initialize(ai);
        for (int i = 0; i < hitboxes.Length; i++)
        {
            hitboxes[i].gameObject.SetActive(false);
        }
    }

    public void SetTarget (Transform target)
    {
        this.target = target;
    }

    public IEnumerator AttackRoutine ()
    {
        OnPreAttackStarted?.Invoke();

        float timePassed = 0f;
        while (timePassed < preAttackTime)
        {
            if(preAttackRotateSpeed != 0 && target != null)
            {
                Vector3 towardsTarget = target.position - ai.transform.position;
                towardsTarget.y = 0f;
                towardsTarget = towardsTarget.normalized;

                Debug.DrawRay(ai.transform.position,towardsTarget,Color.cyan,10);

                ai.transform.rotation = Quaternion.Slerp(ai.transform.rotation, Quaternion.LookRotation(towardsTarget), Time.deltaTime * preAttackRotateSpeed);
            }

            ai.Move(ai.transform.forward * preAttackMoveSpeed);

            timePassed += Time.deltaTime;
            yield return null;
        }

        OnPreAttackEnded?.Invoke();
        timePassed = 0f;

        OnAttackStarted?.Invoke();
        bool hitboxesActivated = false;

        for (int i = 0; i < hitboxes.Length; i++)
        {
            hitboxes[i].gameObject.SetActive(false);
        }

        while (timePassed < attackTime)
        {
            float normalizedTime = Mathf.InverseLerp(0f,attackTime,timePassed);
            if (attackRotateSpeed != 0 && target != null)
            {
                Vector3 towardsTarget = target.position - ai.transform.position;
                towardsTarget.y = 0f;
                towardsTarget = towardsTarget.normalized;

                ai.transform.rotation = Quaternion.Slerp(ai.transform.rotation, Quaternion.LookRotation(towardsTarget), Time.deltaTime * attackRotateSpeed);
            }


            bool hitboxesShouldBeActivated = normalizedTime > hitboxNormalizedActivationMin && normalizedTime < hitboxNormalizedActivationMax;

            if(hitboxesShouldBeActivated != hitboxesActivated)
            {
                for (int i = 0; i < hitboxes.Length; i++)
                {
                    hitboxes[i].gameObject.SetActive(hitboxesShouldBeActivated);
                }
            }

            ai.Move(ai.transform.forward * attackMoveSpeed);

            timePassed += Time.deltaTime;
            hitboxesActivated = hitboxesShouldBeActivated;
            yield return null;
        }

        for (int i = 0; i < hitboxes.Length; i++)
        {
            hitboxes[i].gameObject.SetActive(false);
        }

        OnAttackEnded?.Invoke();

        cooldownTimer = cooldown;

        ai.SetCurrentRoutineAsCompleted();
    }

    public override bool OnStateEnter ()
    {
        if(cooldownTimer > 0)
        {
            Debug.LogWarning("cooldwon bekleniyor hala!!!");
            return false;
        }
        ai.SetCurrentRoutine(AttackRoutine());
        return true;
    }

    public override bool OnStateExit ()
    {
        return true;
    }

    public override void OnStateUpdateNormal ()
    {
    }

    public void AddCooldown (float seconds)
    {
        cooldownTimer += seconds;
    }

    public override void NonCurrentStateUpdate ()
    {
        cooldownTimer = Mathf.Max(0,cooldownTimer - Time.deltaTime);
    }

    public override void OnStateUpdateLateNormal ()
    {
    }

    public override void OnDrawGizmos ()
    {
    }
}