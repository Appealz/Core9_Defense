using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class LaserAttack : ITargetedTowerAttack, ITowerTargetingPolicy
{
    private readonly LaserManager _laserManager;
    private readonly float _width;
    private readonly float _duration;
    private readonly float _damageInterval;
    private readonly TowerUniqueModifiers _uniqueModifiers;

    public LaserAttack(
        LaserManager laserManager,
        float width,
        float duration,
        float damageInterval,
        TowerUniqueModifiers uniqueModifiers)
    {
        _laserManager = laserManager ?? throw new ArgumentNullException(nameof(laserManager));
        _width = width;
        _duration = duration;
        _damageInterval = damageInterval;
        _uniqueModifiers = uniqueModifiers ?? throw new ArgumentNullException(nameof(uniqueModifiers));
    }

    public void Attack(Vector3 origin, Enemy target, TowerStats stats, float damage)
    {
        if (target == null || !target.IsAlive)
            return;

        Vector3 direction = target.Position - origin;

        _laserManager.FireAsync(
            origin,
            direction,
            damage,
            _uniqueModifiers.Calculate(TowerUniqueStatType.LaserWidth, _width),
            _duration,
            _damageInterval).Forget();
    }

    public bool TryFindTarget(
        TowerTargetSelector targetSelector,
        Vector3 origin,
        float defaultRange,
        out Enemy target)
    {
        return targetSelector.TryFindNearestTargetInCombatBounds(origin, out target);
    }
}
