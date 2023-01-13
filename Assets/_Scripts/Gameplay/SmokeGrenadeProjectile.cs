using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmokeGrenadeProjectile : ThrowProjectile
{
    [SerializeField] private ParticleSystem smokeParticle;
    [SerializeField] private float waitTimeBeforeExplosion = 2f;
    [SerializeField] private float activeTime = 10f;
    [SerializeField] private float decayTime = 3f;
    [SerializeField] private GameObject smokeVisionBlocker;

    public List<IDamagable> Effectors => effectors;

    private readonly List<IDamagable> effectors = new List<IDamagable>();


    public override void OnLanded ()
    {
        StartCoroutine(enumerator());

        IEnumerator enumerator ()
        {
            yield return new WaitForSeconds(waitTimeBeforeExplosion);
            smokeParticle.Play();
            smokeVisionBlocker.SetActive(true);

            yield return new WaitForSeconds(activeTime);

            smokeVisionBlocker.SetActive(false);
            smokeParticle.Stop();

            foreach (var item in effectors)
            {
                item.SmokeInflictors.Remove(this);
            }

            yield return new WaitForSeconds(decayTime);

            

            if(photonView.IsMine)
            {
                PhotonNetwork.Destroy(gameObject);
            }
        }
    }
}
