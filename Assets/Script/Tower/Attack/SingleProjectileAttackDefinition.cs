using System;
using UnityEngine;

[Serializable]
public class SingleProjectileAttackDefinition : TowerAttackDefinition
{
    [SerializeField] private float _projectileSpeed = 10f;

    public override ITowerAttack CreateAttack(TowerAttackDependencies dependencies)
    {
        return new SingleProjectileAttack(dependencies.ProjectileManager, _projectileSpeed);
    }
}