using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class EnemyQuery
{
    private readonly EnemyManager _enemyManager;

    public EnemyQuery(EnemyManager enemyManager)
    {
        _enemyManager = enemyManager ?? throw new ArgumentNullException(nameof(enemyManager));
    }

    public bool TryFindNearest(Vector3 origin, float range, out Enemy target)
    {
        target = null;

        if (range <= 0f)
            return false;

        float rangeSqr = range * range;
        float nearestDistanceSqr = float.MaxValue;

        foreach (Enemy enemy in _enemyManager.AliveEnemies)
        {
            if (enemy == null || !enemy.IsAlive)
                continue;

            Vector3 enemyPosition = enemy.Position;

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

    public void CollectInRadius(Vector3 origin, float radius, List<Enemy> results)
    {
        if (results == null)
            throw new ArgumentNullException(nameof(results));

        results.Clear();

        if (radius <= 0f)
            return;

        float radiusSqr = radius * radius;

        foreach (Enemy enemy in _enemyManager.AliveEnemies)
        {
            if (enemy == null || !enemy.IsAlive)
                continue;

            Vector3 enemyPosition = enemy.Position;

            float deltaX = enemyPosition.x - origin.x;
            float deltaY = enemyPosition.y - origin.y;
            float distanceSqr = deltaX * deltaX + deltaY * deltaY;

            if (distanceSqr <= radiusSqr)
                results.Add(enemy);
        }
    }
}