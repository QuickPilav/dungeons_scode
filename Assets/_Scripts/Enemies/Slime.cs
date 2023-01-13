public class Slime : BasicEnemy
{
    protected override int ToxicInflictionDamage => base.ToxicInflictionDamage * 20;

    protected override bool SpawnGibsOnExplosion => true;
    public override Enemies EnemyType => Enemies.Slime;
    public override int DefaultHealth => 500;
}
