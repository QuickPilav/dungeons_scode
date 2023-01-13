using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerStateBase
{
    protected PlayerController ply;
    public virtual void Initialize (PlayerController ply)
    {
        this.ply = ply;
    }

    public abstract void OnStateEnter ();
    public abstract void OnStateUpdate (InputPayload input, Vector3 mousePos);
    public abstract void OnStateExit ();
    public abstract void OnDrawGizmos ();
}
