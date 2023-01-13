public class Spider : BasicEnemy
{
    protected override bool SpawnGibsOnExplosion => true;
    public override Enemies EnemyType => Enemies.Spider;
    public override int DefaultHealth => 20;
}
