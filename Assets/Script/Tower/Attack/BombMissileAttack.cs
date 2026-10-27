using Cysharp.Threading.Tasks;
using UnityEngine;

public class BombMissileAttack : ITargetedTowerAttack
{
    private readonly ProjectileManager _projectileManager;
    private readonly IProjectileImpact _impact;
    private readonly float _projectileSpeed;
    private readonly float _projectileLifetime;
    private readonly float _projectileCollisionRadius;
    private readonly Sprite _projectileSprite;

    public BombMissileAttack(
        ProjectileManager projectileManager,
        IImpactEffect impactEffect,
        float projectileSpeed,
        float projectileLifetime,
        float projectileCollisionRadius,
        Sprite projectileSprite,
        float explosionRadius)
    {
        _projectileManager = projectileManager;
        _projectileSpeed = projectileSpeed;
        _projectileLifetime = Mathf.Max(0.01f, projectileLifetime);
        _projectileCollisionRadius = Mathf.Max(0f, projectileCollisionRadius);
        _projectileSprite = projectileSprite;

        _impact = new ExplosionImpact(explosionRadius, impactEffect);
    }

    public void Attack(Vector3 origin, Enemy target, TowerStats stats, float damage)
    {
        if (target == null || !target.IsAlive)
            return;

        _projectileManager.FireAsync(
            origin,
            target,
            _projectileLifetime,
            _projectileCollisionRadius,
            damage,
            _projectileSpeed,
            _impact,
            _projectileSprite).Forget();
    }
}
