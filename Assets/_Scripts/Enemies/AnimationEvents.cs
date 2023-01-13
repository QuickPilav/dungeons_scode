using UnityEngine;

public class AnimationEvents : MonoBehaviour
{
    [SerializeField] private bool isFrog;

    private Frog frog;

    private void Awake()
    {
        if (isFrog)
        {
            frog = GetComponentInParent<Frog>();
        }
    }

    public void FrogSetMoving(int state)
    {
        frog.EnemyStateWalking.CanNotMove = state == 0;
    }
}
