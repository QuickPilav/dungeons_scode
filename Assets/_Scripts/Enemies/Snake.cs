using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snake : BasicEnemy
{
    public override Enemies EnemyType => Enemies.Snake;

    public override int DefaultHealth => 50;

    protected override bool onlyTargetPlayers => true;

    protected override bool SpawnGibsOnExplosion => false;

    protected override void Initialize()
    {
        base.Initialize();

        enemyStateAttacking.OnAttackEnded += EnemyStateAttacking_OnAttackEnded;
    }

    private void EnemyStateAttacking_OnAttackEnded()
    {
        SpawnManager.Instance.SpawnPoison(transform.position + transform.forward * .5f + Vector3.up * .5f);
    }
}
