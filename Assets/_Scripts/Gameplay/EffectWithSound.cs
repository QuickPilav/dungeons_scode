using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectWithSound : MonoBehaviour
{
    [SerializeField,Range(0,1f)] private float maxVolume = .5f;
    [SerializeField] private float volumeChangeSpeed = 1f;

    private ParticleSystem effect;
    private AudioSource aSource;

    private void Awake ()
    {
        effect = GetComponent<ParticleSystem>();
        aSource = GetComponent<AudioSource>();
    }

    public void Play ()
    {
        effect.Play();

        InGameUI.SlowlyGainAudio(aSource, 0, maxVolume, volumeChangeSpeed);
        aSource.Play();
    }

    public void Stop ()
    {
        effect.Stop();

        InGameUI.SlowlyGainAudio(aSource, maxVolume, 0, volumeChangeSpeed);
    }

}
