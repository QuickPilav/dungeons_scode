using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmokeVisionBlocker : MonoBehaviour
{
    [SerializeField] private SmokeGrenadeProjectile projectile;

    private void OnTriggerEnter (Collider other)
    {
        if(other.TryGetComponent(out IDamagable dmg))
        {
            projectile.Effectors.Add(dmg);
            dmg.SmokeInflictors.Add(projectile);
        }
    }

    private void OnTriggerExit (Collider other)
    {
        if (other.TryGetComponent(out IDamagable dmg))
        {
            projectile.Effectors.Remove(dmg);
            dmg.SmokeInflictors.Remove(projectile);
        }
    }
}
