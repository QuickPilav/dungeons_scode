using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Consumable : ItemInHand
{
    [SerializeField] private int healthGives;
    [SerializeField] private bool givesSpeedBuff;
    [SerializeField] private bool fullsBattery;

    public override void Fire ()
    {
        if (!ws.IsMine)
            return;

        base.Fire();

        if(healthGives != 0 && ply.Health < 100)
        {
            Debug.Log("bandaj kullandýn!");
            ply.Health += healthGives;
            ws.RemoveCurrentItem();
        }
        if(givesSpeedBuff && !ply.ActiveBuffs.Contains(BuffsNames.SpeedBuff))
        {
            Debug.Log("enerji kullandýn!");
            ply.GiveSpeedBuff(.1f,120);
            ws.RemoveCurrentItem();
        }
        if(fullsBattery && ply.BatteryLeft < 100)
        {
            Debug.Log("bataryan doldu!");
            ply.RefillBattery();
            ws.RemoveCurrentItem();
        }
    }
}
