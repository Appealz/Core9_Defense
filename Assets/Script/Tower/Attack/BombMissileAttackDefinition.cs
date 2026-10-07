using System;
using UnityEngine;

[Serializable]
public class BombMissileAttackDefinition : TowerAttackDefinition, IImpactEffectAttackDefinition
{
    [SerializeField] private float _projectileSpeed = 7f;
    [SerializeField] private Sprite _projectileSprite;
    [SerializeField] private float _explosionRadius = 2f;

    public override ITowerAttack CreateAttack(ProjectileManager projectileManager)
    {
        throw new InvalidOperationException("BombMissileAttack은 IImpactEffect가 필요합니다.");
    }

    public ITowerAttack CreateAttack(ProjectileManager projectileManager, IImpactEffect impactEffect)
    {
        return new BombMissileAttack(projectileManager,impactEffect, _projectileSpeed, _projectileSprite, _explosionRadius);
    }
}