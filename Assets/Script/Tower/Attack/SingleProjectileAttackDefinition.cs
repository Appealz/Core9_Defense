using System;
using UnityEngine;

[Serializable]
public class SingleProjectileAttackDefinition : TowerAttackDefinition
{    
    public SingleProjectileAttackDefinition()
    {
        
    }

    public override ITowerAttack CreateAttack()
    {

        return new SingleProjectileAttack();
    }
}
