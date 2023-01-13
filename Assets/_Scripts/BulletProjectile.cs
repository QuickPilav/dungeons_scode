using Photon.Pun;
using UnityEngine;

public class BulletProjectile : MonoBehaviour, IPunInstantiateMagicCallback
{
    private const float BULLET_SPEED = 25;
    private const int CRITICAL_MULTIPLIER = 3;

    [SerializeField] private GameObject bullet;
    [SerializeField] private GameObject pencil;

    private int damage;
    private Rigidbody rb;

    private void Awake ()
    {
        rb = GetComponent<Rigidbody>();

        lastPos = transform.position;
        rb.useGravity = false;
        rb.velocity = transform.forward * BULLET_SPEED;
    }

    private DamageType damageType = DamageType.Bullet;

    private int penetrationAmount;

    public void IInflictedIt (PlayerController inflictor, int damage, int penetrationAmount)
    {
        this.damage = damage;
        this.inflictor = inflictor;
        isProjectileMine = true;
        rayTimer = rayRate * .7f;
        this.penetrationAmount = penetrationAmount;
    }

    private PlayerController inflictor;
    private bool isProjectileMine;

    private Vector3 lastPos;

    private bool lastRay;

    private readonly float rayRate = .1f;
    private float rayTimer;
    private float criticalChance;

    private void Update ()
    {
        if (!isProjectileMine)
            return;

        rayTimer += Time.deltaTime;

        if (rayRate > rayTimer)
            return;

        rayTimer -= rayRate;

        Vector3 diff = transform.position - lastPos;

        lastRay = !lastRay;

        Debug.DrawRay(lastPos, diff, lastRay ? Color.green : Color.red, 5f);

        if (Physics.Raycast(lastPos, diff.normalized, out RaycastHit hit, diff.magnitude, LayerManager.HitLayer, QueryTriggerInteraction.Ignore))
        {
            var damagable = hit.transform.GetComponentInChildren<IDamagable>();

            if (damagable != null)
            {
                if(criticalChance != 0 && damageType == DamageType.Bullet && Random.Range(0f,1f) < criticalChance)
                {
                    damageType = DamageType.Critical;
                    damage *= CRITICAL_MULTIPLIER;
                }

                SpawnManager.Instance.SpawnImpact(hit.point, hit.normal, damagable.SurfaceMaterial);
                damagable.PhotonView.RPC(nameof(damagable.TakeDamageRpc), RpcTarget.MasterClient, inflictor.photonView.Controller, damage, damageType);
                
                if(damagable is EnemyAI enemy)
                {
                    inflictor.OnHitEnemy?.Invoke(enemy,damage);
                }

                penetrationAmount--;
                if (penetrationAmount < 0)
                {
                    PhotonNetwork.Destroy(gameObject);
                }
            }
            else
            {
                SpawnManager.Instance.SpawnImpact(hit.point, hit.normal, SurfaceMaterial.Concrete);
                PhotonNetwork.Destroy(gameObject);
            }

        }

        lastPos = transform.position;
    }

    public void OnPhotonInstantiate (PhotonMessageInfo info)
    {
        object[] data = info.photonView.InstantiationData;
        
        this.damageType = (DamageType)data[0];
        this.criticalChance = (float)data[1];

        bullet.SetActive(true);

        if ((bool)data[2])
            pencil.SetActive(true);
    }
}
