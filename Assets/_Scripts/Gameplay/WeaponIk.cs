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

    public bool dontUseOrientation;

    private Quaternion currentRotation;
    private readonly float reorientateTime = .25f;
    private IEnumerator currentReorientateRoutine;
    private Quaternion startRot;
    [SerializeField] private float movingReorientateSpeed = 5f;

    private void Awake()
    {
        startRot = transform.localRotation;

        currentRotation = transform.rotation;
    }


    private void Update()
    {
        if (dontUseOrientation)
        {
            if(currentReorientateRoutine != null)
            {
                StopCoroutine(currentReorientateRoutine);
            }

            transform.localRotation = startRot;

            currentRotation = Quaternion.Slerp(currentRotation,transform.rotation, movingReorientateSpeed * Time.deltaTime);
        }

        transform.rotation = currentRotation;
    }

    public void Reoritentate (Quaternion targetRot)
    {
        if(currentReorientateRoutine != null)
        {
            StopCoroutine(currentReorientateRoutine);
        }

        currentReorientateRoutine = ReOrientate(targetRot);
        StartCoroutine(currentReorientateRoutine);
    }

    private IEnumerator ReOrientate (Quaternion targetRot)
    {
        Quaternion startRot = currentRotation;
        Quaternion endRot = targetRot;

        float timePassed = 0f;
        while (timePassed < 1f)
        {
            timePassed += Time.deltaTime / reorientateTime;
            currentRotation = Quaternion.Slerp(startRot,endRot,timePassed);
            yield return null;
        }
        currentRotation = endRot;

        currentReorientateRoutine = null;
    }

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
