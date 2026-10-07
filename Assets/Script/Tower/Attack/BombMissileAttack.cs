using Cysharp.Threading.Tasks;
using UnityEngine;

public class BombMissileAttack : ITargetedTowerAttack
{
    private readonly ProjectileManager _projectileManager;
    private readonly IProjectileImpact _impact;
    private readonly float _projectileSpeed;
    private readonly Sprite _projectileSprite;

    public BombMissileAttack(ProjectileManager projectileManager,IImpactEffect impactEffect, float projectileSpeed,Sprite projectileSprite, float explosionRadius)
    {
        _projectileManager = projectileManager;
        _projectileSpeed = projectileSpeed;
        _projectileSprite = projectileSprite;

        _impact = new ExplosionImpact(explosionRadius, impactEffect);
    }

    public void Attack(Vector3 origin, Enemy target, TowerStats stats)
    {
        if (target == null || !target.IsAlive)
            return;

        _projectileManager.FireAsync(origin, target, stats.AttackDamage, _projectileSpeed, _impact, _projectileSprite).Forget();
    }
}