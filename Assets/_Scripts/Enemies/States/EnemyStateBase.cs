using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyStateBase
{
    protected EnemyAI ai;

    public virtual bool NonCurretStateUpdate { get => false; }

    public virtual void Initialize (EnemyAI ai)
    {
        this.ai = ai;
        if(NonCurretStateUpdate)
        {
            ai.RegisterNonCurrentStateUpdate(this);
        }
    }
    public abstract bool OnStateEnter ();
    public abstract void OnStateUpdateLateNormal ();
    public abstract void OnStateUpdateNormal ();
    public abstract void NonCurrentStateUpdate ();
    public abstract bool OnStateExit ();
    public abstract void OnDrawGizmos ();
}
