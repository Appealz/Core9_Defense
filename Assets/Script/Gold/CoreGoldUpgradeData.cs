using System;
using UnityEngine;

[CreateAssetMenu(fileName = "CoreGoldUpgrade", menuName = "Core9/Core Gold Upgrade")]
public sealed class CoreGoldUpgradeData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string _upgradeId;
    [SerializeField] private string _displayName;
    [SerializeField] private TowerStatType _statType;

    [Header("Flat stat increase")]
    [Tooltip("Flat amount added for each purchased level. Critical chance uses 0-1 units (0.01 = 1%p).")]
    [SerializeField, Min(0f)] private float _flatIncreasePerLevel = 1f;

    [Header("Cost")]
    [SerializeField, Min(1)] private int _baseCost = 1;
    [SerializeField] private CoreGoldUpgradeCostGrowth _costGrowth = CoreGoldUpgradeCostGrowth.Linear;
    [Tooltip("Linear: gold added per current level. Exponential: multiplier raised to the current level.")]
    [SerializeField, Min(0f)] private float _costGrowthValue;

    [Header("Level limit")]
    [SerializeField] private bool _hasMaxLevel;
    [SerializeField, Min(1)] private int _maxLevel = 1;

    public string UpgradeId => _upgradeId;
    public string DisplayName => _displayName;
    public TowerStatType StatType => _statType;
    public float FlatIncreasePerLevel => _flatIncreasePerLevel;
    public int BaseCost => _baseCost;
    public CoreGoldUpgradeCostGrowth CostGrowth => _costGrowth;
    public float CostGrowthValue => _costGrowthValue;
    public bool HasMaxLevel => _hasMaxLevel;
    public int MaxLevel => _maxLevel;

    public bool IsMaxLevelReached(int currentLevel)
    {
        return _hasMaxLevel && currentLevel >= _maxLevel;
    }

    public bool TryGetNextCost(int currentLevel, out int cost)
    {
        cost = 0;

        if (!IsConfigurationValid(out _) || currentLevel < 0 || IsMaxLevelReached(currentLevel))
            return false;

        return GoldUpgradeCostUtility.TryCalculate(
            _baseCost,
            _costGrowth,
            _costGrowthValue,
            currentLevel,
            out cost);
    }

    public bool IsConfigurationValid(out string error)
    {
        if (string.IsNullOrWhiteSpace(_upgradeId))
        {
            error = "Upgrade Id is empty.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(_displayName))
        {
            error = $"Display Name is empty for '{_upgradeId}'.";
            return false;
        }

        if (!Enum.IsDefined(typeof(TowerStatType), _statType))
        {
            error = $"Stat Type is invalid for '{_upgradeId}'.";
            return false;
        }

        if (!IsFinitePositive(_flatIncreasePerLevel))
        {
            error = $"Flat Increase Per Level must be finite and positive for '{_upgradeId}'.";
            return false;
        }

        if (_baseCost <= 0)
        {
            error = $"Base Cost must be positive for '{_upgradeId}'.";
            return false;
        }

        if (!Enum.IsDefined(typeof(CoreGoldUpgradeCostGrowth), _costGrowth))
        {
            error = $"Cost Growth is invalid for '{_upgradeId}'.";
            return false;
        }

        bool isGrowthValid = _costGrowth == CoreGoldUpgradeCostGrowth.Linear
            ? IsFiniteNonNegative(_costGrowthValue)
            : IsFinitePositive(_costGrowthValue) && _costGrowthValue >= 1f;

        if (!isGrowthValid)
        {
            error = $"Cost Growth Value is invalid for '{_upgradeId}'.";
            return false;
        }

        if (_hasMaxLevel && _maxLevel <= 0)
        {
            error = $"Max Level must be positive for '{_upgradeId}'.";
            return false;
        }

        error = null;
        return true;
    }

    private static bool IsFinitePositive(float value)
    {
        return value > 0f && !float.IsNaN(value) && !float.IsInfinity(value);
    }

    private static bool IsFiniteNonNegative(float value)
    {
        return value >= 0f && !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
