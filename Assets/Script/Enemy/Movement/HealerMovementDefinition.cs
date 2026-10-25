using System;
using UnityEngine;

[Serializable]
public sealed class HealerMovementDefinition : EnemyMovementDefinition
{
    [SerializeField, Min(0f)] private float _stopDistance = 3f;

    public override IEnemyMovement CreateMovement()
    {
        return new StopAtRangeMovement(_stopDistance);
    }
}
