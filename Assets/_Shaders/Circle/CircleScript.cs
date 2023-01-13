using UnityEngine;

public class CircleScript : MonoBehaviour
{
    public static readonly int posId = Shader.PropertyToID("_Position");
    public static readonly int sizeId = Shader.PropertyToID("_Size");

    private Material wallMat;
    private Camera cam;

    [SerializeField] private float sizeMultiplier = 1f;
    [SerializeField] private float yMultiplier = .2f;
    [SerializeField] private float speed = 5f;

    private bool initialized;

    private float lerpedSize;
    private float targetSize;

    private float checkTimer;
    private readonly float checkRate = .1f;


    public void Initialize (Camera cam)
    {
        this.cam = cam;
        wallMat = SceneLoadedHandler.GetSceneAs<GameScene>().WallMaterial;
        initialized = true;

        wallMat.SetVector(posId, Vector3.zero);
        wallMat.SetFloat(sizeId, 0);
    }


    private void Update ()
    {
        if (!initialized)
            return;

        checkTimer += Time.deltaTime;

        while (checkTimer > checkRate)
        {
            var dir = cam.transform.position - transform.position;
            var ray = new Ray(transform.position, dir.normalized);

            if (Physics.Raycast(ray, 20, LayerManager.HitLayer, QueryTriggerInteraction.Ignore))
            {
                targetSize = 1f;
            }
            else
            {
                targetSize = 0f;
            }

            checkTimer -= checkRate;
        }

        lerpedSize = Mathf.Lerp(lerpedSize, targetSize * sizeMultiplier, Time.deltaTime * speed);
        wallMat.SetFloat(sizeId, lerpedSize);

        var view = cam.WorldToViewportPoint(transform.position);
        wallMat.SetVector(posId, view + Vector3.up * yMultiplier);
    }
}
