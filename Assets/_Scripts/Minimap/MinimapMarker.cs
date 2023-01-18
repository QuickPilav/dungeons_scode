using UnityEngine;

public class MinimapMarker : MonoBehaviour
{
    private const float DEFAULT_SCALE = 5f;

    [SerializeField] private SpriteRenderer spriteRenderer;

    private bool rotateWithTarget;
    private bool alwaysOnMap;
    private Transform target;
    private float priority;

    private string markerKey;
    private bool initialized;
    private float size;
    private bool fixedSize;

    private Camera minimapCam;

    public void Initialize(Camera minimapCam, string markerKey, Transform markerOwner, float size, bool rotateWithTarget, Sprite mapIcon, Color color, bool alwaysOnMap, float priority, bool fixedSize)
    {
        initialized = true;
        target = markerOwner;
        this.markerKey = markerKey;
        this.rotateWithTarget = rotateWithTarget;
        this.alwaysOnMap = alwaysOnMap;
        this.priority = priority;
        this.size = size;
        this.fixedSize = fixedSize;
        transform.localScale = Vector3.one * size;

        this.minimapCam = minimapCam;

        spriteRenderer.sprite = mapIcon;
        spriteRenderer.color = color;

        transform.eulerAngles = Vector3.right * 90f;

        MinimapSystem.Instance.RegisterMarker(markerKey, this);
    }


    private void Update()
    {
        if (!initialized)
            return;

        if (MinimapSystem.CameraFollow == null)
            return;

        spriteRenderer.enabled = target != null;
        if (target == null)
        {
            MinimapSystem.Instance.RemoveMarker(markerKey);
            Destroy(gameObject);
            return;
        }

        transform.localScale = size * Vector3.one;

        if(!fixedSize)
        {
            transform.localScale *= minimapCam.orthographicSize / DEFAULT_SCALE;
        }

        float targetY = MinimapSystem.Minimap_Y + priority;
        if (alwaysOnMap)
        {
            Vector3 middle = MinimapSystem.CameraFollow.position;
            middle.y = targetY;

            Vector3 targetPosition = target.position;
            targetPosition.y = targetY;

            Vector3 add = Vector3.ClampMagnitude(targetPosition - middle, minimapCam.orthographicSize - .5f);
            transform.position = middle + add;
        }
        else
        {
            Vector3 targetPosition = target.position;
            targetPosition.y = targetY;
            transform.position = targetPosition;
        }

        if (rotateWithTarget)
        {
            transform.eulerAngles = new Vector3(90f, target.rotation.eulerAngles.y, 0);
        }
    }
}
