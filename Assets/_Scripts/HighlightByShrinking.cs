using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightByShrinking : MonoBehaviour
{
    [SerializeField] private float highlightMultiplier = 2f;
    [SerializeField] private float highlightSpeed = 1f;
    private Transform _transform;

    private float randomized;
    private void Start()
    {
        randomized = Random.Range(-10f,110f);
        _transform = transform;
    }
    private void Update()
    {
        float sin = Mathf.Sin((Time.time - randomized) * highlightSpeed) * highlightMultiplier;
        _transform.localScale = Vector3.one * (1 + sin);
    }
}
