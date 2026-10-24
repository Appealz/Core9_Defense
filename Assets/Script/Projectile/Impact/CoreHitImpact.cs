using System;
using UnityEngine;

public class CoreHitImpact : IProjectileImpact
{
    private readonly CoreBoundary _coreBoundary;

    public CoreHitImpact(CoreBoundary coreBoundary)
    {
        _coreBoundary = coreBoundary != null ? coreBoundary : throw new ArgumentNullException(nameof(coreBoundary));
    }

    public void Impact(Vector3 position, IDamageable target, float damage)
    {
        _coreBoundary.TryTakeDamage(damage);
    }
}
