using Photon.Pun;
using System.Collections;
using UnityEngine;

public enum Enemies
{
    Spider,
    Frog,
    Wasp,
    Snake,
    Slime
}

public class SpawnManager : MonoBehaviourPun
{
    [System.Serializable]
    public class EnemySpawn
    {
        [SerializeField] private EnemyAI enemyPrefab;
        [SerializeField] private Gib gibPrefab;

        public Gib GibPrefab => gibPrefab;
        public EnemyAI EnemyPrefab => enemyPrefab;

        //spider
        //empty
        //wasp
        //empty
        //slime
    }

    public static SpawnManager Instance;

    private void Awake()
    {
        Instance = this;
    }
    [SerializeField] private ImpactVfx impactPrefab;
    [SerializeField] private DroppedGem gemPrefab;
    [SerializeField] private DroppedItem droppedItemPrefab;
    [SerializeField] private SmokeGrenadeProjectile smokeGrenadeProjectilePrefab;
    [SerializeField] private HandGrenadeProjectile handGrenadeProjectilePrefab;
    [SerializeField] private ToxicGas posionPrefab;
    [Space]
    [SerializeField] private RandomPickup randomPickupPrefab;
    [SerializeField] private BulletProjectile bulletPrefab;
    [SerializeField] private PingObject pingPrefab;
    [SerializeField] private DamagePopup damagePopupPrefab;
    [SerializeField] private Transform damagePopupParent;
    [SerializeField] private Color criticalColor = Color.red;
    [SerializeField] private Color toxicColor = Color.white;
    [Space]
    [SerializeField] private EnemySpawn[] enemySpawns;

    private readonly WaitForSeconds poisonWaiter = new WaitForSeconds(5f);

    private void Start()
    {

        PoolManager.Instance.CreatePool(damagePopupPrefab.gameObject, 36, damagePopupParent);
        PoolManager.Instance.CreatePool(impactPrefab.gameObject, 48);

        foreach (var item in enemySpawns)
        {
            if (item.GibPrefab == null)
                continue;

            PoolManager.Instance.CreatePool(item.GibPrefab.gameObject, 12);
        }
    }

    /// <param name="criticalMultiplier">range(0f,1f)</param>
    public void SpawnBullet(PlayerController inflictor, int damage, DamageType damageType, Vector3 spawnPoint, Vector3 spawnForward, int penetrationAmount, float criticalMultiplier, bool isMamiUltimateBullet)
    {
        object[] data = new object[] { damageType, criticalMultiplier, isMamiUltimateBullet };

        GameObject bullet = PhotonNetwork.Instantiate(bulletPrefab.name, spawnPoint, Quaternion.LookRotation(spawnForward), data: data);

        bullet.GetComponent<BulletProjectile>().IInflictedIt(inflictor, damage, penetrationAmount);

        Destroy(bullet, 15f);
    }

    public void SpawnDroppedGem(int gemAmount, Vector3 pos)
    {
        object[] data = new object[] { gemAmount };

        PhotonNetwork.Instantiate(gemPrefab.name, pos, Quaternion.identity, data: data);
    }

    public void SpawnDroppedItem(Vector3 pos, Vector3 forward, ItemSystem.Items item, int currentBullets, int bulletsLeft)
    {
        object[] data = new object[] { item, currentBullets, bulletsLeft };

        var spawned = PhotonNetwork.Instantiate(droppedItemPrefab.name, pos, Quaternion.LookRotation(forward), data: data);
        spawned.GetPhotonView().TransferOwnership(PhotonNetwork.MasterClient);
    }

    public void SpawnImpact(Vector3 pos, Vector3 hitNormal, SurfaceMaterial surfaceMat)
    {
        if (surfaceMat == SurfaceMaterial.None)
            return;

        photonView.RPC(nameof(SpawnImpactRpc), RpcTarget.All, pos, hitNormal, (byte)surfaceMat);
    }

    public void SpawnPoison(Vector3 pos)
    {
        var spawned = PhotonNetwork.Instantiate(posionPrefab.name, pos, Quaternion.identity);

        StartCoroutine(enumerator());

        IEnumerator enumerator ()
        {
            yield return poisonWaiter;
            PhotonNetwork.Destroy(spawned);
        }
    }

    [PunRPC]
    private void SpawnImpactRpc(Vector3 pos, Vector3 hitNormal, byte surfaceMat)
    {
        var spawnedImpact = PoolManager.Instance.ReuseObject<ImpactVfx>(impactPrefab.gameObject, pos, Quaternion.LookRotation(hitNormal));
        spawnedImpact.Initialize((SurfaceMaterial)surfaceMat);
    }

    public void SpawnPing(Vector3 pos)
    {
        var spawnedPing = Instantiate(pingPrefab, pos + Vector3.up, Quaternion.identity);
        Destroy(spawnedPing.gameObject, 8.5f);
    }

    public T SpawnEnemy<T>(Enemies enemyType, Transform spawnPoint) where T : EnemyAI
    {
        var prefabToSpawn = enemySpawns[(int)enemyType].EnemyPrefab;
        /*
        Vector3 rand = Vector3.zero;//(Random.insideUnitSphere * 2);
        rand.y = 0;
        */
        var spawned = PhotonNetwork.Instantiate(prefabToSpawn.name, spawnPoint.position/* + rand*/, spawnPoint.rotation);

        return spawned.GetComponent<T>();
    }

    //public void SpawnDamagePopup (Vector3 pos, int damage, Color col = default)
    public void SpawnDamagePopup(Vector3 pos, int damage, DamageType damageType)
    {
        if (damageType == DamageType.Silent)
            return;

        Color col;
        float fontSizeMultiplier = 1f;



        switch (damageType)
        {
            case DamageType.Bullet:
                col = Color.white;
                break;
            case DamageType.Enemy:
                col = Color.red;
                break;
            case DamageType.Fire:
                ColorUtility.TryParseHtmlString("#D88514", out col);
                AudioSource.PlayClipAtPoint(SceneLoadedHandler.GetSceneAs<GameScene>().GetRandomFireClip(), pos, 1f);
                break;
            case DamageType.Explosion:
                ColorUtility.TryParseHtmlString("#FF5733", out col);
                fontSizeMultiplier *= 2f;
                break;
            case DamageType.Critical:
                col = criticalColor;
                fontSizeMultiplier *= 2f;
                break;
            case DamageType.Toxicity:

                col = toxicColor;
                fontSizeMultiplier *= 3f;

                break;
            default:
                col = Color.white;
                break;
        }
        Vector3 randomizer = UnityEngine.Random.insideUnitSphere * 0.5f;

        var popup = PoolManager.Instance.ReuseObject<DamagePopup>(damagePopupPrefab.gameObject, pos, Quaternion.identity);
        popup.Initialize(pos + randomizer, damage, col, fontSizeMultiplier);
    }

    public void SpawnSmokeGrenadeProjectile(Vector3 pos, Vector3 landPoint)
    {
        if (Physics.Raycast(landPoint, Vector3.down, out RaycastHit hit, 15, LayerManager.GroundLayer))
        {
            landPoint = hit.point;
        }

        object[] data = new object[] { landPoint };

        PhotonNetwork.Instantiate(smokeGrenadeProjectilePrefab.name, pos, Quaternion.identity, data: data);
    }

    public void SpawnHandGrenadeProjectile(Vector3 pos, Vector3 landPoint)
    {
        if (Physics.Raycast(landPoint, Vector3.down, out RaycastHit hit, 15, LayerManager.GroundLayer))
        {
            landPoint = hit.point;
        }

        object[] data = new object[] { landPoint };

        PhotonNetwork.Instantiate(handGrenadeProjectilePrefab.name, pos, Quaternion.identity, data: data);
    }

    public void SpawnGib(Vector3 pos, Enemies enemyType)
    {
        var gibReused = PoolManager.Instance.ReuseObject<Gib>(enemySpawns[(int)enemyType].GibPrefab.gameObject, pos, Quaternion.identity);
        gibReused.Initialize();
    }

    public void SpawnRandomPickup(Vector3 pos, RandomPickupType pickupType)
    {
        if (Physics.Raycast(pos + Vector3.up, Vector3.down, out RaycastHit hit, 15, LayerManager.GroundLayer))
        {
            pos = hit.point;
        }

        object[] data = new object[] { pickupType };

        PhotonNetwork.Instantiate(randomPickupPrefab.name, pos, Quaternion.identity, data: data);
    }
}
