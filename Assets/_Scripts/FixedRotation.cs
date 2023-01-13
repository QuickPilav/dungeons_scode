using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedRotation : MonoBehaviour
{
    private Quaternion startRot;
    private void Start ()
    {
        startRot = transform.rotation;
    }
    private void Update ()
    {
        transform.rotation = startRot;
    }
}
