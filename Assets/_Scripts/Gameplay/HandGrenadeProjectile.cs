using Photon.Pun;
using System.Collections;
using UnityEngine;

public class HandGrenadeProjectile : ThrowProjectile
{
    [SerializeField] private ParticleSystem explosionParticle;
    [SerializeField] private float waitTimeBeforeExplosion = 2f;
    [SerializeField] private float decayTime = 3f;

    [SerializeField] private float damageStart = 600;
    [SerializeField] private float damageEnd = 10;
    [SerializeField] private float radius = 5f;

    private Collider[] cols;


    public override void OnLanded()
    {
        cols = new Collider[32];
        StartCoroutine(enumerator());

        IEnumerator enumerator()
        {
            yield return new WaitForSeconds(waitTimeBeforeExplosion);
            //photonView.RPC(nameof(ExplodeRpc), RpcTarget.AllViaServer);

            SceneLoadedHandler.GetSceneAs<GameScene>().WorldExplosionEvent?.Invoke(transform.position, 1f);

            Explode();
            explosionParticle.Play();
            mesh.gameObject.SetActive(false);
            yield return new WaitForSeconds(decayTime);

            if (photonView.IsMine)
            {
                PhotonNetwork.Destroy(gameObject);
            }
        }
    }

    private void Explode()
    {
        if (!photonView.IsMine)
            return;

        var colsLength = Physics.OverlapSphereNonAlloc(transform.position, radius, cols, LayerManager.OnlyEnemies, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < colsLength; i++)
        {
            IDamagable dmg = cols[i].GetComponent<IDamagable>();

            if (dmg == null)
            {
                Debug.Log($"{cols[i].name} hatalý!!!");
                continue;
            }

            float distance = Vector3.Distance(transform.position, dmg.ObjectTransform.position);

            float closeness = Mathf.InverseLerp(0, radius, distance);

            int calculatedDamage = Mathf.FloorToInt(Mathf.Lerp(damageStart, damageEnd, closeness));

            dmg.PhotonView.RPC(nameof(dmg.TakeDamageRpc), RpcTarget.MasterClient, photonView.Controller, calculatedDamage, DamageType.Explosion);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
