using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestHoldButton : MonoBehaviourPun, IInteractableHold
{
    public float HoldForSeconds => 2;

    public Transform ObjectTransform => transform;

    public Optional<Outline> ObjectOutline => outline;

    [SerializeField] private Optional<Outline> outline;

    public float YOffset => .1f;

    public bool CantInteractWhen => interactedOnce;
    [SerializeField] private bool interactedOnce;
    [SerializeField] private bool setAsInterected;

    public PlayerController.Player_Anims CustomAnimation => PlayerController.Player_Anims.Interact;
    public int CustomAnimationLayer => 2;

    public bool CanMoveWhileInteracting => false;

    public int InteractionPriority => 100;

    private void Awake ()
    {
        outline.Remap();
        outline.Value.OutlineWidth = 0f;
    }

    public string GetPrompt (Language language)
    {
        return "BASILI TUTMA TESTÝ";
    }

    public void OnInteraction (PlayerController ply)
    {
        Debug.Log("INTERACTION BAÞARILI!!!!!");
        if(setAsInterected)
        {
            interactedOnce = true;
        }
    }

    public void OnInteractionFailed (PlayerController ply)
    {
        Debug.Log("INTERACTION FAILLEDI!");
    }

    public void OnInteractionStart (PlayerController ply)
    {
        Debug.Log("INTERACTION BAÞLADI!");
    }
}
