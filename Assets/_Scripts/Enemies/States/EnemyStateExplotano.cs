using System;
using System.Collections;
using UnityEngine;

[System.Serializable]
public class EnemyStateExplotano : EnemyStateBase
{
    [SerializeField] private SkinnedMeshRenderer explotanoRenderer;
    [SerializeField] private int matIndex = 0;
    [SerializeField] private ParticleSystem explotanoParticle;
    [SerializeField] private float preAttackMoveSpeed = 3f;
    [SerializeField] private float preAttackRotateSpeed = 5f;
    [SerializeField] private float preAttackTime;
    [Space]
    [SerializeField] private float explotanoMinDamage = 100;
    [SerializeField] private float explotanoMaxDamage = 100;
    [SerializeField] private float range = 5;

    private readonly int explosionHash = Shader.PropertyToID("_ExplosionAmount");

    private Transform target;
    private readonly Collider[] cols = new Collider[24];

    public override bool NonCurretStateUpdate => false;

    public void SetTarget(Transform target)
    {
        this.target = target;
    }

    public IEnumerator ExplotanoRoutine()
    {
        float timePassed = 0f;
        while (timePassed < preAttackTime)
        {
            if (preAttackRotateSpeed != 0 && target != null)
            {
                Vector3 towardsTarget = target.position - ai.transform.position;
                towardsTarget.y = 0f;
                towardsTarget = towardsTarget.normalized;

                Debug.DrawRay(ai.transform.position, towardsTarget, Color.cyan, 10);

                ai.transform.rotation = Quaternion.Slerp(ai.transform.rotation, Quaternion.LookRotation(towardsTarget), Time.deltaTime * preAttackRotateSpeed);
            }

            ai.Move(ai.transform.forward * (preAttackMoveSpeed - timePassed));

            timePassed += Time.deltaTime;
            yield return null;
        }

        Explotano();
        ai.SetCurrentRoutineAsCompleted();

        ai.TakeDamageRpc(null, int.MaxValue, DamageType.Silent);
    }


    private void Explotano()
    {
        int hits = Physics.OverlapSphereNonAlloc(ai.transform.position, range, cols, LayerManager.EnemyHitLayer);

        for (int i = 0; i < hits; i++)
        {
            if (!cols[i].TryGetComponent<IDamagable>(out var dmg))
                continue;

            float distanceToExplosion = Vector3.Distance(dmg.ObjectTransform.position, ai.transform.position);

            int damageCalculated = Mathf.RoundToInt(Mathf.Lerp(explotanoMaxDamage, explotanoMinDamage, Mathf.InverseLerp(0,range, distanceToExplosion)));

            if (damageCalculated == 0)
                continue;

            dmg.PhotonView.RPC(nameof(dmg.TakeDamageRpc), dmg.PhotonView.Owner, null, damageCalculated, DamageType.Explosion);
        }

    }

    public IEnumerator ExplotanoEffects()
    {
        var mat = explotanoRenderer.materials[matIndex];
        float timePassed = 0f;
        while (timePassed < 1)
        {
            mat.SetFloat(explosionHash, timePassed);

            timePassed += Time.deltaTime / (preAttackTime - 0.1f);
            yield return null;
        }

        SceneLoadedHandler.GetSceneAs<GameScene>().WorldExplosionEvent?.Invoke(ai.transform.position, .5f);

        explotanoParticle.Play();
    }

    public override bool OnStateEnter()
    {
        ai.SetCurrentRoutine(ExplotanoRoutine());
        return true;
    }

    public override bool OnStateExit()
    {
        return true;
    }

    public override void OnStateUpdateNormal()
    {
    }

    public override void NonCurrentStateUpdate()
    {
    }

    public override void OnStateUpdateLateNormal()
    {
    }

    public override void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(ai.transform.position, range);
    }
}
