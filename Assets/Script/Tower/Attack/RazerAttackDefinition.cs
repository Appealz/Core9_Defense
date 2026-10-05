using UnityEngine;
using System;

[Serializable]
public class RazerAttackDefinition : TowerAttackDefinition
{
    public override ITowerAttack CreateAttack(ProjectileManager projectileManager)
    {
        return new RazerAttack();
    }
}
