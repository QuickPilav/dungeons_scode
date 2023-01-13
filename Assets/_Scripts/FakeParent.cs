using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakeParent : MonoBehaviour
{
    public Transform target;

    Vector3 startParentPosition;
    Quaternion startParentRotationQ;

    Vector3 startChildPosition;
    Quaternion startChildRotationQ;

    Matrix4x4 parentMatrix;

    void Start()
    {
        startParentPosition = target.position;
        startParentRotationQ = target.rotation;

        startChildPosition = transform.position;
        startChildRotationQ = transform.rotation;
        var startParentScale = target.lossyScale;

        // Keeps child position from being modified at the start by the parent's initial transform
        startChildPosition = DivideVectors(Quaternion.Inverse(target.rotation) * (startChildPosition - startParentPosition), startParentScale);
    }

    void Update()
    {
        parentMatrix = Matrix4x4.TRS(target.position, target.rotation, target.lossyScale);

        transform.SetPositionAndRotation(parentMatrix.MultiplyPoint3x4(startChildPosition), (target.rotation * Quaternion.Inverse(startParentRotationQ)) * startChildRotationQ);
    }

    Vector3 DivideVectors(Vector3 num, Vector3 den)
    {
        return new Vector3(num.x / den.x, num.y / den.y, num.z / den.z);
    }
}
