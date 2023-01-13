using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PoolObject : MonoBehaviour
{
    public abstract void OnObjectReuseBeforeActivation();
    public abstract void OnObjectReused();

    protected virtual void Destroy()
    {
        gameObject.SetActive(false);
        
        /*if(isPoolItem)
        {
            gameObject.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
        */
    }
}
