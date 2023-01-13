using UnityEngine;

public class LayerManager : MonoBehaviour
{
    public static LayerMask HitLayer;
    public static LayerMask EnemyHitLayer;
    public static LayerMask GroundLayer;
    public static LayerMask OnlyPlayers;
    public static LayerMask OnlyEnemies;

    [SerializeField] private LayerMask hitLayer;
    [SerializeField] private LayerMask enemyHitLayer;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask onlyPlayers;
    [SerializeField] private LayerMask onlyEnemies;


    private void Awake ()
    {
        HitLayer = hitLayer;
        EnemyHitLayer = enemyHitLayer;
        GroundLayer = groundLayer;
        OnlyPlayers = onlyPlayers;
        OnlyEnemies = onlyEnemies;
    }

}
