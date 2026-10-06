using System;
using UnityEngine;

public class TowerTargetSelector
{
    private readonly EnemyQuery _enemyQuery;

    public TowerTargetSelector(EnemyQuery enemyQuery)
    {
        _enemyQuery = enemyQuery ?? throw new ArgumentNullException(nameof(enemyQuery));
    }

    public bool TryFindNearestTarget(Vector3 origin, float range, out Enemy target)
    {
        return _enemyQuery.TryFindNearest(origin, range, out target);
    }
}