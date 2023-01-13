using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstantRotation : MonoBehaviour
{
    [SerializeField] private float rotateSpeed = 5f;
    [SerializeField] private bool rotateOnX;
    [SerializeField] private bool rotateOnY;
    [SerializeField] private bool rotateOnZ;
    private Vector3 rotateVector;

    private void Awake ()
    {
        rotateVector = new(rotateOnX ? 1f: 0f, rotateOnY ? 1f : 0f, rotateOnZ ? 1f : 0f);
    }

    private void OnValidate ()
    {
        Awake();
    }
    private void Update ()
    {
        transform.Rotate(rotateSpeed * Time.deltaTime * rotateVector);
    }
}
