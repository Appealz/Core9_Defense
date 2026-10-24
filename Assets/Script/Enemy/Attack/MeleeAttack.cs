using System;
using UnityEngine;

public class MeleeAttack : IEnemyAttack
{
    private readonly CoreBoundary _coreBoundary;

    public bool IsRanged => false;

    public MeleeAttack(CoreBoundary coreBoundary)
    {
        _coreBoundary = coreBoundary != null ? coreBoundary : throw new ArgumentNullException(nameof(coreBoundary));
    }

    public void Attack(float damage)
    {
        if (_coreBoundary.TryTakeDamage(damage))
            Debug.Log($"[Core Hit] Damage: {damage}");
    }
}
