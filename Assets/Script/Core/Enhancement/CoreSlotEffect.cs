using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class CoreSlotEffect
{
    public string EffectId { get; }
    public int MaxStackCount { get; }
    public Color SlotColor { get; }
    public int VisualPriority { get; }

    protected CoreSlotEffect(string effectId, int maxStackCount, Color slotColor, int visualPriority)
    {
        EffectId = effectId;
        MaxStackCount = maxStackCount;
        SlotColor = slotColor;
        VisualPriority = visualPriority;
    }

    public virtual bool CanAddTo(IReadOnlyList<CoreSlotEffect> existingEffects)
    {
        int stackCount = 0;

        for (int i = 0; i < existingEffects.Count; i++)
        {
            if (string.Equals(existingEffects[i].EffectId, EffectId, StringComparison.Ordinal))
                stackCount++;
        }

        return stackCount < MaxStackCount;
    }

    public virtual void AddStatModifiers(TowerStatModifiers modifiers)
    {
    }

    public virtual TowerSlotRuntimeEffect CreateTowerRuntimeEffect(Tower tower)
    {
        return null;
    }
}

public sealed class StatCoreSlotEffect : CoreSlotEffect
{
    public TowerStatType StatType { get; }
    public StatModifierType ModifierType { get; }
    public float Value { get; }
    public float MaxTotalValue { get; }

    public StatCoreSlotEffect(
        string effectId,
        int maxStackCount,
        Color slotColor,
        int visualPriority,
        TowerStatType statType,
        StatModifierType modifierType,
        float value,
        float maxTotalValue)
        : base(effectId, maxStackCount, slotColor, visualPriority)
    {
        StatType = statType;
        ModifierType = modifierType;
        Value = value;
        MaxTotalValue = maxTotalValue;
    }

    public override bool CanAddTo(IReadOnlyList<CoreSlotEffect> existingEffects)
    {
        if (!base.CanAddTo(existingEffects) || Value <= 0f)
            return false;

        if (MaxTotalValue <= 0f)
            return true;

        float totalValue = Value;

        for (int i = 0; i < existingEffects.Count; i++)
        {
            if (existingEffects[i] is not StatCoreSlotEffect statEffect ||
                !string.Equals(statEffect.EffectId, EffectId, StringComparison.Ordinal))
            {
                continue;
            }

            totalValue += statEffect.Value;
        }

        return totalValue <= MaxTotalValue + 0.0001f;
    }

    public override void AddStatModifiers(TowerStatModifiers modifiers)
    {
        modifiers.TryAdd(StatType, ModifierType, Value);
    }
}

public sealed class HealthRegenerationCoreSlotEffect : CoreSlotEffect
{
    public float FlatHealing { get; }
    public float MaxHpPercentHealing { get; }
    public float Interval { get; }

    public HealthRegenerationCoreSlotEffect(
        string effectId,
        int maxStackCount,
        Color slotColor,
        int visualPriority,
        float flatHealing,
        float maxHpPercentHealing,
        float interval)
        : base(effectId, maxStackCount, slotColor, visualPriority)
    {
        FlatHealing = flatHealing;
        MaxHpPercentHealing = maxHpPercentHealing;
        Interval = interval;
    }

    public override bool CanAddTo(IReadOnlyList<CoreSlotEffect> existingEffects)
    {
        return base.CanAddTo(existingEffects) &&
            (FlatHealing > 0f || MaxHpPercentHealing > 0f) &&
            Interval > 0f;
    }

    public override TowerSlotRuntimeEffect CreateTowerRuntimeEffect(Tower tower)
    {
        return new HealthRegenerationRuntimeEffect(tower, this);
    }
}
