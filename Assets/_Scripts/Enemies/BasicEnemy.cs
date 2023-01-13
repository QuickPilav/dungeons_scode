using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public abstract class BasicEnemy : EnemyAI
{
    public enum EnemyAnims
    {
        attacking,
        dying,
    }

    public enum EnemyParameters
    {
        speed,
    }

    public IDamagable Enemy
    {
        get => __enemy;
        set
        {
            __enemy = value;
            if (__enemy != null)
            {
                enemyStateWalking.SetTarget(Enemy.ObjectTransform);
            }
            else
            {
                enemyStateWalking.SetTarget(target: null);
            }

            enemyStateAttacking.AddCooldown(1);
        }
    }
    private IDamagable __enemy;

    [SerializeField] private float eyeRange = 15f;
    [SerializeField] private float attackDistance = 2f;
    [Space]
    [SerializeField] protected EnemyStateIdle enemyStateIdle;
    [SerializeField] protected EnemyStateWalking enemyStateWalking;
    [SerializeField] protected EnemyStateAttacking enemyStateAttacking;
    [SerializeField] protected EnemyStateAnimation enemyStateAnimation;
    [SerializeField] private float randomMovePositionMultiplier = 1f;

    public EnemyStateIdle EnemyStateIdle => enemyStateIdle;
    public EnemyStateWalking EnemyStateWalking => enemyStateWalking;
    public EnemyStateAttacking EnemyStateAttacking => enemyStateAttacking;
    public EnemyStateAnimation EnemyStateAnimation => enemyStateAnimation;

    private const int MOVE_INTERVAL = 5;
    private const int ENEMY_CHECK_THINK_INTERVAL = 5;

    protected abstract bool SpawnGibsOnExplosion { get; }

    protected override void Initialize()
    {
        base.Initialize();

        enemyStateIdle.Initialize(this);
        enemyStateWalking.Initialize(this);
        enemyStateAttacking.Initialize(this);
        enemyStateAnimation.Initialize(this);

        enemyStateAttacking.OnAttackEnded += EnemyStateAttacking_OnAttackEnded;
    }

    private void EnemyStateAttacking_OnAttackEnded()
    {
        StartCoroutine(enumerator());

        IEnumerator enumerator()
        {
            SetCurrentState(enemyStateIdle);
            yield return StartCoroutine(enemyStateIdle.IdleFor(1f));
            SetCurrentState(enemyStateWalking);
        }
    }

    protected override void OnSpawned()
    {
        base.OnSpawned();
        SetCurrentState(enemyStateWalking);
    }

    protected override void Think()
    {
        base.Think();

        bool checkEnemyExists = thinkCounter % ENEMY_CHECK_THINK_INTERVAL == 0;

        if (checkEnemyExists && SmokeInflictors.Count == 0)
        {
            Enemy = GetClosestEnemy(true, eyeRange);
        }

        //düþman var mý önce onu kontrol et
        if (Enemy == null || Enemy.IsDead || Enemy.ObjectTransform == null)
        {
            WanderBehaviour();
        }
        else
        {
            if (currentState.Value == enemyStateWalking)
            {
                //eðer düþmanýmýz bir anda görünmez olmuþ ise takibi býrak.
                if (Enemy.IsInvisible || SmokeInflictors.Count != 0 || Enemy.SmokeInflictors.Count != 0)
                {
                    Enemy = null;
                    return;
                }

                float distanceToEnemy = Vector3.Distance(Enemy.ObjectTransform.position, transform.position);

                //eðer düþmana çok yakýn isek
                if (distanceToEnemy < attackDistance)
                {
                    AttackBehaviour();
                }
            }

        }
    }

    protected override void OnUpdateNormal()
    {
        base.OnUpdateNormal();

        enemyStateAnimation.SetParameter(enemyStateAnimation.GetParameterHash((int)EnemyParameters.speed), enemyStateWalking.NormalizedLerpedSpeed);
    }

    public override void SetTarget(Transform transform)
    {
        base.SetTarget(transform);

        enemyStateWalking.SetTarget(transform.position);
    }

    private void WanderBehaviour()
    {
        if (!enemyStateWalking.IsStopped)
            return;

        if (thinkCounter % MOVE_INTERVAL == 0)
        {
            NavMesh.SamplePosition(transform.position + Random.insideUnitSphere * randomMovePositionMultiplier, out NavMeshHit hit, randomMovePositionMultiplier, 1);
            enemyStateWalking.SetTarget(hit.position);
            SetCurrentState(enemyStateWalking);
        }
    }

    protected virtual void AttackBehaviour()
    {
        if (!SetCurrentState(enemyStateAttacking))
            return;
        
        enemyStateAttacking.SetTarget(Enemy.ObjectTransform);
        photonView.RPC(nameof(PlayAnimationRPC), RpcTarget.All, enemyStateAnimation.GetAnimationHash((int)EnemyAnims.attacking));
    }

    [PunRPC]
    public override void SetSpeedParameterRPC(float normalizedSpeed)
    {
        enemyStateWalking.NormalizedSpeed = normalizedSpeed;
    }

    [PunRPC]
    public override void PlayAnimationRPC(int animHash)
    {
        enemyStateAnimation.Play(animHash);
    }

    [PunRPC]
    public override void DieRpc(Player inflictor, DamageType reasonOfDeath)
    {
        bool isExplosionDmg = reasonOfDeath == DamageType.Explosion;
        if (isExplosionDmg || reasonOfDeath == DamageType.Silent)
        {
            meshHolder.gameObject.SetActive(false);
        }

        if (isExplosionDmg && SpawnGibsOnExplosion)
        {
            SpawnManager.Instance.SpawnGib(transform.position, EnemyType);
        }
        else
        {
            SetCurrentState(enemyStateAnimation);
            StartCoroutine(enemyStateAnimation.Play(enemyStateAnimation.GetAnimationHash((int)EnemyAnims.dying), 1f));
        }
        base.DieRpc(inflictor, reasonOfDeath);
    }
    /*
    protected override void OnDrawGizmos ()
    {
        base.OnDrawGizmos();

        Gizmos.color = Color.white;

        Gizmos.DrawWireSphere(transform.position, eyeRange);
    }
    */
}
