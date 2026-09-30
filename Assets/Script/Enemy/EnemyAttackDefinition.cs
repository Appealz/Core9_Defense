using UnityEngine;

using System;

[Serializable]
public abstract class EnemyAttackDefinition
{
    public abstract IEnemyAttack CreateAttack();
}
