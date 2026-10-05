using UnityEngine;
using System;

[Serializable]
public class BombMissileAttackDefinition : TowerAttackDefinition
{
    public override ITowerAttack CreateAttack(ProjectileManager projectileManager)
    {
        return new BombMissileAttack();
    }
}
