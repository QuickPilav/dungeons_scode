using TMPro;
using UnityEngine;

public class DamagePopup : PoolObject
{
    private const float ALIVE_TIME = 3f;
    private const float DEFAULT_FONT_SIZE = 30;

    [SerializeField] private TextMeshProUGUI damageText;
    private bool initialized;

    private Camera plyCam;
    private Vector3 pos;

    private float timePassed;


    private void Awake ()
    {
        plyCam = CameraSystem.Instance.Camera;
    }

    public void Initialize (Vector3 pos, int damage, Color col, float fontSizeMultiplier)
    {
        this.pos = pos;
        damageText.text = damage.ToString();
        damageText.color = col;
        initialized = true;
        damageText.fontSize = DEFAULT_FONT_SIZE * fontSizeMultiplier;
    }

    public override void OnObjectReuseBeforeActivation ()
    {
        timePassed = 0;
        initialized = false;
    }

    public override void OnObjectReused () 
    {

    }

    private void Update ()
    {
        if (!initialized)
            return;

        transform.position = plyCam.WorldToScreenPoint(pos);

        timePassed += Time.deltaTime;
        if (timePassed > ALIVE_TIME)
        {
            Destroy();
        }
    }
}
