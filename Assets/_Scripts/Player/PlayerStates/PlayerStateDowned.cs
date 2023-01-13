using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerStateDowned : PlayerStateBase
{
    public override void OnStateEnter ()
    {
        ply.photonView.RPC(nameof(ply.PlayAnimationRpc), Photon.Pun.RpcTarget.All, PlayerController.Player_Anims.Downed, 0);
    }

    public override void OnStateUpdate (InputPayload input, Vector3 mousePos)
    {
    }

    public override void OnStateExit ()
    {
        ply.photonView.RPC(nameof(ply.PlayAnimationRpc), Photon.Pun.RpcTarget.All, PlayerController.Player_Anims.GetUp, 0);
    }

    public override void OnDrawGizmos ()
    {
    }
}
