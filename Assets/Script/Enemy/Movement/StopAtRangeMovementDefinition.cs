
using System;

[Serializable]
public class StopAtRangeMovementDefinition : EnemyMovementDefinition
{
    public override IEnemyMovement CreateMovement()
    {
        return new StopAtRangeMovement();
    }
}
