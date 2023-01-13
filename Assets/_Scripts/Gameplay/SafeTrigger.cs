using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SafeTrigger : MonoBehaviour
{
    private void OnTriggerEnter (Collider other)
    {
        if(!IsNextSafePoint())
        {
            return;
        }

        if (other.TryGetComponent(out IDamagable dmg) && dmg is PlayerController ply)
        {
            ply.IsSafe = true;
        }
    }

    private void OnTriggerExit (Collider other)
    {
        if (!IsNextSafePoint())
        {
            return;
        }

        if (other.TryGetComponent(out IDamagable dmg) && dmg is PlayerController ply)
        {
            ply.IsSafe = false;
        }
    }

    private bool IsNextSafePoint ()
    {
        int childIndex = transform.GetSiblingIndex();

        return WaveManager.Instance.CurrentFloodIndex == childIndex;
    }
}
