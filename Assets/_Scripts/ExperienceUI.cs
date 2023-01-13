using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ExperienceUI : PoolObject
{
    [SerializeField] private TextMeshProUGUI xpText;

    private const float RANDOMIZE_SPAWN_POS = 2f;
    private const float MOVE_SPEED = 5f;
    private const float UP_MOVE_SPEED = 22f;
    private const float UP_TIME = 2f;
    private const float DISAPPEAR_TIME = 4f;

    private float timePassed;
    private RectTransform parent;
    private RectTransform rt;
    private Vector2 spawnPos;


    private void Awake()
    {
        rt = GetComponent<RectTransform>();
        parent = transform.parent.GetComponent<RectTransform>();
        spawnPos = rt.anchoredPosition;
    }
    
    public void Initialize (int xp)
    {
        xpText.text = $"+{xp}xp";
        rt.anchoredPosition = spawnPos + Random.insideUnitCircle * RANDOMIZE_SPAWN_POS;
    }

    private void Update()
    {
        timePassed += Time.deltaTime;

        if(timePassed < UP_TIME)
        {
            rt.position += Time.deltaTime * UP_MOVE_SPEED * Vector3.up;
            return;
        }
        else if (timePassed < DISAPPEAR_TIME)
        {
            rt.position = Vector2.Lerp(rt.position, parent.anchoredPosition, Time.deltaTime * MOVE_SPEED);
            return;
        }

        Destroy();
    }

    public override void OnObjectReuseBeforeActivation()
    {

    }

    public override void OnObjectReused()
    {
        timePassed = 0;
    }
}
