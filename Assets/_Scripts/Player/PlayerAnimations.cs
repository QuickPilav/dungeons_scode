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

    public void Initialize(PlayerController ply)
    {
        this.ply = ply;
        xHash = Animator.StringToHash("x");
        zHash = Animator.StringToHash("z");
    }

    public void Update()
    {
        lerpedX = Mathf.Lerp(lerpedX, targetX, Time.deltaTime * lerpSpeed);
        lerpedZ = Mathf.Lerp(lerpedZ, targetZ, Time.deltaTime * lerpSpeed);

        anim.SetFloat(xHash, lerpedX);
        anim.SetFloat(zHash, lerpedZ);
    }

    /// <returns>returns that if we should reorientate the player?</returns>
    public void SetXZ(float targetX, float targetZ)
    {
        this.targetX = targetX;
        this.targetZ = targetZ;
    }

    public bool SetXZAndCheckReorientation(float targetX, float targetZ, float angleLimit, Vector3 mousePos, ref Vector3 reorientationDir)
    {
        SetXZ(targetX, targetZ);

        if (targetX != 0 || targetZ != 0)
            return false;

        mousePos.y = 0;

        Vector3 ourPos = anim.transform.position;
        ourPos.y = 0;

        Vector3 forwardVector = anim.transform.forward;
        forwardVector.y = 0;
        
        Vector3 mouseDir = (mousePos - ourPos).normalized;
        mouseDir.y = 0;

        Debug.DrawRay(ourPos, forwardVector, Color.green);
        Debug.DrawRay(ourPos, mouseDir, Color.blue);

        float angle = Vector3.Angle(mouseDir, forwardVector);

        reorientationDir = mouseDir;

        return angle >= angleLimit * 2;
    }
}
