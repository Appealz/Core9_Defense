using System;
using UnityEngine;

[Serializable]
public sealed class GlobalStatRewardDefinition : RewardDefinition
{
    [Tooltip("Stat affected on every current and future tower.")]
    [SerializeField] private TowerStatType _statType;
    [Tooltip("Flat is added before Percent is multiplied.")]
    [SerializeField] private StatModifierType _modifierType;
    [Tooltip("Use 0.15 for 15%. CriticalChance Flat values are percentage points (0.05 = 5%p).")]
    [SerializeField, Min(0f)] private float _value = 1f;

    public override IReward CreateReward(RewardDependencies dependencies)
    {
        return new GlobalStatReward(dependencies.TowerManager, _statType, _modifierType, _value);
    }

    public override string GetDescription(string fallbackDescription)
    {
        string statName = GetStatName();

        if (_statType == TowerStatType.CriticalChance && _modifierType == StatModifierType.Flat)
            return $"{statName} +{_value * 100f:0.##}%p";

        if (_modifierType == StatModifierType.Percent)
            return $"{statName} +{_value * 100f:0.##}%";

        if (_statType == TowerStatType.AttackSpeed)
            return $"{statName} +{_value:0.##}/s";

        return $"{statName} +{_value:0.##}";
    }

    private string GetStatName()
    {
        return _statType switch
        {
            TowerStatType.AttackDamage => "Attack Damage",
            TowerStatType.MaxHp => "Max HP",
            TowerStatType.AttackSpeed => "Attack Speed",
            TowerStatType.CriticalChance => "Critical Chance",
            _ => "Stat"
        };
    }
}
