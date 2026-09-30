using System;
using UnityEngine;

[Serializable]
public class MeleeAttackDefinition : EnemyAttackDefinition
{
    public override IEnemyAttack CreateAttack()
    {
        return new MeleeAttack();
    }
}
