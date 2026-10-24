using UnityEngine;
using System;

[Serializable]
public class MeleeAttackDefinition : EnemyAttackDefinition
{
    public override IEnemyAttack CreateAttack(UnityEngine.Transform origin, ProjectileManager projectileManager, CoreBoundary coreBoundary)
    {
        return new MeleeAttack(coreBoundary);
    }
}

