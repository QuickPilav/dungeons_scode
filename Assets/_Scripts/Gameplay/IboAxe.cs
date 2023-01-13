using Photon.Pun;
using System.Collections;
using UnityEngine;

public class IboAxe : ItemInHand
{
    public int RotationDamage { get => rotationDamage; }

    public override bool customFireSystem => true;

    [SerializeField] private float activeTimeUntilExplosion = 4f;
    [SerializeField] private float rotateSpeed = 15f;
    [SerializeField] private GameObject rotationHitbox;
    [SerializeField] private int rotationDamage = 50;
    [SerializeField] private float randomMoveMultiplier = 3f;
    [SerializeField] private float rotationAngleMultiplier = 1f;
    [Space]
    [SerializeField] private float finalExplosionRadius = 5f;
    [SerializeField] private int finalExplosionDamage = 150;
    [SerializeField] private float waitTimeUntilControlIsBack = 1f;
    [SerializeField] private float cooldownTime = .5f;

    private float activeFor;
    private WaitForSeconds waiterUntilControlIsBack;
    private WaitForSeconds cooldownWaiter;
    private bool exploded;

    public override void Initialize (PlayerController ply, ItemSystem ws, InventorySystem inv, ItemSystem.Items itemIndex)
    {
        base.Initialize(ply, ws, inv, itemIndex);
        waiterUntilControlIsBack = new WaitForSeconds(waitTimeUntilControlIsBack);
        cooldownWaiter = new WaitForSeconds(cooldownTime);
        rotationHitbox.SetActive(false);
    }

    public override void OnEquipped ()
    {
        base.OnEquipped();
        rotationHitbox.SetActive(true);
        activeFor = 0;
        ply.GroundedState.IsSwingingAxe = true;
        exploded = false;
    }

    public override void UpdateOwner (InputPayload input)
    {
        base.UpdateOwner(input);

        if (exploded)
            return;

        ply.transform.Rotate(rotateSpeed * Time.deltaTime * Vector3.up);

        activeFor += Time.deltaTime;

        float sin = Mathf.Sin(activeFor * rotationAngleMultiplier) * randomMoveMultiplier;
        float cos = Mathf.Cos(activeFor * rotationAngleMultiplier) * randomMoveMultiplier;

        ply.Move(new Vector3(cos, 0, sin));


        if (activeFor >= activeTimeUntilExplosion)
        {
            Explode();
        }
    }

    private void Explode ()
    {
        exploded = true;

        rotationHitbox.SetActive(false);

        StartCoroutine(enumerator());

        IEnumerator enumerator ()
        {
            yield return waiterUntilControlIsBack;

            ply.GroundedState.IsPlacingAxe = true;

            var cols = Physics.OverlapSphere(transform.position, finalExplosionRadius, LayerManager.OnlyEnemies, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < cols.Length; i++)
            {
                IDamagable dmg = cols[i].GetComponent<IDamagable>();

                if (dmg == null)
                    continue;
                
                dmg.PhotonView.RPC(nameof(dmg.TakeDamageRpc), RpcTarget.MasterClient, ply.photonView.Controller, finalExplosionDamage, DamageType.Explosion);
            }

            yield return cooldownWaiter;

            ply.ClassHandler.CanNotSetAbilityBar = false;

            ply.GroundedState.IsPlacingAxe = false;
            ply.GroundedState.IsSwingingAxe = false;
            ws.RemoveCurrentItem();

        }
    }

    public override void Fire ()
    {
        base.Fire();

        if (!ws.IsMine)
            return;

        Explode();
    }
}
