using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gib : PoolObject
{
    [SerializeField] private float throwSpeed = 10f;
    [SerializeField] private float ySpeed = 10f;
    private Rigidbody[] rbs;

    private void Awake ()
    {
        rbs = GetComponentsInChildren<Rigidbody>();
    }

    private static readonly WaitForSeconds gravityWaiter = new(5f);
    private static readonly WaitForSeconds destroyWaiter = new(1f);

    public void Initialize ()
    {
        StartCoroutine(enumerator());

        IEnumerator enumerator ()
        {
            foreach (var rb in rbs)
            {
                Vector3 random = Random.insideUnitSphere;
                random.y = 0;
                random = random.normalized * throwSpeed;
                random.y = ySpeed;

                rb.velocity = random;
                rb.angularVelocity = Random.insideUnitSphere * 10;
            }
            yield return gravityWaiter;

            foreach (var rb in rbs)
            {
                rb.detectCollisions = false;
            }

            yield return destroyWaiter;

            Destroy();
        }
    }

    public override void OnObjectReuseBeforeActivation ()
    {
        foreach (var rb in rbs)
        {
            rb.detectCollisions = true;
            
            rb.velocity = Vector3.zero;
            rb.transform.localPosition = Vector3.zero;
            rb.transform.localRotation = Quaternion.identity;
        }
    }

    public override void OnObjectReused ()
    {

    }
}
