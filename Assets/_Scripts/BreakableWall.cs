using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BreakableWall : MonoBehaviour
{
    [SerializeField] private ParticleSystem particles;

    private Collider col;
    private MeshRenderer meshRenderer;
    private NavMeshObstacle obst;

    [SerializeField] private GameObject[] secretSpawnpoints;

    private void Awake()
    {
        col = GetComponent<Collider>();
        obst = GetComponent<NavMeshObstacle>();
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public void Break()
    {
        particles.Play();

        col.enabled = false;
        obst.enabled = false;
        meshRenderer.enabled = false;

        foreach (var item in secretSpawnpoints)
        {
            item.SetActive(true);
        }

        Destroy(gameObject, 5f);
    }
}
