using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopInteractable : ClickableButton
{
    [SerializeField] private float shopDistance = 2.7f;

    private PlayerController plyInteractor;

    protected override bool OnClicked (PlayerController ply)
    {
        plyInteractor = ply;
        ShopUI.Instance.IsShopOpen = !ShopUI.Instance.IsShopOpen;
        return false;
    }

    public override int InteractionPriority => 1;

    private void Update ()
    {
        if(!ShopUI.Instance.IsShopOpen)
        {
            plyInteractor = null;
            return;
        }

        if (plyInteractor == null)
            return;


        if(Vector3.Distance(plyInteractor.transform.position, transform.position) > shopDistance)
        {
            ShopUI.Instance.IsShopOpen = false;
            plyInteractor = null;
        }
    }

    public override void OnPhotonInstantiate (PhotonMessageInfo info)
    {
        throw new System.NotImplementedException();
    }
}
