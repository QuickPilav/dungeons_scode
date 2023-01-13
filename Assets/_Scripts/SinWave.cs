using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SinWave : MonoBehaviour
{
    [SerializeField] private float speedMultiplier = 5;
    [SerializeField] private float sinMultiplier = 1f;
    [SerializeField] private bool affectX;
    [SerializeField] private bool affectY;
    [SerializeField] private bool affectZ;
    private Vector3 movementVector;

    private void Awake ()
    {
        movementVector = new(affectX ? 1f : 0f, affectY ? 1f : 0f, affectZ ? 1f : 0f);
    }

    private void OnValidate ()
    {
        Awake();
    }

    private void Update ()
    {
        float sin = Mathf.Sin(Time.time * speedMultiplier) * sinMultiplier;

        transform.localPosition = movementVector * sin;
    }
}
