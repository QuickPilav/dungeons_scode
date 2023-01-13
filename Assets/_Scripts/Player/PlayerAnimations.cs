using UnityEngine;

[System.Serializable]
public class PlayerAnimations
{
    private const float lerpSpeed = 25f;

    [SerializeField] private Animator anim;

    private int xHash;
    private int zHash;

    private float lerpedX;
    private float lerpedZ;

    private float targetX;
    private float targetZ;

    private PlayerController ply;

    public void Initialize (PlayerController ply)
    {
        this.ply = ply;
        xHash = Animator.StringToHash("x");
        zHash = Animator.StringToHash("z");
    }

    public void Update ()
    {
        lerpedX = Mathf.Lerp(lerpedX, targetX, Time.deltaTime * lerpSpeed);
        lerpedZ = Mathf.Lerp(lerpedZ, targetZ, Time.deltaTime * lerpSpeed);

        anim.SetFloat(xHash, lerpedX);
        anim.SetFloat(zHash, lerpedZ);
    }

    public void SetXZ (float targetX, float targetZ)
    {
        this.targetX = targetX;
        this.targetZ = targetZ;
    }
}
