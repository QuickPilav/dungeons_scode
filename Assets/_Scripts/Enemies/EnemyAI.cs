using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public enum DamageType
{
    Bullet,
    Enemy,
    Fire,
    Explosion,
    Katana,
    Critical,
    Toxicity,
    Silent
}

public interface IDamagable
{
    public bool IsInvisible { get; }
    public List<SmokeGrenadeProjectile> SmokeInflictors { get; }
    public ObservableList<ToxicGas> ToxicInflictors { get; }
    public bool AffectedByToxicity { get; }
    public bool IsPlayer { get; }
    public int Health { get; }
    public bool IsDead { get; }
    public Transform ObjectTransform { get; }
    public Action<IDamagable> OnDeath { get; set; }
    public PhotonView PhotonView { get; }
    public SurfaceMaterial SurfaceMaterial { get; }

    public void TakeDamageRpc (Player inflictor, int amount, DamageType damageType);
}

public abstract class EnemyAI : MonoBehaviourPunCallbacks, IDamagable
{
    private bool initialized;

    protected Optional<EnemyStateBase> currentState;

    public abstract Enemies EnemyType { get; }
    public abstract int DefaultHealth { get; }

    private const int MAX_THINK_COUNTER = 512;

    protected virtual bool onlyTargetPlayers => false;

    public int Health { get; set; }

    public Transform ObjectTransform => transform;

    public PhotonView PhotonView { get => photonView; }

    public bool IsDead { get; private set; }

    public IDamagable LastEnemy { get; private set; }
    public Action<IDamagable> OnDeath { get; set; }

    public bool IsPlayer => false;

    public bool IsInvisible => false;

    public List<SmokeGrenadeProjectile> SmokeInflictors => smokeInflictors;

    public virtual bool AffectedByToxicity => false;
    public ObservableList<ToxicGas> ToxicInflictors => toxicInflictors;

    private readonly ObservableList<ToxicGas> toxicInflictors = new ObservableList<ToxicGas>();
    private readonly List<SmokeGrenadeProjectile> smokeInflictors = new List<SmokeGrenadeProjectile>();

    private CharacterController cc;
    private Collider[] colliders;

    public bool NotAffectedByGravity;

    [SerializeField] private EffectWithSound fireEffect;
    [SerializeField] protected Transform meshHolder;
    [SerializeField] private GameObject antiPlayerGetOnTop;
    [Space]
    [SerializeField] protected float thinkInterval;
    [SerializeField] private int gemDrops = 5;
    [SerializeField] private int abilityPointsGives = 5;

    protected int thinkCounter;
    private float toxicTimer;
    protected virtual float ToxicInflictionRate { get; } = 1.666f;
    protected virtual int ToxicInflictionDamage { get; } = 5;

    public SurfaceMaterial SurfaceMaterial => SurfaceMaterial.Human;


    private float thinkTimer;
    private Optional<IEnumerator> currentRoutine;

    protected List<EnemyStateBase> nonCurrentStateUpdates;

    private void Awake ()
    {
        cc = GetComponent<CharacterController>();
        colliders = new Collider[12];

        nonCurrentStateUpdates = new List<EnemyStateBase>();

        thinkCounter = UnityEngine.Random.Range(0,100);

        toxicTimer = UnityEngine.Random.Range(0f,ToxicInflictionRate);

        Initialize();
    }

    protected virtual void Initialize ()
    {
        initialized = true;

        OnSpawned();
    }

    private void OnToxicAdded (ObservableList<ToxicGas> sender, ListChangedEventArgs<ToxicGas> e)
    {
        InGameUI.Instance.UpdateToxicState(true);
    }

    private void OnToxicRemoved (ObservableList<ToxicGas> sender, ListChangedEventArgs<ToxicGas> e)
    {
        bool stillInToxic = sender.Count > 0;
        InGameUI.Instance.UpdateToxicState(stillInToxic);
        if (!stillInToxic)
        {
            toxicTimer = 0;
        }
    }

    private void Update ()
    {
        if (!initialized)
            return;

        if (IsDead)
            return;

        bool currentStateEnabled = currentState.Enabled;

        OnUpdateNormal();
        if (currentStateEnabled)
        {
            currentState.Value.OnStateUpdateNormal();
        }

        for (int i = 0; i < nonCurrentStateUpdates.Count; i++)
        {
            nonCurrentStateUpdates[i].NonCurrentStateUpdate();
        }

        if (!PhotonNetwork.IsMasterClient)
            return;

        if(transform.position.y < -30)
        {
            TakeDamageRpc(null, int.MaxValue, DamageType.Silent);
            return;
        }

        bool thinkThisFrame = thinkTimer > thinkInterval;

        thinkTimer += Time.deltaTime;
        if (thinkThisFrame)
        {
            thinkTimer -= thinkInterval;
        }

        if (!NotAffectedByGravity)
        {
            Move(Physics.gravity);
        }


        OnUpdateMaster();
        if (thinkThisFrame)
        {
            Think();
            thinkCounter = (thinkCounter + 1) % MAX_THINK_COUNTER;
        }

        if (currentStateEnabled)
        {
            currentState.Value.OnStateUpdateLateNormal();
        }

        OnAfterUpdate();
        if (thinkThisFrame)
        {
            ThinkAfter();
        }

        if(AffectedByToxicity)
        {
            HandleToxicity();
        }
    }

    private void HandleToxicity ()
    {
        if (ToxicInflictors.Count == 0)
            return;

        toxicTimer += Time.deltaTime;

        if (toxicTimer >= ToxicInflictionRate)
        {
            photonView.RPC(nameof(TakeDamageRpc), RpcTarget.MasterClient, null, ToxicInflictionDamage, DamageType.Toxicity);

            toxicTimer -= ToxicInflictionRate;
        }
    }

    public bool SetCurrentState (EnemyStateBase value)
    {
        if (value == currentState.Value)
            return false;

        if (currentState.Enabled)
        {
            if (!currentState.Value.OnStateExit())
            {
                return false;
            }
        }

        if (value == null)
        {
            Debug.LogWarning("State null seçildi!");
            currentState.Value = null;
            return true;
        }

        if (!value.OnStateEnter())
        {
            return false;
        }

        currentState.Value = value;

        return true;
    }

    protected virtual void OnUpdateNormal ()
    {

    }
    protected virtual void OnUpdateMaster ()
    {

    }

    protected virtual void OnAfterUpdate ()
    {

    }

    protected virtual void Think ()
    {

    }

    protected virtual void ThinkAfter ()
    {

    }

    public override void OnMasterClientSwitched (Player newMasterClient)
    {

    }

    //runs on master client
    [PunRPC]
    public virtual void TakeDamageRpc (Player inflictor, int amount, DamageType damageType)
    {
        if (IsDead)
            return;

        if (damageType != DamageType.Silent)
        {
            photonView.RPC(nameof(SpawnDamagePopupRpc), RpcTarget.All, amount, damageType);
        }

        Health -= amount;

        if (Health <= 0)
        {
            OnDeath?.Invoke(this);

            bool isInflictorNotNull = inflictor != null;

            int gemMultiplier = isInflictorNotNull ? (inflictor.TagObject as PlayerController).GemMultiplier : 1;

            if(damageType != DamageType.Toxicity && damageType != DamageType.Silent)
            {
                SpawnManager.Instance.SpawnDroppedGem(gemDrops * gemMultiplier, transform.position);
            }

            if (isOnFire)
            {
                photonView.RPC(nameof(SetFireStateRpc), RpcTarget.All, false);
            }

            photonView.RPC(nameof(DieRpc), RpcTarget.All, inflictor, damageType);

            if(isInflictorNotNull)
            {
                var ply = inflictor.TagObject as PlayerController;
                if (ply.ClassHandler.CurrentClass is not PlayerClassSissy)
                {
                    ply.photonView.RPC(nameof(ply.AddAbilityBarRpc), inflictor, abilityPointsGives);
                }
            }
            
        }
    }

    [PunRPC]
    public void SpawnDamagePopupRpc (int damage, DamageType damageType)
    {
        SpawnManager.Instance.SpawnDamagePopup(transform.position, damage, damageType);
    }

    //runs on all
    [PunRPC]
    public virtual void DieRpc (Player inflictor, DamageType reasonOfDeath)
    {
        if (inflictor != null && PhotonNetwork.LocalPlayer.ActorNumber == inflictor.ActorNumber)
        {
            PlayerController.ClientInstance.Value.OnKilledEnemy?.Invoke(this);
        }

        StopAllCoroutines();

        if (currentRoutine.Enabled)
        {
            StopCoroutine(currentRoutine.Value);
            currentRoutine.Value = null;
        }

        antiPlayerGetOnTop.SetActive(false);

        IsDead = true;
        cc.enabled = false;
        Despawn();
    }

    protected virtual void OnSpawned ()
    {
        Health = DefaultHealth;
    }

    public void Move (Vector3 dir)
    {
        cc.Move(dir * Time.deltaTime);
    }

    public bool FindEnemies (bool findPlayer, float range, out List<IDamagable> damagables)
    {
        damagables = new List<IDamagable>();
        IDamagable myDmg = this;

        int colsLength = Physics.OverlapSphereNonAlloc(transform.position, range, colliders, findPlayer ? LayerManager.EnemyHitLayer : LayerManager.HitLayer);
        for (int i = 0; i < colsLength; i++)
        {
            if (colliders[i] == null)
                break;

            IDamagable dmg = colliders[i].GetComponentInParent<IDamagable>();
            if (dmg != null)
            {
                if (dmg.IsDead || dmg == myDmg || dmg.IsInvisible || dmg.SmokeInflictors.Count != 0)
                    continue;

                if (onlyTargetPlayers && !dmg.IsPlayer)
                    continue;

                damagables.Add(dmg);
            }
        }

        if (damagables.Count == 0)
            return false;

        return true;
    }

    public IDamagable GetClosestEnemy (bool findPlayer, float range)
    {
        if (FindEnemies(findPlayer, range, out List<IDamagable> damagables))
        {
            return damagables.OrderBy((d) => (d.ObjectTransform.position - transform.position).sqrMagnitude).First();
        }
        return null;
    }

    protected virtual void OnDrawGizmos ()
    {
        if (currentState.Enabled)
        {
            currentState.Value.OnDrawGizmos();
        }
    }


    [PunRPC]
    public virtual void SetSpeedParameterRPC (float normalizedSpeed) { }

    [PunRPC]
    public virtual void PlayAnimationRPC (int animHash) { }

    private void Despawn ()
    {
        StartCoroutine(enumerator());

        IEnumerator enumerator ()
        {
            yield return new WaitForSeconds(2f);

            float timePassed = 0f;

            Vector3 startPos = meshHolder.position;
            Vector3 endPos = startPos + Vector3.down * 2f;

            while (timePassed < 1f)
            {
                timePassed += Time.deltaTime;
                meshHolder.position = Vector3.Lerp(startPos, endPos, timePassed);
                yield return null;
            }

            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.Destroy(gameObject);
            }
        }
    }

    public void SetCurrentRoutine (IEnumerator routine)
    {
        if (currentRoutine.Enabled)
        {
            StopCoroutine(currentRoutine.Value);
        }

        currentRoutine.Value = routine;

        StartCoroutine(routine);
    }

    public void SetCurrentRoutineAsCompleted ()
    {
        currentRoutine.Value = null;
    }

    public void RegisterNonCurrentStateUpdate (EnemyStateBase state)
    {
        nonCurrentStateUpdates.Add(state);
    }

    private Optional<IEnumerator> fireRoutine;

    public void SetOnFire (Player inflictor, int damagePerTick, float damageRate)
    {
        if (isOnFire)
            return;

        photonView.RPC(nameof(SetFireStateRpc), RpcTarget.All, true);
        if (PhotonNetwork.IsMasterClient)
        {
            SetOnFireRpc(inflictor, damagePerTick, damageRate);
        }
        else
        {
            photonView.RPC(nameof(SetOnFireRpc), RpcTarget.MasterClient, inflictor, damagePerTick, damageRate);
        }
    }

    private bool isOnFire;

    [PunRPC]
    protected void SetOnFireRpc (Player inflictor, int damagePerTick, float damageRate)
    {
        fireTickWaiter = new WaitForSeconds(damageRate);
        StartCoroutine(FireRoutine(inflictor, damagePerTick));
    }

    [PunRPC]
    protected void SetFireStateRpc (bool state)
    {
        isOnFire = state;
        if (state)
        {
            fireEffect.Play();
        }
        else
        {
            fireEffect.Stop();
        }
    }

    private WaitForSeconds fireTickWaiter;

    protected IEnumerator FireRoutine (Player inflictor, int damagePerTick)
    {
        while (isOnFire)
        {
            TakeDamageRpc(inflictor, damagePerTick, DamageType.Fire);
            yield return fireTickWaiter;
        }
    }

    public virtual void SetTarget (Transform transform)
    {

    }
}
