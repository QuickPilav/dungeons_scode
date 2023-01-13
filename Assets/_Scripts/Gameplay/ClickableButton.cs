using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;
using static PlayerController;

public enum ButtonSounds
{
    Normal,
    Collect,
    WaterBottle,
    Apple,
    Peksemet
}
public abstract class ClickableButton : MonoBehaviourPun, IInteractable, IPunInstantiateMagicCallback
{
    public Optional<Outline> ObjectOutline { get => outline; }
    public float YOffset { get => yOffset; }

    public virtual bool CantInteractWhen => (onlyHostCanInteract && !PhotonNetwork.IsMasterClient) || interacted;

    public Transform ObjectTransform => transform;

    public Player_Anims CustomAnimation => Player_Anims.Interact;
    public int CustomAnimationLayer => 2;

    public abstract int InteractionPriority { get; }

    [SerializeField] private Optional<Outline> outline;

    [SerializeField] private string prompt_ENG = "için týkla";
    [SerializeField] private string prompt_TR = "click for";

    [SerializeField] private float yOffset;
    [SerializeField] protected bool onlyHostCanInteract;
    [SerializeField] protected bool interactableOnce;
    [SerializeField] protected bool callEventOnlyOnClicker;
    [SerializeField] protected float interactionRate = .5f;
    [SerializeField] protected ButtonSounds buttonSound;

    private WaitForSeconds waiter;

    private bool interacted;

    private void Awake ()
    {
        if (!interactableOnce)
        {
            waiter = new WaitForSeconds(interactionRate);
        }

        outline.Remap();


        if(outline.Enabled)
        {
            outline.Value.OutlineWidth = 0f;
        }
    }


    public virtual string GetPrompt (Language language)
    {
        return language == Language.Turkish ? prompt_TR : prompt_ENG;
    }

    public void OnInteraction (PlayerController ply)
    {
        if (interacted)
            return;

        if (callEventOnlyOnClicker)
        {
            OnClickedRpc(ply.photonView.Owner);
        }
        else
        {
            photonView.RPC(nameof(OnClickedRpc), RpcTarget.AllViaServer, ply.photonView.Owner);
        }

        InGameUI.Instance.PlayButtonSound(buttonSound);
    }

    [PunRPC]
    public void OnClickedRpc (Player ply)
    {
        interacted = true;
        if (OnClicked(ply.TagObject as PlayerController))
        {

            return;
        }

        if (interactableOnce)
            return;

        StartCoroutine(enumerator());

        IEnumerator enumerator ()
        {
            yield return waiter;
            interacted = false;
        }
    }

    /// <summary>Eðer yok olmuþ ise true dönün</summary>
    protected abstract bool OnClicked (PlayerController ply);

    public abstract void OnPhotonInstantiate (PhotonMessageInfo info);
}
