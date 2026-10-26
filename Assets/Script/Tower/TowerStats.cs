using UnityEngine;

public class TowerStats
{
    private readonly float _baseAttackDamage;
    private readonly float _baseMaxHp;
    private readonly float _baseAttackSpeed;
    private readonly float _baseCriticalChance;
    private readonly float _minimumAttackInterval;
    private readonly float _criticalChanceCap;

#if UNITY_EDITOR
    private float? _debugCriticalChanceOverride;
#endif

    public float BaseAttackDamage => _baseAttackDamage;
    public float BaseMaxHp => _baseMaxHp;
    public float BaseAttackSpeed => _baseAttackSpeed;
    public float AttackDamage { get; private set; }
    public float MaxHp { get; private set; }
    public float AttackSpeed { get; private set; }
    public float AttackInterval { get; private set; }
    public float AttackRate => AttackInterval;
    public float AttackRange { get; private set; }
    public float CriticalChance { get; private set; }
    public float CriticalDamageMultiplier { get; }

#if UNITY_EDITOR
    public float EffectiveCriticalChance => _debugCriticalChanceOverride ?? CriticalChance;
    public bool HasDebugCriticalChanceOverride => _debugCriticalChanceOverride.HasValue;
#endif

    public TowerStats(TowerPartData partData, TowerStatModifiers modifiers)
    {
        _baseAttackDamage = Mathf.Max(0f, partData.AttackDamage);
        _baseMaxHp = Mathf.Max(1f, partData.MaxHp);
        _minimumAttackInterval = Mathf.Max(0.01f, partData.MinimumAttackInterval);

        float baseAttackInterval = Mathf.Max(_minimumAttackInterval, partData.AttackRate);
        _baseAttackSpeed = 1f / baseAttackInterval;
        _baseCriticalChance = Mathf.Clamp01(partData.CriticalChance);
        _criticalChanceCap = Mathf.Clamp01(partData.CriticalChanceCap);

        AttackRange = Mathf.Max(0f, partData.AttackRange);
        CriticalDamageMultiplier = Mathf.Max(1f, partData.CriticalDamageMultiplier);

        ApplyModifiers(modifiers);
    }

    public void ApplyModifiers(TowerStatModifiers modifiers)
    {
        ApplyModifiers(modifiers, null);
    }

    public void ApplyModifiers(
        TowerStatModifiers globalModifiers,
        TowerStatModifiers slotModifiers)
    {
        AttackDamage = Mathf.Max(
            0f,
            globalModifiers.Calculate(TowerStatType.AttackDamage, _baseAttackDamage, slotModifiers)
        );
        MaxHp = Mathf.Max(
            1f,
            globalModifiers.Calculate(TowerStatType.MaxHp, _baseMaxHp, slotModifiers)
        );
        AttackSpeed = Mathf.Max(
            0.01f,
            globalModifiers.Calculate(TowerStatType.AttackSpeed, _baseAttackSpeed, slotModifiers)
        );
        AttackInterval = Mathf.Max(_minimumAttackInterval, 1f / AttackSpeed);
        CriticalChance = Mathf.Clamp(
            globalModifiers.Calculate(TowerStatType.CriticalChance, _baseCriticalChance, slotModifiers),
            0f,
            _criticalChanceCap
        );
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
