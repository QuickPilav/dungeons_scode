using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyStateIdle : EnemyStateBase
{
    public override bool OnStateEnter ()
    {
        return true;
    }

    public override bool OnStateExit ()
    {
        return true;
    }

    public override void NonCurrentStateUpdate ()
    {
    }

    public IEnumerator IdleFor (float seconds)
    {
        yield return new WaitForSeconds(seconds);
    }

    public override void OnStateUpdateNormal ()
    {
    }

    public override void OnStateUpdateLateNormal ()
    {
    }

    public override void OnDrawGizmos ()
    {
    }
}
