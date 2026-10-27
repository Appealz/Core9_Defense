using System;
using System.Collections.Generic;

public sealed class TowerTypeUpgradeState
{
    private readonly Dictionary<TowerType, TowerTypeUpgradeData> _dataByType = new();
    private readonly Dictionary<TowerType, int> _levelByType = new();
    private readonly Dictionary<TowerType, TowerStatModifiers> _statModifiersByType = new();
    private readonly Dictionary<TowerType, TowerUniqueModifiers> _uniqueModifiersByType = new();

    public TowerTypeUpgradeState(IReadOnlyList<TowerTypeUpgradeData> upgradeDataList)
    {
        foreach (TowerType towerType in Enum.GetValues(typeof(TowerType)))
        {
            _levelByType[towerType] = 0;
            _statModifiersByType[towerType] = new TowerStatModifiers();
            _uniqueModifiersByType[towerType] = new TowerUniqueModifiers();
        }

        if (upgradeDataList == null)
            return;

        for (int i = 0; i < upgradeDataList.Count; i++)
        {
            TowerTypeUpgradeData data = upgradeDataList[i];

            if (data != null)
                _dataByType[data.TowerType] = data;
        }
    }

    public int GetLevel(TowerType towerType) => _levelByType[towerType];

    public int GetMaxLevel(TowerType towerType)
    {
        return _dataByType.TryGetValue(towerType, out TowerTypeUpgradeData data) ? data.MaxLevel : 0;
    }

    public TowerStatModifiers GetStatModifiers(TowerType towerType) => _statModifiersByType[towerType];
    public TowerUniqueModifiers GetUniqueModifiers(TowerType towerType) => _uniqueModifiersByType[towerType];

    public bool CanApply(TowerTypeUpgradeData data, int targetLevel)
    {
        return data != null &&
            _dataByType.TryGetValue(data.TowerType, out TowerTypeUpgradeData registeredData) &&
            registeredData == data &&
            targetLevel == GetLevel(data.TowerType) + 1 &&
            targetLevel <= data.MaxLevel &&
            data.TryGetLevel(targetLevel, out _);
    }

    public bool TryApply(TowerTypeUpgradeData data, int targetLevel)
    {
        if (!CanApply(data, targetLevel) || !data.TryGetLevel(targetLevel, out TowerTypeUpgradeLevelData levelData))
            return false;

        TowerStatModifiers statModifiers = GetStatModifiers(data.TowerType);
        TowerUniqueModifiers uniqueModifiers = GetUniqueModifiers(data.TowerType);

        for (int i = 0; i < levelData.StatUpgrades.Count; i++)
        {
            TowerStatUpgradeValue upgrade = levelData.StatUpgrades[i];

            if (!upgrade.IsValid)
                return false;
        }

        for (int i = 0; i < levelData.UniqueUpgrades.Count; i++)
        {
            TowerUniqueUpgradeValue upgrade = levelData.UniqueUpgrades[i];

            if (!upgrade.IsValid)
                return false;
        }

        for (int i = 0; i < levelData.StatUpgrades.Count; i++)
        {
            TowerStatUpgradeValue upgrade = levelData.StatUpgrades[i];

            if (!statModifiers.TryAdd(upgrade.StatType, upgrade.ModifierType, upgrade.Value))
                return false;
        }

        for (int i = 0; i < levelData.UniqueUpgrades.Count; i++)
        {
            TowerUniqueUpgradeValue upgrade = levelData.UniqueUpgrades[i];

            if (!uniqueModifiers.TryAdd(upgrade.StatType, upgrade.ModifierType, upgrade.Value))
                return false;
        }

        _levelByType[data.TowerType] = targetLevel;
        return true;
    }
}
