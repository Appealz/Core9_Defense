using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[CreateAssetMenu(fileName = "TowerTypeUpgradeData", menuName = "Scriptable Objects/Tower Type Upgrade Data")]
public sealed class TowerTypeUpgradeData : ScriptableObject
{
    [SerializeField] private TowerType _towerType;
    [SerializeField, Min(1)] private int _maxLevel = 3;
    [SerializeField] private List<TowerTypeUpgradeLevelData> _levels = new();

    public TowerType TowerType => _towerType;
    public int MaxLevel => Mathf.Max(1, _maxLevel);

    public bool TryGetLevel(int level, out TowerTypeUpgradeLevelData levelData)
    {
        for (int i = 0; i < _levels.Count; i++)
        {
            if (_levels[i] != null && _levels[i].Level == level)
            {
                levelData = _levels[i];
                return true;
            }
        }

        levelData = null;
        return false;
    }

#if UNITY_EDITOR
    public void Configure(TowerType towerType, int maxLevel, List<TowerTypeUpgradeLevelData> levels)
    {
        _towerType = towerType;
        _maxLevel = Mathf.Max(1, maxLevel);
        _levels = levels ?? new List<TowerTypeUpgradeLevelData>();
    }
#endif
}

[Serializable]
public sealed class TowerTypeUpgradeLevelData
{
    [SerializeField, Min(1)] private int _level = 1;
    [SerializeField] private List<TowerStatUpgradeValue> _statUpgrades = new();
    [SerializeField] private List<TowerUniqueUpgradeValue> _uniqueUpgrades = new();

    public int Level => _level;
    public IReadOnlyList<TowerStatUpgradeValue> StatUpgrades => _statUpgrades;
    public IReadOnlyList<TowerUniqueUpgradeValue> UniqueUpgrades => _uniqueUpgrades;

    public TowerTypeUpgradeLevelData(
        int level,
        List<TowerStatUpgradeValue> statUpgrades,
        List<TowerUniqueUpgradeValue> uniqueUpgrades)
    {
        _level = Mathf.Max(1, level);
        _statUpgrades = statUpgrades ?? new List<TowerStatUpgradeValue>();
        _uniqueUpgrades = uniqueUpgrades ?? new List<TowerUniqueUpgradeValue>();
    }

    public string GetDescription(TowerType towerType)
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(towerType).Append(" Lv.").Append(_level).Append(": ");

        for (int i = 0; i < _statUpgrades.Count; i++)
            AppendValue(builder, _statUpgrades[i].StatType.ToString(), _statUpgrades[i].ModifierType, _statUpgrades[i].Value);

        for (int i = 0; i < _uniqueUpgrades.Count; i++)
            AppendValue(builder, _uniqueUpgrades[i].StatType.ToString(), _uniqueUpgrades[i].ModifierType, _uniqueUpgrades[i].Value);

        return builder.ToString().TrimEnd(' ', ',');
    }

    private static void AppendValue(StringBuilder builder, string name, StatModifierType modifierType, float value)
    {
        if (builder.Length > 0 && builder[builder.Length - 1] != ' ')
            builder.Append(", ");

        builder.Append(name).Append(" +");
        builder.Append(modifierType == StatModifierType.Percent ? value * 100f : value);

        if (modifierType == StatModifierType.Percent)
            builder.Append('%');
    }
}

[Serializable]
public struct TowerStatUpgradeValue
{
    [SerializeField] private TowerStatType _statType;
    [SerializeField] private StatModifierType _modifierType;
    [SerializeField, Min(0f)] private float _value;

    public TowerStatType StatType => _statType;
    public StatModifierType ModifierType => _modifierType;
    public float Value => _value;
    public bool IsValid => Enum.IsDefined(typeof(TowerStatType), _statType) &&
        Enum.IsDefined(typeof(StatModifierType), _modifierType) &&
        _value > 0f && !float.IsNaN(_value) && !float.IsInfinity(_value);

    public TowerStatUpgradeValue(TowerStatType statType, StatModifierType modifierType, float value)
    {
        _statType = statType;
        _modifierType = modifierType;
        _value = value;
    }
}

[Serializable]
public struct TowerUniqueUpgradeValue
{
    [SerializeField] private TowerUniqueStatType _statType;
    [SerializeField] private StatModifierType _modifierType;
    [SerializeField, Min(0f)] private float _value;

    public TowerUniqueStatType StatType => _statType;
    public StatModifierType ModifierType => _modifierType;
    public float Value => _value;
    public bool IsValid => Enum.IsDefined(typeof(TowerUniqueStatType), _statType) &&
        Enum.IsDefined(typeof(StatModifierType), _modifierType) &&
        _value > 0f && !float.IsNaN(_value) && !float.IsInfinity(_value);

    public TowerUniqueUpgradeValue(TowerUniqueStatType statType, StatModifierType modifierType, float value)
    {
        _statType = statType;
        _modifierType = modifierType;
        _value = value;
    }
}
