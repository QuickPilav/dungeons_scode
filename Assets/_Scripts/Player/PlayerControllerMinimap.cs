using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControllerMinimap : MonoBehaviourPun
{
    [SerializeField] private Color myColor = Color.yellow;
    [SerializeField] private Color teammateColor = Color.cyan;

    private void Start()
    {
        bool isMine = photonView.IsMine;
        MinimapSystem.Instance.SpawnMarker("ply", transform, 5f, true, MinimapSystem.M_Player, isMine ? myColor : teammateColor, true, isMine ? 1 : 0.5f, true,isMine);
    }
}
