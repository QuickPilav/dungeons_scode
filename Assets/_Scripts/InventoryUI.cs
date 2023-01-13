using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private InventorySlotUI slotPrefab;
    [SerializeField] private Transform slotsParent;
    [Space]
    [SerializeField] private RectTransform selectedItemShower;

    private Dictionary<int, InventorySlotUI> slotsUIs;

    public void Initialize (InventorySystem inv)
    {
        inv.OnSlotUpdate += Inv_OnSlotUpdate;
        inv.OnSlotChanged += Inv_OnSlotChanged;
        slotsUIs = new Dictionary<int, InventorySlotUI>();
    }

    private async void Inv_OnSlotChanged (InventorySlot slot)
    {
        if (InventorySystem.NotPickupableItems.Contains(slot.Item))
        {
            //ULTÝ VEYA DAHA ÜSTÜN BÝR ÞEYÝ ENVANTERDE AYRI GÖSTERECEÐÝZ, YA DA GÖSTERMEYECEÐÝZ.
            return;
        }
        await System.Threading.Tasks.Task.Delay(50);
        slotsUIs.TryGetValue(slot.SlotIndex, out var value);
        selectedItemShower.position = value.Rect.position;
    }

    private void Inv_OnSlotUpdate (InventorySlot slot, InventorySlotUpdateType updateType)
    {
        if (InventorySystem.NotPickupableItems.Contains(slot.Item))
        {
            //ULTÝ VEYA DAHA ÜSTÜN BÝR ÞEYÝ ENVANTERDE AYRI GÖSTERECEÐÝZ, YA DA GÖSTERMEYECEÐÝZ.
            return;
        }

        slotsUIs.TryGetValue(slot.SlotIndex, out var value);
        switch (updateType)
        {
            case InventorySlotUpdateType.Addition:

                var spawned = Instantiate(slotPrefab, slotsParent);
                spawned.SetItem(slot.Item.ToString(), slot.CurrentBullets);
                slotsUIs.Add(slot.SlotIndex, spawned);

                selectedItemShower.SetAsFirstSibling();

                break;

            case InventorySlotUpdateType.Update:

                value.SetItem(slot.Item.ToString(), slot.CurrentBullets);

                break;

            case InventorySlotUpdateType.Deletion:

                slotsUIs.Remove(slot.SlotIndex);
                Destroy(value.gameObject);
                selectedItemShower.SetAsFirstSibling();

                break;
        }

    }
}
