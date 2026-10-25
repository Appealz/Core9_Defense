using System;
using UnityEngine;

[Serializable]
public sealed class HealerAbilityDefinition : EnemyAbilityDefinition
{
    [SerializeField, Min(0.01f)] private float _healRadius = 3f;
    [SerializeField, Min(0.01f)] private float _healAmount = 1f;
    [SerializeField, Min(0.01f)] private float _healInterval = 2f;
    [SerializeField] private Sprite _healAreaSprite;
    [SerializeField] private Color _healAreaColor = new(0.2f, 1f, 0.4f, 0.2f);

    public override bool CanReceiveHealing => false;

    public override IEnemyAbility CreateAbility(Enemy owner)
    {
        return new HealerAbility(owner, _healRadius, _healAmount, _healInterval, _healAreaSprite, _healAreaColor);
    }
}
