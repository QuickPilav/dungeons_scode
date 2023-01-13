using System.Collections.Generic;
using UnityEngine;

public class ToxicGas : MonoBehaviour
{
    [SerializeField] private List<IDamagable> subscribedToxicities;

    private void Awake()
    {
        subscribedToxicities = new List<IDamagable>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IDamagable dmg) && dmg.AffectedByToxicity)
        {
            subscribedToxicities.Add(dmg);
            dmg.ToxicInflictors.Add(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out IDamagable dmg) && dmg.AffectedByToxicity)
        {
            subscribedToxicities.Remove(dmg);
            dmg.ToxicInflictors.Remove(this);
        }
    }

    private void OnDestroy()
    {
        foreach (var item in subscribedToxicities)
        {
            item.ToxicInflictors.Remove(this);
        }

        subscribedToxicities.Clear();
    }
}
