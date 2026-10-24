using System;
using UnityEngine;

[Serializable]
public class RangedAttackDefinition : EnemyAttackDefinition
{
    [SerializeField, Min(0.01f)] private float _projectileSpeed = 5f;
    [SerializeField] private Sprite _projectileSprite;

    public override IEnemyAttack CreateAttack(Transform origin, ProjectileManager projectileManager, CoreBoundary coreBoundary)
    {
        return new RangedAttack(origin, projectileManager, coreBoundary, _projectileSpeed, _projectileSprite);
    }
}
