using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloseProximity : MonoBehaviour
{
    private PlayerController myPly;
    private void Awake ()
    {
        myPly = GetComponentInParent<PlayerController>();
        if(myPly.photonView.IsMine)
        {
            Destroy(this);
        }
    }

    private void OnTriggerEnter (Collider col)
    {
        if(col.TryGetComponent(out PlayerController ply) && ply != myPly)
        {
            ply.AddProximityPlayer(myPly);
        }
    }

    private void OnTriggerExit (Collider col)
    {
        if (col.TryGetComponent(out PlayerController ply) && ply != myPly)
        {
            ply.RemoveProximityPlayer(myPly);
        }
    }
}
