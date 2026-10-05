using System;
using UnityEngine;

[Serializable]
public abstract class TowerAttackDefinition
{
    public abstract ITowerAttack CreateAttack(ProjectileManager projectileManager);
}
