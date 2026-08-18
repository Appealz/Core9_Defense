using System;
using UnityEngine;

[Serializable]
public class SingleProjectileAttackDefinition : TowerAttackDefinition
{
    public float TestValue = 30f;
    public SingleProjectileAttackDefinition()
    {
        
    }

    public override ITowerAttack CreateAttack()
    {

        return new SingleProjectileAttack();
    }
}
