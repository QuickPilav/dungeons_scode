using Photon.Pun;
using Unity.VisualScripting;
using UnityEngine;

public enum RandomPickupType
{
    WaterBottle,
    Apple,
    Peksemet
}

public class RandomPickup : ClickableButton
{
    private const int APPLE_HEALTH_AMOUNT = 5;

    private RandomPickupType randomPickupType;

    private bool initialized;

    public override bool CantInteractWhen => base.CantInteractWhen || !initialized;

    public override int InteractionPriority => 1;

    public override string GetPrompt (Language language)
    {
        if (language == Language.Turkish)
        {
            string trVersion;
            switch (randomPickupType)
            {
                case RandomPickupType.WaterBottle:
                    trVersion = "su þiþesi";
                    break;
                case RandomPickupType.Apple:
                    trVersion = "elma";
                    break;
                default:
                    trVersion = randomPickupType.ToString().FirstCharacterToUpper();
                    break;
            }

            return $"{base.GetPrompt(language)} {trVersion.FirstCharacterToUpper()}";
        }
        else
        {
            return $"{base.GetPrompt(language)} {randomPickupType.ToString().FirstCharacterToUpper()}";
        }
    }

    protected override bool OnClicked (PlayerController ply)
    {
        initialized = false;

        switch (randomPickupType)
        {
            case RandomPickupType.WaterBottle:
                ply.GiveSpeedBuff(.05f, 5);
                break;
            case RandomPickupType.Apple:
                ply.Health += APPLE_HEALTH_AMOUNT;
                break;
            case RandomPickupType.Peksemet:
                ply.ItemSystem.OnPeksemetUsed();
                break;
        }


        if (photonView.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
            return true;
        }

        photonView.RPC(nameof(DestroyViaOwnerRpc), photonView.Owner);
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

        RandomPickupType randomPickupType = (RandomPickupType)data[0];

        this.randomPickupType = randomPickupType;

        foreach (Transform child in transform.GetChild(0))
        {
            child.gameObject.SetActive(false);
        }
        transform.GetChild(0).GetChild((int)randomPickupType).gameObject.SetActive(true);

        switch (randomPickupType)
        {
            case RandomPickupType.WaterBottle:
                buttonSound = ButtonSounds.WaterBottle;
                break;
            case RandomPickupType.Apple:
                buttonSound = ButtonSounds.Apple;
                break;
            case RandomPickupType.Peksemet:
                buttonSound = ButtonSounds.Peksemet;
                break;
        }

        initialized = true;
    }
}
