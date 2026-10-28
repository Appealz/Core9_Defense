using System;
using UnityEngine;

public class TowerTargetSelector
{
    private readonly EnemyQuery _enemyQuery;
    private readonly CombatBoundsProvider _combatBoundsProvider;

    public TowerTargetSelector(
        EnemyQuery enemyQuery,
        CombatBoundsProvider combatBoundsProvider = null)
    {
        _enemyQuery = enemyQuery ?? throw new ArgumentNullException(nameof(enemyQuery));
        _combatBoundsProvider = combatBoundsProvider;
    }

    public bool TryFindNearestTarget(Vector3 origin, float range, out Enemy target)
    {
        return _enemyQuery.TryFindNearest(origin, range, out target);
    }

    public bool TryFindNearestTargetInCombatBounds(Vector3 origin, out Enemy target)
    {
        target = null;

        return _combatBoundsProvider != null &&
               _enemyQuery.TryFindNearestInBounds(
                   origin,
                   _combatBoundsProvider.GetBounds(),
                   out target);
    }
}
