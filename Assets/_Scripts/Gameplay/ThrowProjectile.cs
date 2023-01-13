using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ThrowProjectile : MonoBehaviourPun, IPunInstantiateMagicCallback
{
    [SerializeField] private float maxY = 1f;
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] protected Transform mesh;

    private Vector3 startPoint;
    private Vector3 landPoint;
    private float timePassed;

    private Vector3 lastPos;

    private void Update ()
    {
        if (timePassed == 1)
            return;

        timePassed = Mathf.Min(timePassed + Time.deltaTime * moveSpeed, 1);

        float sin = Mathf.Sin(timePassed * Mathf.PI) * maxY;

        transform.position = Vector3.Lerp(startPoint, landPoint, timePassed) + Vector3.up * sin;
        
        mesh.rotation = Quaternion.LookRotation((lastPos - transform.position).normalized);

        lastPos = transform.position;

        if(timePassed == 1)
        {
            OnLanded();
        }
    }

    public abstract void OnLanded ();

    public void OnPhotonInstantiate (PhotonMessageInfo info)
    {
        object[] data = info.photonView.InstantiationData;

        landPoint = (Vector3)data[0];
        startPoint = transform.position;
        lastPos = transform.position;
    }
}
