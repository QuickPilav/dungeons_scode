using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class WeaponIk : MonoBehaviour
{
    [System.Serializable]
    public class Bone
    {
        public Transform bone;
        [Range(0,1f)] public float weight = 1f;
    }

    [SerializeField] private Transform targetTransform;
    [SerializeField] private Transform aimTransform;
    [SerializeField] private Bone[] boneTransforms;

    [SerializeField, Range(0,1f)] private float weight = 1f;
    [SerializeField] private int iterations = 10;
    [SerializeField] private float angleLimit = 90f;
    [SerializeField] private float distanceLimit = 1.5f;

    private void LateUpdate ()
    {
        Vector3 targetPosition = GetTargetPosition();
        for (int i = 0; i < iterations; i++)
        {
            for (int b = 0; b < boneTransforms.Length; b++)
            {
                AimAtTarget(boneTransforms[b].bone, targetPosition, boneTransforms[b].weight * weight);
            }
        }
    }

    private Vector3 GetTargetPosition ()
    {
        Vector3 targetDirection = targetTransform.position - aimTransform.position;
        Vector3 aimDirection = aimTransform.forward;
        float blendOut = 0f;

        float targetAngle = Vector3.Angle(targetDirection, aimDirection);
        if(targetAngle > angleLimit)
        {
            blendOut += (targetAngle - angleLimit) / 50f;
        }

        float targetDistance = targetDirection.magnitude;
        if(targetDistance < distanceLimit)
        { 
            blendOut += distanceLimit - targetDistance; 
        }

        Vector3 direction = Vector3.Slerp(targetDirection,aimDirection,blendOut);
        return aimTransform.position + direction;
    }

    private void AimAtTarget (Transform bone, Vector3 targetPosition, float weight)
    {
        Vector3 aimDirection = aimTransform.forward;
        Vector3 targetDirection = targetPosition - aimTransform.position;
        Quaternion aimTowards = Quaternion.FromToRotation(aimDirection, targetDirection);
        Quaternion blendedRotation = Quaternion.Slerp(Quaternion.identity, aimTowards, weight);
        bone.rotation = blendedRotation * bone.rotation;
    }
}
