using System;
using UnityEngine;

public sealed class TowerUniqueModifiers
{
    private readonly float[] _flatValues = new float[Enum.GetValues(typeof(TowerUniqueStatType)).Length];
    private readonly float[] _percentValues = new float[Enum.GetValues(typeof(TowerUniqueStatType)).Length];

    public bool TryAdd(TowerUniqueStatType statType, StatModifierType modifierType, float value)
    {
        if (!Enum.IsDefined(typeof(TowerUniqueStatType), statType) ||
            !Enum.IsDefined(typeof(StatModifierType), modifierType) ||
            value <= 0f || float.IsNaN(value) || float.IsInfinity(value))
        {
            return false;
        }

        int index = (int)statType;

        if (modifierType == StatModifierType.Flat)
            _flatValues[index] += value;
        else
            _percentValues[index] += value;

        return true;
    }

    public float Calculate(TowerUniqueStatType statType, float baseValue)
    {
        int index = (int)statType;
        return (baseValue + _flatValues[index]) * (1f + _percentValues[index]);
    }

    public int CalculateInt(TowerUniqueStatType statType, int baseValue)
    {
        return Mathf.Max(1, Mathf.RoundToInt(Calculate(statType, baseValue)));
    }

    public float GetFlat(TowerUniqueStatType statType) => _flatValues[(int)statType];
    public float GetPercent(TowerUniqueStatType statType) => _percentValues[(int)statType];
}
