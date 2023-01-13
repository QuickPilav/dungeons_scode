using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum InventorySlotUpdateType
{
    Addition,
    Update,
    Deletion
}

public class InventorySystem : MonoBehaviourPun
{
    public static List<ItemSystem.Items> NotPickupableItems = new()
    {
        ItemSystem.Items.MamiUlti,
        ItemSystem.Items.HandGrenade,
        ItemSystem.Items.Katana,
        ItemSystem.Items.IboAxe
    };

    public event Action<InventorySlot, InventorySlotUpdateType> OnSlotUpdate;

    public event Action<InventorySlot> OnSlotChanged;

    public const int SLOT_AMOUNT = 4;

    private ItemSystem itemSystem;

    [SerializeField] private Dictionary<int, InventorySlot> slots;
    [SerializeField] private Dictionary<int, InventorySlot> slotsUnspecialItems;

    public InventorySlot CurrentSlot { get => slots[currentSlotIndex]; }
    private int currentSlotIndex = -1;
    private int lastAddedSlot;

    private readonly float switchRate = .25f;
    private float switchTimer;

    private PlayerController ply;


    public void Initialize (PlayerController ply, ItemSystem itemSystem)
    {
        this.ply = ply;
        this.itemSystem = itemSystem;

        slots = new Dictionary<int, InventorySlot>();
        slotsUnspecialItems = new Dictionary<int, InventorySlot>();

        if (photonView.IsMine)
        {
            InGameUI.Instance.GetComponent<InventoryUI>().Initialize(this);
        }

        /*
        AddItem(ItemSystem.Items.Rifle, 30, 90);
        AddItem(ItemSystem.Items.Pistol, 16,200);
        AddItem(ItemSystem.Items.Deagle, 16, 200);
        AddItem(ItemSystem.Items.Sniper, 16, 200);
        AddItem(ItemSystem.Items.Shotgun, 16, 200);
        */
        /*
        AddItem(ItemSystem.Items.Bandage);
        AddItem(ItemSystem.Items.EnergyDrink);
        AddItem(ItemSystem.Items.Battery);
        AddItem(ItemSystem.Items.SmokeGrenade);
        */

        AddItem(ItemSystem.Items.Pistol, out _, 12, int.MaxValue, isDroppable: false);

        SwitchSlot(slots.ElementAt(0).Key);
    }

    public void UpdateByInput (InputPayload input)
    {
        switchTimer += Time.deltaTime;
        if (input.itemSwitchRequest != -1 && switchTimer >= switchRate)
        {
            switchTimer = 0f;

            if (input.itemSwitchRequest < slotsUnspecialItems.Count)
            {
                SwitchSlot(slotsUnspecialItems.OrderBy(x => x.Key).ElementAt(input.itemSwitchRequest).Key);
                return;
            }
        }

        if (input.drop && slotsUnspecialItems.Count > 1)
        {
            DropItem(currentSlotIndex);
            return;
        }
    }

    public void SwitchSlot (int requestedSlotIndex)
    {
        if (requestedSlotIndex < 0 && requestedSlotIndex != -2)
            return;

        //Debug.Log($"gelen request {requestedSlotIndex}!");

        slots.TryGetValue(requestedSlotIndex, out var slot);

        if (slot.SlotIndex == currentSlotIndex || slot.Item == ItemSystem.Items.None)
        {
            if (requestedSlotIndex != -2)
            {
                return;
            }
        }
        if (currentSlotIndex != -1 && slots.ContainsKey(currentSlotIndex))
        {
            CurrentSlot.OnSlotDeselected();
        }

        currentSlotIndex = slot.SlotIndex;

        CurrentSlot.OnSlotSelected();

        if (requestedSlotIndex != -2)
        {
            OnSlotChanged?.Invoke(CurrentSlot);
        }

        itemSystem.SetVisibility(CurrentSlot.Item);
    }

    private int GetFirstEmptySlot (bool forceSlot)
    {
        if (forceSlot)
            return lastAddedSlot;

        for (int i = 0; i < slotsUnspecialItems.Count; i++)
        {
            if (slots.ElementAt(i).Value.Item == ItemSystem.Items.None)
            {
                Debug.Log("boþ slot bulundu!");
                return i;
            }

        }

        if (slotsUnspecialItems.Count < SLOT_AMOUNT)
        {
            Debug.Log("daha var olmamýþ slot bulundu!");
            return slotsUnspecialItems.Count;
        }

        Debug.Log("envanterde yer yok!");
        return -1;
    }

    public bool AddItem (ItemSystem.Items itemName, out InventorySlot slot, int currentBullets = -1, int bulletsLeft = -1, bool isDroppable = true)
    {
        int slotIndex = GetFirstEmptySlot(NotPickupableItems.Contains(itemName));

        if (slotIndex == -1)
        {
            slot = null;
            return false;
        }

        lastAddedSlot++;
        slot = AddItemToSlot(itemName, lastAddedSlot, currentBullets, bulletsLeft, isDroppable);
        return true;
    }

    public InventorySlot AddItemToSlot (ItemSystem.Items item, int slotIndex, int currentBullets, int bulletsLeft, bool isDroppable = true)
    {
        InventorySlot slot;
        if (currentBullets != -1)
        {
            slot = new WeaponSlot(slotIndex, item, currentBullets, bulletsLeft, isDroppable);
            slot.OnCurrentBulletsChanged += (slot, newAmmo) =>
            {
                OnSlotUpdate?.Invoke(slot, InventorySlotUpdateType.Update);
            };
            slots.Add(slotIndex, slot);
            if (!NotPickupableItems.Contains(slot.Item))
            {
                slotsUnspecialItems.Add(slotIndex, slot);
            }
        }
        else
        {
            slot = new InventorySlot(slotIndex, item, isDroppable);
            slots.Add(slotIndex, slot);
            if (!NotPickupableItems.Contains(slot.Item))
            {
                slotsUnspecialItems.Add(slotIndex, slot);
            }
        }

        OnSlotUpdate?.Invoke(slot, InventorySlotUpdateType.Addition);

        return slot;
    }

    public bool RemoveItem (int slotIndex)
    {
        if (!slots.TryGetValue(slotIndex, out var slot))
        {
            return false;
        }

        return RemoveItem(slot);
    }

    private bool RemoveItem (InventorySlot slot)
    {
        if (slot.SlotIndex == currentSlotIndex)
        {
            try
            {
                SwitchSlot(slotsUnspecialItems.Where(x => x.Key != currentSlotIndex).Last().Key);
            }
            catch (Exception)
            {
                AddItem(ItemSystem.Items.Pistol, out _, 12, 36);
                SwitchSlot(slotsUnspecialItems.Where(x => x.Key != currentSlotIndex).Last().Key);
            }
        }

        OnSlotUpdate?.Invoke(slot, InventorySlotUpdateType.Deletion);
        if (!NotPickupableItems.Contains(slot.Item))
        {
            slotsUnspecialItems.Remove(slot.SlotIndex);
        }
        return slots.Remove(slot.SlotIndex);
    }

    public bool DropItem (int slotIndex)
    {
        if (!slots.TryGetValue(slotIndex, out var slot))
        {
            return false;
        }

        return DropItem(slot);
    }

    public bool DropItem (InventorySlot slot)
    {
        if (!slot.IsDroppable)
            return false;

        if (itemSystem.currentItem != null && slot.Item == itemSystem.currentItem.ItemIndex)
        {
            itemSystem.currentItem.OnDropped();
        }

        SpawnManager.Instance.SpawnDroppedItem(ply.EyePosition, ply.transform.forward, slot.Item, slot.CurrentBullets, slot.BulletsLeft);
        return RemoveItem(slot);
    }

    public bool Contains (ItemSystem.Items itemType, out InventorySlot slot)
    {
        foreach (var item in slots)
        {
            if (item.Value.Item == itemType)
            {
                slot = item.Value;
                return true;
            }
        }
        slot = null;
        return false;
    }
}


[System.Serializable]
public class InventorySlot
{
    public Action<InventorySlot, int> OnCurrentBulletsChanged;
    public ItemSystem.Items Item { get => item; }
    [SerializeField] protected ItemSystem.Items item;

    public bool IsDroppable { get; private set; }

    public int SlotIndex { get => slotIndex; }
    private readonly int slotIndex;

    public virtual int CurrentBullets { get => currentBullets; set { } }
    public virtual int BulletsLeft { get => bulletsLeft; set { } }

    protected bool isSelectedSlot;

    protected int currentBullets = -1;
    protected int bulletsLeft = -1;

    public InventorySlot (int slotIndex, ItemSystem.Items item, bool isDroppable = true)
    {
        this.IsDroppable = isDroppable;
        this.slotIndex = slotIndex;
        this.item = item;
        IsDroppable = isDroppable;
    }

    public virtual void OnSlotSelected ()
    {
        isSelectedSlot = true;
    }

    public virtual void OnSlotDeselected ()
    {
        isSelectedSlot = false;
    }
}

public class WeaponSlot : InventorySlot
{
    public WeaponSlot (int slotIndex, ItemSystem.Items item, int currentBullets, int bulletsLeft, bool isDroppable = true) : base(slotIndex, item, isDroppable)
    {
        this.currentBullets = currentBullets;
        this.bulletsLeft = bulletsLeft;
    }

    public override int CurrentBullets
    {
        get => currentBullets;
        set
        {
            currentBullets = value;
            OnCurrentBulletsChanged?.Invoke(this, currentBullets);
            if (isSelectedSlot)
            {
                InGameUI.Instance.UpdateBullets(currentBullets, bulletsLeft);
            }
        }
    }

    public override int BulletsLeft
    {
        get => bulletsLeft;
        set
        {
            bulletsLeft = value;
        }
    }

    public override void OnSlotSelected ()
    {
        base.OnSlotSelected();
        InGameUI.Instance.UpdateBullets(currentBullets, bulletsLeft);
    }
}