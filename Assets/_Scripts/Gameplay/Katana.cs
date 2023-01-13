using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

public class Katana : ItemInHand
{
    [SerializeField] private GameObject forwardShower;
    public override bool customFireSystem => true;

    public int Damage { get => damage;}

    [SerializeField] private int damage;
    [SerializeField] private float chargeTime = .5f;
    [SerializeField] private float dashAmount = 5f;
    [SerializeField] private float dashFor = 5f;

    [SerializeField] private GameObject katanaHitbox;

    private bool lastFireInput;
    private float chargedAmount;

    public override void Initialize (PlayerController ply, ItemSystem ws, InventorySystem inv, ItemSystem.Items itemIndex)
    {
        base.Initialize(ply, ws, inv, itemIndex);

        forwardShower.SetActive(false);
        katanaHitbox.SetActive(false);
    }

    public override void UpdateOwner (InputPayload input)
    {
        base.UpdateOwner(input);

        if(input.fire && !lastFireInput)
        {
            forwardShower.SetActive(true);
            chargedAmount = 0f;
        }
        else if (input.fire)
        {
            chargedAmount += Time.deltaTime;
        }
        else if (!input.fire & lastFireInput)
        {
            forwardShower.SetActive(false);

            if(chargedAmount >= chargeTime)
            {
                ReleasedChargedFire();
            }

        }

        lastFireInput = input.fire;
    }

    public void ReleasedChargedFire ()
    {
        ply.StartCoroutine(enumerator());

        IEnumerator enumerator ()
        {
            float timePassed = 0f;
            
            katanaHitbox.SetActive(true);

            Vector3 forward = transform.forward;
            while (timePassed < dashFor)
            {
                ply.Move(forward * dashAmount);
                timePassed += Time.deltaTime;
                yield return null;
            }

            katanaHitbox.SetActive(false);
        }

    }

    public override void OnDisequipped ()
    {
        base.OnDisequipped();

        forwardShower.SetActive(false);
        katanaHitbox.SetActive(false);

        lastFireInput = false;
        chargedAmount = 0f;
    }

}
