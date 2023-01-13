using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawSphereGizmo : MonoBehaviour
{
    [SerializeField] private Color color = Color.magenta;
    [SerializeField] private float radius = .2f;

    private void OnDrawGizmos ()
    {
        Gizmos.color = color;
        Gizmos.DrawWireSphere(transform.position,radius);
    }
}
