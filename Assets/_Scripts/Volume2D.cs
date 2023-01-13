using UnityEngine;

public class Volume2D : MonoBehaviour
{
    [SerializeField] private float minDist = 4;
    [SerializeField] private float maxDist = 10;

    private Transform listenerTransform;
    private AudioSource audioSource;
    private float maxVolume;

    private void Awake ()
    {
        audioSource = GetComponent<AudioSource>();
        maxVolume = audioSource.volume;
        audioSource.volume = 0f;
        PlayerController.OnClientSpawned.SubscribeToEvent(OnClientSpawned);
    }

    private void OnDestroy ()
    {
        PlayerController.OnClientSpawned.UnsubscribeToEvent(OnClientSpawned);
    }

    private void OnClientSpawned (PlayerController obj)
    {
        listenerTransform = obj.transform;
    }

    private void LateUpdate ()
    {
        if (listenerTransform == null)
            return;

        float dist = Vector3.Distance(transform.position, listenerTransform.position);
        
        if (dist < minDist)
        {
            audioSource.volume = maxVolume;
        }
        else if (dist > maxDist)
        {
            audioSource.volume = 0;
        }
        else
        {
            audioSource.volume = maxVolume * (1 - ((dist - minDist) / (maxDist - minDist)));
        }
    }

    private void OnDrawGizmosSelected ()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, minDist);
        Gizmos.DrawWireSphere(transform.position, maxDist);
    }
}
