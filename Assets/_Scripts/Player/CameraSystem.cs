using System;
using UnityEngine;

[System.Serializable]
public struct Optional<T>
{
    public bool Enabled 
    { 
        get => _enabled; 
        set => _enabled = value; 
    }
    public T Value
    {
        get => _value;
        set
        {
            _value = value;

            Remap();
        }
    }
    [SerializeField] private bool _enabled;
    [SerializeField] private T _value;

    public Optional (T value)
    {
        _value = value;
        _enabled = _value != null;
    }

    public void Remap ()
    {
        Enabled = _value != null;
    }
}

public class CameraSystem : StaticInstance<CameraSystem>
{
    [SerializeField] private Camera cam;
    [SerializeField] private float cameraLerpSpeed = 25f;

    [Space]

    [SerializeField] private float normalFov = 60;
    [SerializeField] private float aimedFov = 50;
    [SerializeField] private float fovMoveSpeed = 2f;
    [SerializeField] private float aimMoveSpeed = 2f;

    [SerializeField] private Transform shootPoint;
    [SerializeField] private Transform cameraHolder;
    [SerializeField] private EZCameraShake.CameraShaker shaker;

    public Vector3 MousePos { get => shootPoint.position;}

    private bool moveTowardsLook;

    private Vector3 lerpedShootPoint;

    private float MAX_EXPLOSION_HEAR_DISTANCE = 10f;

    public Camera Camera { get => cam; }
    
    private Optional<Transform> follow;

    protected override void Awake()
    {
        base.Awake();

    
        Invoke(nameof(Hey), 1f);
    }

    private void Hey ()
    {
        SceneLoadedHandler.GetSceneAs<GameScene>().WorldExplosionEvent += OnExplosionHappened;
    }


    private void OnExplosionHappened(Vector3 pos, float magnitude)
    {
        if (!follow.Enabled)
            return;

        float dist = Vector3.Distance(pos, follow.Value.position);

        float value = MAX_EXPLOSION_HEAR_DISTANCE - dist;

        ShakeOnce(value * magnitude);
    }

    private void Update ()
    {
        if (!follow.Enabled)
            return;

        Vector3 point = follow.Value.position;

        transform.position = Vector3.Lerp(transform.position, point, Time.deltaTime * cameraLerpSpeed);

        Vector3 targetShootPoint = moveTowardsLook ? cameraHolder.InverseTransformPoint(shootPoint.position) * .5f : Vector3.zero;
        
        lerpedShootPoint = Vector3.Lerp(lerpedShootPoint, targetShootPoint, Time.deltaTime * aimMoveSpeed);

        cameraHolder.localPosition = lerpedShootPoint;

        float targetFov = moveTowardsLook ? aimedFov : normalFov;

        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFov, Time.deltaTime * fovMoveSpeed);
        
    }

    public void Follow (Transform toFollow)
    {
        follow.Value = toFollow;
    }

    public void SetAttackPoint (InputPayload input, Vector3 pos)
    {
        //shootPoint.position = follow.Value.position + pos;
        shootPoint.position = pos;
        
        if(input.lastZoom != input.zoom)
        {
            InGameUI.Instance.ShowBlackbars(input.zoom);
        }
        
        moveTowardsLook = input.zoom;
    }

    public void ShakeOnce (float impact)
    {
        shaker.ShakeOnce(3 * impact, 2f, 0.1f, 2f);
    }

    public Vector3 GetMousePosition () => shootPoint.position;
}
