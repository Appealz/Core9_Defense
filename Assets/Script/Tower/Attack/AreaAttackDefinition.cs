using UnityEngine;
using System;

[Serializable]
public class AreaAttackDefinition : TowerAttackDefinition
{
    public override ITowerAttack CreateAttack()
    {
        return new AreaAttack();
    }
}
