using UnityEngine;

public class TowerStats
{
    private readonly float _baseAttackDamage;
    private readonly float _baseMaxHp;
    private readonly float _baseAttackSpeed;
    private readonly float _baseAttackRange;
    private readonly float _baseCriticalChance;
    private readonly float _baseCriticalDamageMultiplier;
    private readonly float _minimumAttackInterval;
    private readonly float _criticalChanceCap;

#if UNITY_EDITOR
    private float? _debugCriticalChanceOverride;
#endif

    public float BaseAttackDamage => _baseAttackDamage;
    public float BaseMaxHp => _baseMaxHp;
    public float BaseAttackSpeed => _baseAttackSpeed;
    public float BaseAttackRange => _baseAttackRange;
    public float BaseCriticalDamageMultiplier => _baseCriticalDamageMultiplier;
    public float AttackDamage { get; private set; }
    public float MaxHp { get; private set; }
    public float AttackSpeed { get; private set; }
    public float AttackInterval { get; private set; }
    public float AttackRate => AttackInterval;
    public float AttackRange { get; private set; }
    public float CriticalChance { get; private set; }
    public float CriticalDamageMultiplier { get; private set; }

#if UNITY_EDITOR
    public float EffectiveCriticalChance => _debugCriticalChanceOverride ?? CriticalChance;
    public bool HasDebugCriticalChanceOverride => _debugCriticalChanceOverride.HasValue;
#endif

    public TowerStats(
        TowerPartData partData,
        TowerStatModifiers globalModifiers,
        TowerStatModifiers typeModifiers = null)
        : this(partData, globalModifiers, null, typeModifiers)
    {
    }

    public TowerStats(
        TowerPartData partData,
        TowerStatModifiers globalModifiers,
        TowerStatModifiers coreGoldModifiers,
        TowerStatModifiers typeModifiers)
    {
        _baseAttackDamage = Mathf.Max(0f, partData.AttackDamage);
        _baseMaxHp = Mathf.Max(1f, partData.MaxHp);
        _minimumAttackInterval = Mathf.Max(0.01f, partData.MinimumAttackInterval);

        float baseAttackInterval = Mathf.Max(_minimumAttackInterval, partData.AttackRate);
        _baseAttackSpeed = 1f / baseAttackInterval;
        _baseAttackRange = Mathf.Max(0f, partData.AttackRange);
        _baseCriticalChance = Mathf.Clamp01(partData.CriticalChance);
        _criticalChanceCap = Mathf.Clamp01(partData.CriticalChanceCap);
        _baseCriticalDamageMultiplier = Mathf.Max(1f, partData.CriticalDamageMultiplier);

        ApplyModifiers(globalModifiers, coreGoldModifiers, null, typeModifiers);
    }

    public void ApplyModifiers(TowerStatModifiers modifiers)
    {
        ApplyModifiers(modifiers, null);
    }

    public void ApplyModifiers(
        TowerStatModifiers globalModifiers,
        TowerStatModifiers slotModifiers)
    {
        ApplyModifiers(globalModifiers, slotModifiers, null);
    }

    public void ApplyModifiers(
        TowerStatModifiers globalModifiers,
        TowerStatModifiers slotModifiers,
        TowerStatModifiers typeModifiers)
    {
        ApplyModifiers(globalModifiers, null, slotModifiers, typeModifiers);
    }

    public void ApplyModifiers(
        TowerStatModifiers globalModifiers,
        TowerStatModifiers coreGoldModifiers,
        TowerStatModifiers slotModifiers,
        TowerStatModifiers typeModifiers)
    {
        ApplyModifiers(
            globalModifiers,
            coreGoldModifiers,
            slotModifiers,
            typeModifiers,
            null);
    }

    public void ApplyModifiers(
        TowerStatModifiers globalModifiers,
        TowerStatModifiers coreGoldModifiers,
        TowerStatModifiers slotModifiers,
        TowerStatModifiers typeModifiers,
        TowerStatModifiers individualGoldModifiers)
    {
        AttackDamage = CalculateModifiedStat(
            TowerStatType.AttackDamage,
            globalModifiers,
            coreGoldModifiers,
            slotModifiers,
            typeModifiers,
            individualGoldModifiers);
        MaxHp = CalculateModifiedStat(
            TowerStatType.MaxHp,
            globalModifiers,
            coreGoldModifiers,
            slotModifiers,
            typeModifiers,
            individualGoldModifiers);
        AttackSpeed = CalculateModifiedStat(
            TowerStatType.AttackSpeed,
            globalModifiers,
            coreGoldModifiers,
            slotModifiers,
            typeModifiers,
            individualGoldModifiers);
        AttackInterval = Mathf.Max(_minimumAttackInterval, 1f / AttackSpeed);
        CriticalChance = CalculateModifiedStat(
            TowerStatType.CriticalChance,
            globalModifiers,
            coreGoldModifiers,
            slotModifiers,
            typeModifiers,
            individualGoldModifiers);
        AttackRange = CalculateModifiedStat(
            TowerStatType.AttackRange,
            globalModifiers,
            coreGoldModifiers,
            slotModifiers,
            typeModifiers,
            individualGoldModifiers);
        CriticalDamageMultiplier = CalculateModifiedStat(
            TowerStatType.CriticalDamage,
            globalModifiers,
            coreGoldModifiers,
            slotModifiers,
            typeModifiers,
            individualGoldModifiers);
    }

    public TowerStatSummary CreateStatSummary(
        TowerStatModifiers globalModifiers,
        TowerStatModifiers coreGoldModifiers,
        TowerStatModifiers typeModifiers,
        TowerStatModifiers individualGoldModifiers)
    {
        TowerStatValues baseValues = new(
            _baseAttackDamage,
            _baseAttackSpeed,
            _baseMaxHp);
        TowerStatValues nonCoreValues = new(
            CalculateModifiedStat(
                TowerStatType.AttackDamage,
                globalModifiers,
                coreGoldModifiers,
                null,
                typeModifiers,
                individualGoldModifiers),
            CalculateModifiedStat(
                TowerStatType.AttackSpeed,
                globalModifiers,
                coreGoldModifiers,
                null,
                typeModifiers,
                individualGoldModifiers),
            CalculateModifiedStat(
                TowerStatType.MaxHp,
                globalModifiers,
                coreGoldModifiers,
                null,
                typeModifiers,
                individualGoldModifiers));
        TowerStatValues finalValues = new(AttackDamage, AttackSpeed, MaxHp);

        return new TowerStatSummary(baseValues, nonCoreValues, finalValues);
    }

    private float CalculateModifiedStat(
        TowerStatType statType,
        TowerStatModifiers globalModifiers,
        TowerStatModifiers coreGoldModifiers,
        TowerStatModifiers slotModifiers,
        TowerStatModifiers typeModifiers,
        TowerStatModifiers individualGoldModifiers)
    {
        float value = globalModifiers.Calculate(
            statType,
            GetBaseStat(statType),
            coreGoldModifiers,
            slotModifiers,
            typeModifiers,
            individualGoldModifiers);

        return statType switch
        {
            TowerStatType.AttackDamage => Mathf.Max(0f, value),
            TowerStatType.MaxHp => Mathf.Max(1f, value),
            TowerStatType.AttackSpeed => Mathf.Max(0.01f, value),
            TowerStatType.CriticalChance => Mathf.Clamp(value, 0f, _criticalChanceCap),
            TowerStatType.AttackRange => Mathf.Max(0f, value),
            TowerStatType.CriticalDamage => Mathf.Max(1f, value),
            _ => value
        };
    }

    private float GetBaseStat(TowerStatType statType)
    {
        return statType switch
        {
            TowerStatType.AttackDamage => _baseAttackDamage,
            TowerStatType.MaxHp => _baseMaxHp,
            TowerStatType.AttackSpeed => _baseAttackSpeed,
            TowerStatType.CriticalChance => _baseCriticalChance,
            TowerStatType.AttackRange => _baseAttackRange,
            TowerStatType.CriticalDamage => _baseCriticalDamageMultiplier,
            _ => 0f
        };
    }

    public float CalculateAttackDamage(float criticalRoll)
    {
        return CalculateAttackDamage(criticalRoll, out _);
    }

    public float CalculateAttackDamage(float criticalRoll, out bool isCritical)
    {
        float criticalChance = CriticalChance;

#if UNITY_EDITOR
        criticalChance = EffectiveCriticalChance;
#endif

        bool isValidRoll = criticalRoll >= 0f && criticalRoll <= 1f;
        isCritical = isValidRoll && (criticalChance >= 1f || criticalRoll < criticalChance);
        return isCritical ? AttackDamage * CriticalDamageMultiplier : AttackDamage;
    }

#if UNITY_EDITOR
    public void SetDebugCriticalChanceOverride(float? criticalChance)
    {
        _debugCriticalChanceOverride = criticalChance.HasValue
            ? Mathf.Clamp01(criticalChance.Value)
            : (float?)null;
    }
#endif
}
