using System;
using UnityEngine;

[Serializable]
public class SingleProjectileAttackDefinition : TowerAttackDefinition
{
    [SerializeField] private float _projectileSpeed = 10f;

    public override ITowerAttack CreateAttack(ProjectileManager projectileManager)
    {
        return new SingleProjectileAttack(projectileManager, _projectileSpeed);
    }
}