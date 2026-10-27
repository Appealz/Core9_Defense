using System;
using UnityEngine;

[Serializable]
public class AreaAttackDefinition : TowerAttackDefinition
{
    [SerializeField] private Sprite _effectSprite;
    [SerializeField, Range(0f, 1f)] private float _effectAlpha = 0.2f;

    public override ITowerAttack CreateAttack(
        TowerAttackDependencies dependencies,
        TowerUniqueModifiers uniqueModifiers)
    {
        return new AreaAttack();
    }

    public override ITowerAttackEffect CreateEffect()
    {
        return new AreaEffect(_effectSprite, _effectAlpha);
    }
}
