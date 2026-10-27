using System;
using UnityEngine;

[Serializable]
public class BombMissileAttackDefinition : TowerAttackDefinition
{
    [SerializeField] private float _projectileSpeed = 7f;
    [SerializeField] private Sprite _projectileSprite;
    [SerializeField] private float _explosionRadius = 2f;

    public override ITowerAttack CreateAttack(
        TowerAttackDependencies dependencies,
        TowerUniqueModifiers uniqueModifiers)
    {
        return new BombMissileAttack(dependencies.ProjectileManager, dependencies.ImpactEffect, _projectileSpeed, _projectileSprite, _explosionRadius);
    }
}
