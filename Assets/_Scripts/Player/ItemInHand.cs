using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ItemInHand : MonoBehaviour
{
    public virtual bool customFireSystem { get => false; }
    public ItemSystem.Items ItemIndex { get; private set; }
    
    [SerializeField] protected MeshRenderer[] parts;
    [Space]
    [SerializeField] protected AudioClip drawClip;

    protected PlayerController ply;
    protected ItemSystem ws;
    protected AudioSource aSource;
    protected InventorySystem inv;
    protected bool isActive;

    public virtual void Initialize (PlayerController ply, ItemSystem ws,InventorySystem inv, ItemSystem.Items itemIndex)
    {
        this.ply = ply;
        this.ws = ws;
        this.ItemIndex = itemIndex;
        this.inv = inv;

        aSource = GetComponent<AudioSource>();
    }

    public virtual void OnClassInitialized (PlayerClassBase currentClass)
    {

    }

    public virtual void OnDropped ()
    {

    }

    public virtual void UpdateOwner (InputPayload input)
    {

    }

    public virtual void OnEquipped ()
    {
        isActive = true;
        aSource.clip = drawClip;
        aSource.Play();
        if(ws.IsMine)
        {
            ply.CurrentWeaponSlotIndex = inv.CurrentSlot.Item;
        }
    }

    public virtual void OnDisequipped ()
    {
        isActive = false;
    }

    public virtual void Fire ()
    {

    }

    public void SetVisibility (bool state)
    {
        for (int i = 0; i < parts.Length; i++)
        {
            parts[i].enabled = state;
        }
    }

}
