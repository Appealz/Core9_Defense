using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class LaserAttack : ITargetedTowerAttack
{
    private readonly LaserManager _laserManager;
    private readonly float _width;
    private readonly float _duration;
    private readonly float _damageInterval;

    public LaserAttack(LaserManager laserManager, float width, float duration, float damageInterval)
    {
        _laserManager = laserManager ?? throw new ArgumentNullException(nameof(laserManager));
        _width = width;
        _duration = duration;
        _damageInterval = damageInterval;
    }

    public void Attack(Vector3 origin, Enemy target, TowerStats stats)
    {
        if (target == null || !target.IsAlive)
            return;

        Vector3 direction = target.Position - origin;

        _laserManager.FireAsync(
            origin,
            direction,
            stats.AttackDamage,
            stats.AttackRange,
            _width,
            _duration,
            _damageInterval).Forget();
    }
}