using UnityEngine;

using System;

[Serializable]
public abstract class EnemyMovementDefinition
{
    public abstract IEnemyMovement CreateMovement();
}
