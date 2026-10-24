using System;
using UnityEngine;

[Serializable]
public abstract class EnemyAttackDefinition
{
    public abstract IEnemyAttack CreateAttack(Transform origin, ProjectileManager projectileManager, CoreBoundary coreBoundary);
}
