using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSystem : MonoBehaviour
{
    public enum Items
    {
        None,
        Pistol,
        Deagle,
        Shotgun,
        Rifle,
        Sniper,

        Bandage,
        EnergyDrink,
        Battery,
        SmokeGrenade,
        MamiUlti,
        HandGrenade,
        Katana,
        IboAxe
    }

    public bool CanAction { get => !IsReloading && ply.CurrentState == ply.GroundedState && ply.GroundedState.CanMovePlayer && ply.GroundedState.CanRotatePlayer; }

    public bool IsReloading { get; set; }

    public bool IsMine { get; private set; }


    [SerializeField] private ItemInHand[] allItems;
    [SerializeField] private List<WeaponInHand> allWeapons;
    [HideInInspector] public ItemInHand currentItem;
    [HideInInspector] public Optional<WeaponInHand> currentWeapon;

    private PlayerController ply;
    private PhotonView pv;
    private InventorySystem inv;
    private float fireTimer;
    private int currentSlotIndex = -1;
    private bool lastFireInput;

    public void Initialize (PlayerController ply, InventorySystem inv, PhotonView pv)
    {
        this.ply = ply;
        this.pv = pv;
        this.inv = inv;

        IsMine = pv.IsMine;

        if (IsMine)
        {
            inv.OnSlotChanged += OnSlotChanged;
        }

        allWeapons = new List<WeaponInHand>();
        for (int i = 1; i < allItems.Length; i++)
        {
            allItems[i].Initialize(ply, this, inv, (Items)i);

            if (allItems[i] is WeaponInHand wih)
            {
                allWeapons.Add(wih);
            }
        }
        
        ply.OnRolled += OnRolled;
    }

    private void OnRolled ()
    {
        if (currentWeapon.Enabled)
        {
            currentWeapon.Value.StopReload();
        }
    }

    public ItemInHand GetItem (Items item)
    {
        return allItems[(int)item];
    }

    public void OnSlotChanged (InventorySlot newSlot)
    {
        if (currentItem != null)
        {
            currentItem.OnDisequipped();
        }

        currentItem = GetItem(newSlot.Item);
        currentSlotIndex = newSlot.SlotIndex;

        currentItem.OnEquipped();


        currentWeapon.Value = currentItem as WeaponInHand;
        fireTimer = -.5f;
    }

    public void UpdateByInput (InputPayload input)
    {
        if (currentWeapon.Enabled)
        {
            if (fireTimer < currentWeapon.Value.FireRate)
                fireTimer += Time.deltaTime;

            currentItem.UpdateOwner(input);

            if (input.reload && !IsReloading && inv.CurrentSlot.BulletsLeft > 0 && inv.CurrentSlot.CurrentBullets < currentWeapon.Value.BulletsPerMag && fireTimer >= currentWeapon.Value.FireRate && CanAction)
            {
                pv.RPC(nameof(ReloadWeaponRpc), RpcTarget.All, currentWeapon.Value.ItemIndex);
                fireTimer -= currentWeapon.Value.FireRate;
            }

            if (!currentWeapon.Value.customFireSystem)
            {
                bool fireInput = false;

                if (input.fire)
                {
                    if (currentWeapon.Value.FireMode == FireModes.Auto)
                    {
                        fireInput = true;
                    }
                    else
                    {
                        fireInput = !lastFireInput;
                    }
                }

                if (fireInput && fireTimer >= currentWeapon.Value.FireRate && CanAction)
                {
                    if (inv.CurrentSlot.CurrentBullets > 0)
                    {
                        pv.RPC(nameof(FireWeaponRpc), RpcTarget.All, currentWeapon.Value.ItemIndex);
                    }
                    else if (inv.CurrentSlot.BulletsLeft > 0 && !IsReloading)
                    {
                        pv.RPC(nameof(ReloadWeaponRpc), RpcTarget.All, currentWeapon.Value.ItemIndex);
                    }
                    else if (inv.CurrentSlot.CurrentBullets == 0 && inv.CurrentSlot.BulletsLeft == 0)
                    {
                        inv.DropItem(currentSlotIndex);
                        return;
                    }
                    fireTimer -= currentWeapon.Value.FireRate;
                }

                lastFireInput = input.fire;
            }
        }
        else if (currentItem != null)
        {
            currentItem.UpdateOwner(input);
            if (!currentItem.customFireSystem)
            {
                bool fireInput = false;

                if (input.fire)
                {
                    fireInput = !lastFireInput;
                }

                if (fireInput && CanAction)
                {
                    pv.RPC(nameof(FireWeaponRpc), RpcTarget.All, currentItem.ItemIndex);
                }

                lastFireInput = input.fire;
            }


        }

    }

    public void SetVisibility (Items weapon)
    {
        //weapons on hand
        for (int i = 1; i < allItems.Length; i++)
        {
            allItems[i].SetVisibility(false);
        }

        allItems[(int)weapon].SetVisibility(true);
    }

    public void FireFromWeapon (WeaponInHand weapon, DamageType dmgType)
    {
        Transform shootPoint = weapon.ShootPoint;


        int damagePerPellet = weapon.Damage / weapon.PelletAmount;

        for (int i = 0; i < weapon.PelletAmount; i++)
        {
            Vector3 forwardVector = Vector3.forward;
            float deviation = Random.Range(0f, weapon.Spread);
            float angle = Random.Range(0f, 360f);
            forwardVector = Quaternion.AngleAxis(deviation, Vector3.up) * forwardVector;
            forwardVector = Quaternion.AngleAxis(angle, Vector3.forward) * forwardVector;
            forwardVector = Quaternion.LookRotation(shootPoint.forward) * forwardVector;

            SpawnManager.Instance.SpawnBullet(ply, Mathf.RoundToInt(damagePerPellet * ply.DamageMultiplier), dmgType, shootPoint.position, forwardVector, weapon.PenetrationAmonut + ply.ClassHandler.CurrentClass.BulletPenetrationAdditive, weapon.CriticalMultiplier, weapon.ItemIndex == Items.MamiUlti);
        }
    }

    [PunRPC]
    public void FireWeaponRpc (Items itemIndex)
    {
        allItems[(int)itemIndex].Fire();
    }

    [PunRPC]
    public void ReloadWeaponRpc (Items itemIndex)
    {
        (allItems[(int)itemIndex] as WeaponInHand).Reload();
    }

    public void OnClassInitialized (PlayerClassBase value)
    {
        for (int i = 1; i < allItems.Length; i++)
        {
            allItems[i].OnClassInitialized(value);
        }
    }


    private Optional<IEnumerator> peksemetRoutine;

    public void OnPeksemetUsed ()
    {
        if(peksemetRoutine.Enabled)
        {
            Debug.LogError("PEKSEMET KULLANILDI");
            StopCoroutine(peksemetRoutine.Value);
        }

        peksemetRoutine.Value = enumerator();
        StartCoroutine(peksemetRoutine.Value);

        IEnumerator enumerator ()
        {
            Debug.Log("Peksemet active!");

            foreach (var item in allWeapons)
            {
                item.ActivatePeksemet(true);
            }

            yield return peksemetWaiter;

            Debug.Log("Peksemet deactive!");
            foreach (var item in allWeapons)
            {
                item.ActivatePeksemet(false);   
            }

            peksemetRoutine.Value = null;
        }
    }

    private WaitForSeconds peksemetWaiter = new WaitForSeconds(20);

    public void RemoveCurrentItem ()
    {
        inv.RemoveItem(currentSlotIndex);
    }
}
