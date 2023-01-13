using Photon.Pun;
using UnityEngine;

public class StartGameButton : ClickableButton
{
    public override bool CantInteractWhen => base.CantInteractWhen || WaveManager.GameStarted;

    public override int InteractionPriority => 1;

    protected override bool OnClicked (PlayerController ply)
    {
        WaveManager.Instance.StartGame();
        return false;
    }

    public override void OnPhotonInstantiate (PhotonMessageInfo info)
    {
        throw new System.NotImplementedException();
    }
}
