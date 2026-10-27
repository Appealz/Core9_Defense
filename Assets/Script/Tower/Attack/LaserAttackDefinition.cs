using UnityEngine;
using System;

[Serializable]
public class LaserAttackDefinition : TowerAttackDefinition
{
    [SerializeField] private float _width = 0.3f;
    [SerializeField] private float _duration = 0.5f;
    [SerializeField] private float _damageInterval = 0.1f;

    public override ITowerAttack CreateAttack(
        TowerAttackDependencies dependencies,
        TowerUniqueModifiers uniqueModifiers)
    {
        return new LaserAttack(dependencies.LaserManager, _width, _duration, _damageInterval, uniqueModifiers);
    }
}
