using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public interface IInteractable
{
    /// <summary>en yüksek sayý en öncelikli...</summary>
    public int InteractionPriority { get;}
    public Transform ObjectTransform { get; }
    public Optional<Outline> ObjectOutline { get; }
    public float YOffset { get; }
    public void OnInteraction (PlayerController ply);
    public string GetPrompt (Language language);
    public bool CantInteractWhen { get; }
    public PlayerController.Player_Anims CustomAnimation { get; }
    public int CustomAnimationLayer { get; }
}

public interface IInteractableHold : IInteractable
{
    public float HoldForSeconds { get; }
    public bool CanMoveWhileInteracting { get; }
    public void OnInteractionStart (PlayerController ply);
    public void OnInteractionFailed (PlayerController ply);
}

[System.Serializable]
public class PlayerInteraction
{
    private PlayerController ply;


    private const float INTERACTION_RADIUS = 2;

    private const float INTERACTION_ANGLE = 145;

    [SerializeField] private Collider[] cols;
    
    private float holdTimer;
    private Language currentLanguage;

    private Optional<IInteractable> lastInteractable;
    private Optional<IInteractableHold> lastHoldInteractable;

    private bool lastPressInteraction;

    public bool CanNotMove { get; private set; }

    //is mine
    public void Initialize (PlayerController ply)
    {
        this.ply = ply;
        cols = new Collider[10];
        SaveSocket.OnSettingsChanged.SubscribeToEvent(OnSettingsChanged);
    }

    ~PlayerInteraction()
    {
        SaveSocket.OnSettingsChanged.UnsubscribeToEvent(OnSettingsChanged);
    }

    private void OnSettingsChanged (SettingsSave save)
    {
        currentLanguage = save.language;
    }


    public void Update (InputPayload input)
    {
        var interactable = GetClosestInteractable();

        if (lastInteractable.Enabled && lastInteractable.Value != interactable)
        {
            if (lastInteractable.Value.ObjectOutline.Enabled)
            {
                lastInteractable.Value.ObjectOutline.Value.OutlineWidth = 0;
            }
            if (lastHoldInteractable.Enabled)
            {
                lastHoldInteractable.Value.OnInteractionFailed(ply);
                InGameUI.Instance.CancelCast();
                lastHoldInteractable.Value = null;
                ply.photonView.RPC(nameof(ply.PlayAnimationRpc), Photon.Pun.RpcTarget.All, PlayerController.Player_Anims.Speed, 0);
                CanNotMove = false;
            }
        }

        if (interactable != null)
        {
            bool shouldOutline = false;
            if (interactable.CantInteractWhen)
            {
                InGameUI.Instance.ShowPrompt(string.Empty);
            }
            else
            {
                shouldOutline = true;
                InGameUI.Instance.ShowPrompt(interactable.GetPrompt(currentLanguage));
            }

            if (interactable.ObjectOutline.Enabled)
            {
                interactable.ObjectOutline.Value.OutlineWidth = shouldOutline ? 5f : 0f;
            }
        }
        else
        {
            InGameUI.Instance.ShowPrompt(string.Empty);
        }

        if (input.interaction && interactable != null && !interactable.CantInteractWhen)
        {
            if(interactable is IInteractableHold hold)
            {
                lastHoldInteractable.Remap();
                if(!lastHoldInteractable.Enabled && !lastPressInteraction)
                {
                    holdTimer = hold.HoldForSeconds;
                    lastHoldInteractable.Value = hold;
                    hold.OnInteractionStart(ply);
                    InGameUI.Instance.StartNewCast(hold.HoldForSeconds);
                    ply.photonView.RPC(nameof(ply.PlayAnimationRpc), Photon.Pun.RpcTarget.All, interactable.CustomAnimation, interactable.CustomAnimationLayer);
                    CanNotMove = !hold.CanMoveWhileInteracting;
                }
                else
                {
                    holdTimer -= Time.deltaTime;
                    if(holdTimer <= 0)
                    {
                        hold.OnInteraction(ply);
                        InGameUI.Instance.CancelCast();
                        lastHoldInteractable.Value = null;
                        ply.photonView.RPC(nameof(ply.PlayAnimationRpc), Photon.Pun.RpcTarget.All, PlayerController.Player_Anims.Speed, 0);
                        CanNotMove = false;
                    }
                }
            }
            else if(!lastPressInteraction)
            {
                cols = new Collider[10];
                interactable.OnInteraction(ply);
                ply.photonView.RPC(nameof(ply.PlayAnimationRpc), Photon.Pun.RpcTarget.All, interactable.CustomAnimation, interactable.CustomAnimationLayer);
            }
            
        }
        else
        {
            if (lastHoldInteractable.Enabled)
            {
                lastHoldInteractable.Value.OnInteractionFailed(ply);
                InGameUI.Instance.CancelCast();
                lastHoldInteractable.Value = null;
                ply.photonView.RPC(nameof(ply.PlayAnimationRpc), Photon.Pun.RpcTarget.All, PlayerController.Player_Anims.Speed, 0);
                CanNotMove = false;
            }
        }

        lastInteractable.Value = interactable;
        lastPressInteraction = input.interaction;
    }

    private IInteractable GetClosestInteractable ()
    {
        var interactables = FindInteractables(ply.EyePosition, INTERACTION_RADIUS);

        if (interactables.Count > 0)
        {
            //interactables = interactables.OrderByDescending(x => (x.ObjectTransform.position - ply.EyePosition).sqrMagnitude).ToList();

            interactables = interactables.OrderByDescending(x => x.InteractionPriority).ToList();

            return interactables.FirstOrDefault();
        }
        return null;
    }


    private List<IInteractable> FindInteractables (Vector3 checkPoint, float radius)
    {
        List<IInteractable> interactables = new List<IInteractable>();

        int length = Physics.OverlapSphereNonAlloc(checkPoint, radius, cols, LayerManager.HitLayer);

        for (int i = 0; i < length; i++)
        {
            Transform target = cols[i].transform;
            
            if (target.CompareTag("NoCollision"))
                continue;

            var interactable = target.GetComponentInParent<IInteractable>();

            if (interactable == null)
                continue;

            if (interactable.CantInteractWhen)
                continue;

            Vector3 targetPosition = target.position + Vector3.up * interactable.YOffset;

            Vector3 dirToTarget = (targetPosition - checkPoint).normalized;

            if (Vector3.Angle(ply.transform.forward, dirToTarget) < INTERACTION_ANGLE / 2)
            {
                float dstToTarget = Vector3.Distance(checkPoint, targetPosition);

                if (!Physics.Raycast(checkPoint, dirToTarget, out RaycastHit hit, dstToTarget, LayerManager.GroundLayer))
                {
                    interactables.Add(interactable);
                }
                else if (hit.collider == cols[i])
                {

                    interactables.Add(interactable);

                }
            }
        }

        return interactables;
    }

    public void ForceStopHoldInteraction ()
    {
        if(lastInteractable.Enabled)
        {
            if (lastInteractable.Value.ObjectOutline.Enabled)
            {
                lastInteractable.Value.ObjectOutline.Value.OutlineWidth = 0;
            }
            if (lastHoldInteractable.Enabled)
            {
                lastHoldInteractable.Value.OnInteractionFailed(ply);
                InGameUI.Instance.CancelCast();
                lastHoldInteractable.Value = null;
                ply.photonView.RPC(nameof(ply.PlayAnimationRpc), Photon.Pun.RpcTarget.All, PlayerController.Player_Anims.Speed, 0);
                CanNotMove = false;
            }
        }
    }
}
