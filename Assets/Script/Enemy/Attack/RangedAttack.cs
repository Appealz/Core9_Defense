using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class RangedAttack : IEnemyAttack
{
    private readonly Transform _origin;
    private readonly ProjectileManager _projectileManager;
    private readonly CoreBoundary _coreBoundary;
    private readonly IProjectileImpact _impact;
    private readonly float _projectileSpeed;
    private readonly Sprite _projectileSprite;

    public bool IsRanged => true;

    public RangedAttack(Transform origin, ProjectileManager projectileManager, CoreBoundary coreBoundary, float projectileSpeed, Sprite projectileSprite)
    {
        _origin = origin != null ? origin : throw new ArgumentNullException(nameof(origin));
        _projectileManager = projectileManager ?? throw new ArgumentNullException(nameof(projectileManager));
        _coreBoundary = coreBoundary != null ? coreBoundary : throw new ArgumentNullException(nameof(coreBoundary));

        _projectileSpeed = Mathf.Max(0.01f, projectileSpeed);
        _projectileSprite = projectileSprite;
        _impact = new CoreHitImpact(coreBoundary);
    }

    public void Attack(float damage)
    {
        if (!_coreBoundary.CanReceiveDamage || damage <= 0f)
            return;

        Vector3 hitPosition = _coreBoundary.GetClosestPoint(_origin.position);

        _projectileManager.FireFixedAsync(_origin.position, hitPosition, damage, _projectileSpeed, _impact, _projectileSprite).Forget();
    }
}
