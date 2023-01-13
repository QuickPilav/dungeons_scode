using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SurfaceMaterial : byte
{
    Terrain,
    Metal,
    Water,
    Concrete,
    Wood,
    Human,
    None
}

public class ImpactVfx : PoolObject
{
    [SerializeField] private GameObject[] impactEffects;
    [SerializeField] private GameObject hitDecal;

    private AudioSource aSource;

    [SerializeField] private AudioClip[] terrainHits;
    [SerializeField] private AudioClip[] metalHits;
    [SerializeField] private AudioClip[] waterHits;
    [SerializeField] private AudioClip[] concreteHits;
    [SerializeField] private AudioClip[] woodHits;
    [SerializeField] private AudioClip[] humanHits;

    private void Awake ()
    {
        aSource = GetComponent<AudioSource>();
        aSource.volume = .5f;
        aSource.maxDistance = 24;
        aSource.spatialBlend = 1;
    }

    public void Initialize (SurfaceMaterial surfaceMat)
    {
        impactEffects[(int)surfaceMat].SetActive(true);
        hitDecal.SetActive(surfaceMat != SurfaceMaterial.Human);
        
        hitDecal.transform.localRotation *= Quaternion.Euler(0, 0, Random.Range(-360, 360));

        AudioClip[] audioClips;
        switch (surfaceMat)
        {
            case SurfaceMaterial.Terrain:
                audioClips = terrainHits;
                break;
            case SurfaceMaterial.Metal:
                audioClips = metalHits;

                break;
            case SurfaceMaterial.Water:
                audioClips = waterHits;

                break;
            case SurfaceMaterial.Concrete:
                audioClips = concreteHits;

                break;
            case SurfaceMaterial.Wood:
                audioClips = woodHits;

                break;
            case SurfaceMaterial.Human:
                audioClips = humanHits;
                break;
            default:
                return;
        }

        var ad = audioClips[Random.Range(0, audioClips.Length)];

        aSource.PlayOneShot(ad);
    }

    public override void OnObjectReuseBeforeActivation ()
    {
        foreach (var item in impactEffects)
        {
            item.SetActive(false);
        }
    }

    public override void OnObjectReused ()
    {

    }
}
