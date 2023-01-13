using Photon.Pun;
using UnityEngine;



public class Frog : BasicEnemy
{
    public override Enemies EnemyType => Enemies.Frog;

    public override int DefaultHealth => 25;

    protected override bool SpawnGibsOnExplosion => false;

    [SerializeField] private EnemyStateExplotano explotanoState;

    protected override void Initialize()
    {
        base.Initialize();

        explotanoState.Initialize(this);
    }

    protected override void AttackBehaviour()
    {
        photonView.RPC(nameof(RpcExplode), RpcTarget.All);
    }

    [PunRPC]
    public void RpcExplode ()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            SetCurrentState(explotanoState);
            explotanoState.SetTarget(Enemy.ObjectTransform);
        }

        StartCoroutine(explotanoState.ExplotanoEffects());
        PlayAnimationRPC(enemyStateAnimation.GetAnimationHash((int)EnemyAnims.attacking));
    }
}
