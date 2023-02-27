using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ProtectPoint : MonoBehaviourPun, IDamagable
{
#if UNITY_EDITOR
    private const int DEFAULT_HEALTH = 300;
#else
    private const int DEFAULT_HEALTH = 500;
#endif

    private readonly WaitForSeconds damageCooldownWaiter = new WaitForSeconds(.1f);

    public bool IsInvisible => !isCurrentProtectionPoint;

    public List<SmokeGrenadeProjectile> SmokeInflictors => inflictors;

    private readonly List<SmokeGrenadeProjectile> inflictors = new List<SmokeGrenadeProjectile>();

    public bool IsPlayer => false;
    public SurfaceMaterial SurfaceMaterial
    {
        get
        {
            if(IsDead)
            {
                return SurfaceMaterial.None;
            }
            return SurfaceMaterial.Metal;
        }
    }
    public int Health
    {
        get => health;
        set
        {
            health = Mathf.Clamp(value, 0, DEFAULT_HEALTH);

            InGameUI.Instance.UpdateBossHealth(health);

            if (health == 0)
            {
                Explode();
            }
        }
    }

    private int health = DEFAULT_HEALTH;
    private bool isCurrentProtectionPoint;

    private bool cantTakeDamage;

    public bool IsDead { get; private set; }

    public Transform ObjectTransform => transform;

    public Action<IDamagable> OnDeath { get; set; }

    public bool AffectedByToxicity => false;
    public ObservableList<ToxicGas> ToxicInflictors => null;

    public PhotonView PhotonView => photonView;


    [SerializeField] private GameObject normalMesh;
    [SerializeField] private GameObject explodedMesh;
    [SerializeField] private ParticleSystem explosionEffect;

    private void Awake ()
    {
        normalMesh.SetActive(true);
        explodedMesh.SetActive(false);
    }

    public void SetAsCurrentProtectionPoint ()
    {
        Debug.Log($"{transform.name} açýldým!");
        isCurrentProtectionPoint = true;
        InGameUI.Instance.StartBossfight(Health, DEFAULT_HEALTH);
    }

    private void Explode ()
    {
        if (IsDead)
            return;

        isCurrentProtectionPoint = false;
        IsDead = true;
        OnDeath?.Invoke(this);

        explosionEffect.Play();

        InGameUI.Instance.StopBossfight();

        Destroy( GetComponent<Collider>() );

        normalMesh.SetActive(false);
        explodedMesh.SetActive(true);

        //ses oynat
        //efekt oynat


        if (!PhotonNetwork.IsMasterClient)
        {
            //Debug.Log("patladý ancak biz master client deðiliz");
            return;
        }

    }

    [PunRPC]
    public void TakeDamageRpc (Player inflictor, int amount, DamageType damageType)
    {
        //Debug.Log($"{amount} hasar geldi!");


        if (!isCurrentProtectionPoint || cantTakeDamage
#if !UNITY_EDITOR 
            || inflictor != null )
#else
            )
#endif
            return;

        cantTakeDamage = true;

        int newHealth = Health - amount;

        photonView.RPC(nameof(TakeDamageOnAllRpc), RpcTarget.AllViaServer, newHealth, damageType);

        if (newHealth > 0)
        {
            StartCoroutine(enumerator());

            IEnumerator enumerator ()
            {
                yield return damageCooldownWaiter;
                cantTakeDamage = false;
            }
        }
    }

    [PunRPC]
    private void TakeDamageOnAllRpc (int currentHealth, DamageType damageType)
    {
        this.Health = currentHealth;

        switch (damageType)
        {
            case DamageType.Bullet:
                break;
            case DamageType.Enemy:
                break;
            case DamageType.Fire:
                break;
            case DamageType.Explosion:
                break;
            case DamageType.Katana:
                break;
            case DamageType.Critical:
                break;
        }
    }
}
