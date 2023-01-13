using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Wasp : BasicEnemy
{
    protected override bool SpawnGibsOnExplosion => true;
    public override Enemies EnemyType => Enemies.Wasp;
    public override int DefaultHealth => 10;
}
