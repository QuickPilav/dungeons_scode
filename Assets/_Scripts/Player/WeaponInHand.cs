using System.Collections;
using UnityEngine;

public enum FireModes
{
    Auto,
    Single
}

public class WeaponInHand : ItemInHand
{
    private const float PEKSEMET_RELOAD_MULTIPLIER = .1f; //range (0f,1f)

    private const float LASER_DISTANCE = 50f;

    public int Damage { get => damage; }
    public int BulletsPerMag { get => bulletsPerMag; }
    public int PelletAmount { get => pelletAmount; }
    public Transform ShootPoint { get => shootPoint; }
    public int PenetrationAmonut { get => penetrationAmount; }
    public FireModes FireMode { get => fireMode; }
    public float FireRate { get => fireRate; }
    public float Spread { get => spread; }
    public float CriticalMultiplier { get => criticalMultiplier; }

    [SerializeField] private bool useLaser;
    [SerializeField] private FireModes fireMode;
    [SerializeField] private float fireRate = .25f;
    [SerializeField] private int damage = 7;
    [SerializeField] private int bulletsPerMag = 12;
    [SerializeField] private int pelletAmount = 1;
    [SerializeField] private float spread = 1f;
    [SerializeField] private float shakeAmount = .3f;
    [Space]
    [SerializeField] private float reloadDuration = 1.4f;
    [Space]
    [SerializeField] private Transform shootPoint;
    [SerializeField] private ParticleSystem shootParticle;
    [Space]
    [SerializeField] private AudioClip[] fireClips;
    [SerializeField] private AudioClip[] fireOthersClip;
    [SerializeField] private AudioClip reloadClip;
    [SerializeField] private int penetrationAmount = 1;
    [SerializeField, Range(0f, 1f)] private float criticalMultiplier = .2f;
    [SerializeField] private DamageType dmgType = DamageType.Bullet;

    private int lastFireSound;

    private WaitForSeconds reloadWaiter;
    private WaitForSeconds peksemetReloadWaiter;
    private Optional<IEnumerator> reloadRoutine;
    private float reloadTimer;
    private bool peksemetActivated;

    private LineRenderer plyLaser;


    public override void Initialize (PlayerController ply, ItemSystem ws, InventorySystem inv, ItemSystem.Items itemIndex)
    {
        base.Initialize(ply, ws, inv, itemIndex);

        plyLaser = ply.LaserTransform;

        reloadWaiter = new WaitForSeconds(reloadDuration);
    }
    public override void OnClassInitialized (PlayerClassBase currentClass)
    {
        base.OnClassInitialized(currentClass);

        float equation = reloadDuration;

        if (currentClass is PlayerClassVLonderz londerz)
        {
            equation *= (1 - londerz.ReloadMultiplier);
            reloadWaiter = new WaitForSeconds(equation);
            Debug.Log($"Reload multiplier: {londerz.ReloadMultiplier}, {reloadDuration} -> {equation}");
        }

        equation *= (1 - PEKSEMET_RELOAD_MULTIPLIER);
        peksemetReloadWaiter = new WaitForSeconds(equation);
    }
    public override void OnEquipped()
    {
        base.OnEquipped();
    }
    public override void OnDisequipped ()
    {
        base.OnDisequipped();
        StopReload();

        plyLaser.SetPosition(0,Vector3.down);
        plyLaser.SetPosition(1,Vector3.down);
    }
    public override void OnDropped ()
    {
        base.OnDropped();
        StopReload();
    }
    public override void Fire ()
    {
        if (ws == null)
            return;

        ply.AnimationState.Play(ply.AnimationState.GetAnimationHash((int)PlayerController.Player_Anims.GunShoot), 1);
        
        if(shootParticle != null)
        {
            shootParticle.Play();
        }
        
        if (ws.IsMine)
        {
            inv.CurrentSlot.CurrentBullets--;
            CameraSystem.Instance.ShakeOnce(shakeAmount);
            aSource.PlayOneShot(fireClips[lastFireSound]);
            ws.FireFromWeapon(this, dmgType);
        }
        else
        {
            aSource.PlayOneShot(fireOthersClip[lastFireSound]);
        }

        lastFireSound = (lastFireSound + 1) % fireClips.Length;
    }
    public void StopReload ()
    {
        if (!reloadRoutine.Enabled)
            return;
        
        aSource.Stop();
        aSource.clip = null;
        StopCoroutine(reloadRoutine.Value);
        ws.IsReloading = false;
    }
    private void Update()
    {
        if(!isActive)
            return;

        if (!useLaser)
            return;

        Vector3 pos;

        float dist;
        if(Physics.Raycast(shootPoint.position,shootPoint.forward,out RaycastHit hit, LASER_DISTANCE, LayerManager.HitLayer, QueryTriggerInteraction.Ignore))
        {
            dist = hit.distance;
            pos = hit.point;
        }
        else
        {
            dist = LASER_DISTANCE;
            pos = shootPoint.position + shootPoint.forward * LASER_DISTANCE;
        }

        pos += Vector3.down * .5f;

        plyLaser.sharedMaterial.SetFloat("_Distance",dist);

        plyLaser.SetPosition(0, shootPoint.position + Vector3.down * .5f);
        plyLaser.SetPosition(1, pos);

    }
    public override void UpdateOwner (InputPayload input)
    {
        base.UpdateOwner(input);
        reloadTimer += Time.deltaTime;
    }
    public void Reload ()
    {
        if (ws == null)
            return;

        aSource.clip = reloadClip;
        aSource.Play();

        if (reloadTimer >= 5f)
        {
            ply.PlayDialogue(PlayerClassBase.CharacterSoundIndex.Reloading);
            reloadTimer = 0f;
        }

        if (ws.IsMine)
        {
            reloadRoutine.Value = ReloadRoutine();
            StartCoroutine(reloadRoutine.Value);
        }
    }
    public void ActivatePeksemet (bool state)
    {
        peksemetActivated = state;
    }
    private IEnumerator ReloadRoutine ()
    {
        ws.IsReloading = true;

        yield return peksemetActivated ? peksemetReloadWaiter : reloadWaiter;

        int bulletsToLoad = bulletsPerMag - inv.CurrentSlot.CurrentBullets;
        int bulletsToDeduct = (inv.CurrentSlot.BulletsLeft >= bulletsToLoad) ? bulletsToLoad : inv.CurrentSlot.BulletsLeft;

        inv.CurrentSlot.BulletsLeft -= bulletsToDeduct;
        inv.CurrentSlot.CurrentBullets += bulletsToDeduct;

        ws.IsReloading = false;
        reloadRoutine.Value = null;
    }
}
