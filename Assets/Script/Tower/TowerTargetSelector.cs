using System;
using UnityEngine;

public class TowerTargetSelector
{
    private readonly EnemyManager _enemyManager;

    public TowerTargetSelector(EnemyManager enemyManager)
    {
        _enemyManager = enemyManager ?? throw new ArgumentNullException(nameof(enemyManager));
    }

    public bool TryFindNearestTarget(Vector3 origin, float range, out Enemy target)
    {
        target = null;

        if (range <= 0f)
            return false;

        float rangeSqr = range * range;
        float nearestDistanceSqr = float.MaxValue;

        foreach (Enemy enemy in _enemyManager.AliveEnemies)
        {
            if (enemy == null)
                continue;

            Vector3 enemyPosition = enemy.transform.position;

            float deltaX = enemyPosition.x - origin.x;
            float deltaY = enemyPosition.y - origin.y;
            float distanceSqr = deltaX * deltaX + deltaY * deltaY;

            if (distanceSqr > rangeSqr || distanceSqr >= nearestDistanceSqr)
                continue;

            nearestDistanceSqr = distanceSqr;
            target = enemy;
        }

        return target != null;
    }
}