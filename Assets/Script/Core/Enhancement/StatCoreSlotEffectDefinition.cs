using UnityEngine;

[CreateAssetMenu(fileName = "CoreSlotStatEffect", menuName = "Core9/Core Slot Effects/Stat")]
public sealed class StatCoreSlotEffectDefinition : CoreSlotEffectDefinition
{
    [SerializeField] private TowerStatType _statType;
    [SerializeField] private StatModifierType _modifierType;
    [SerializeField, Min(0f)] private float _value = 1f;
    [Tooltip("Zero means unlimited. The limit is checked per slot and effect id.")]
    [SerializeField, Min(0f)] private float _maxTotalValue;

    public override string GetDescription()
    {
        string statName = _statType == TowerStatType.AttackDamage ? "Attack Damage" : "Attack Speed";

        if (_modifierType == StatModifierType.Percent)
            return $"{statName} +{_value * 100f:0.##}%";

        return $"{statName} +{_value:0.##}";
    }

    protected override CoreSlotEffect CreateEffect(
        string effectId,
        int maxStackCount,
        Color slotColor,
        int visualPriority)
    {
        return new StatCoreSlotEffect(
            effectId,
            maxStackCount,
            slotColor,
            visualPriority,
            _statType,
            _modifierType,
            Mathf.Max(0f, _value),
            Mathf.Max(0f, _maxTotalValue)
        );
    }
}
