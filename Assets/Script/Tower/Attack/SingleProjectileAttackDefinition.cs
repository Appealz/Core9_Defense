using System;
using UnityEngine;

[Serializable]
public class SingleProjectileAttackDefinition : TowerAttackDefinition
{
    [SerializeField] private float _projectileSpeed = 10f;
    [SerializeField, Min(0.01f)] private float _straightProjectileLifetime = 3f;

    public override ITowerAttack CreateAttack(
        TowerAttackDependencies dependencies,
        TowerUniqueModifiers uniqueModifiers)
    {
        return new SingleProjectileAttack(
            dependencies.ProjectileManager,
            _projectileSpeed,
            _straightProjectileLifetime,
            uniqueModifiers);
    }
}
