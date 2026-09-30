using UnityEngine;

using System;

[Serializable]
public class ChaseMovementDefinition : EnemyMovementDefinition
{
    public override IEnemyMovement CreateMovement()
    {
        return new ChaseMovement();
    }
}
