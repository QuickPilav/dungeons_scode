using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class DroppedGem : ClickableButton
{
    private int gemHas;

    private bool initialized;

    public override bool CantInteractWhen => base.CantInteractWhen || !initialized;

    /*
    [PunRPC]
    public void GemInitializeRpc (int gemHas)
    {
        this.gemHas = gemHas;

        initialized = true;
    }
    */

    public override int InteractionPriority => 0;

    public override string GetPrompt (Language language)
    {
        return language == Language.Turkish ? $"{gemHas} {base.GetPrompt(language)}" : $"{base.GetPrompt(language)} {gemHas} Gems";
    }

    protected override bool OnClicked (PlayerController ply)
    {
        ply.GemsHave += gemHas;
        initialized = false;

        if(photonView.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
            return true;
        }
        
        photonView.RPC(nameof(DestroyViaOwnerRpc),photonView.Owner);
        return true;
    }

    [PunRPC]
    public void DestroyViaOwnerRpc ()
    {
        PhotonNetwork.Destroy(gameObject);
    }

    public override void OnPhotonInstantiate (PhotonMessageInfo info)
    {
        object[] data = info.photonView.InstantiationData;

        this.gemHas = (int)data[0];

        initialized = true;
    }
}
