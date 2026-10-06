using System;
using UnityEngine;

[Serializable]
public class BombMissileAttackDefinition : TowerAttackDefinition
{
    [SerializeField] private float _projectileSpeed = 7f;
    [SerializeField] private Sprite _projectileSprite;

    public override ITowerAttack CreateAttack(ProjectileManager projectileManager)
    {
        return new BombMissileAttack(projectileManager, _projectileSpeed, _projectileSprite);
    }
}