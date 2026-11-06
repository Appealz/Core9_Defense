using System;
using System.Collections.Generic;
using UnityEngine;

public enum PermanentUpgradeTarget
{
    Core = 0,
    Tower = 1
}

public enum PermanentUpgradeKind
{
    Flat = 0,
    Rate = 1
}

[Serializable]
public sealed class PersistentUpgradeDefinition
{
    [SerializeField] private TowerStatType _statType;
    [SerializeField] private string _displayName;

    [Header("Prototype values - tune in the catalog")]
    [SerializeField, Min(0f)] private float _flatAmountPerLevel = 1f;
    [SerializeField, Min(0f)] private float _rateAmountPerLevel = 0.01f;
    [SerializeField, Min(1)] private int _flatBaseCost = 10;
    [SerializeField, Min(0)] private int _flatCostPerLevel;
    [SerializeField, Min(1)] private int _rateBaseCost = 200;
    [SerializeField, Min(0)] private int _rateCostPerLevel;
    [SerializeField, Min(1)] private int _rateUnlockFlatLevel = 10;

    public TowerStatType StatType => _statType;
    public string DisplayName => _displayName;
    public float FlatAmountPerLevel => _flatAmountPerLevel;
    public float RateAmountPerLevel => _rateAmountPerLevel;
    public int RateUnlockFlatLevel => _rateUnlockFlatLevel;

    public float GetAccumulatedAmount(PermanentUpgradeKind kind, int level)
    {
        float amountPerLevel = kind == PermanentUpgradeKind.Flat
            ? _flatAmountPerLevel
            : _rateAmountPerLevel;
        return Mathf.Max(0, level) * amountPerLevel;
    }

    public int GetNextCost(PermanentUpgradeKind kind, int currentLevel)
    {
        int baseCost = kind == PermanentUpgradeKind.Flat ? _flatBaseCost : _rateBaseCost;
        int costPerLevel = kind == PermanentUpgradeKind.Flat
            ? _flatCostPerLevel
            : _rateCostPerLevel;
        long cost = baseCost + (long)costPerLevel * Mathf.Max(0, currentLevel);
        return (int)Math.Min(int.MaxValue, Math.Max(1L, cost));
    }

    public void Configure(
        TowerStatType statType,
        string displayName,
        float flatAmountPerLevel,
        float rateAmountPerLevel,
        int flatBaseCost,
        int rateBaseCost,
        int rateUnlockFlatLevel)
    {
        _statType = statType;
        _displayName = displayName;
        _flatAmountPerLevel = Mathf.Max(0f, flatAmountPerLevel);
        _rateAmountPerLevel = Mathf.Max(0f, rateAmountPerLevel);
        _flatBaseCost = Mathf.Max(1, flatBaseCost);
        _rateBaseCost = Mathf.Max(1, rateBaseCost);
        _rateUnlockFlatLevel = Mathf.Max(1, rateUnlockFlatLevel);
    }
}

[CreateAssetMenu(
    fileName = "PersistentUpgradeCatalog",
    menuName = "Core9/Persistent Upgrade Catalog")]
public sealed class PersistentUpgradeCatalog : ScriptableObject
{
    [SerializeField] private List<PersistentUpgradeDefinition> _definitions = new();

    public IReadOnlyList<PersistentUpgradeDefinition> Definitions => _definitions;

    public bool TryGetDefinition(
        TowerStatType statType,
        out PersistentUpgradeDefinition definition)
    {
        for (int i = 0; i < _definitions.Count; i++)
        {
            PersistentUpgradeDefinition candidate = _definitions[i];

            if (candidate != null && candidate.StatType == statType)
            {
                definition = candidate;
                return true;
            }
        }

        definition = null;
        return false;
    }

    public void Configure(List<PersistentUpgradeDefinition> definitions)
    {
        _definitions = definitions ?? new List<PersistentUpgradeDefinition>();
    }
}
