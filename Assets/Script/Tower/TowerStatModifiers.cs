using System;

public sealed class TowerStatModifiers
{
    private readonly float[] _flatValues = new float[Enum.GetValues(typeof(TowerStatType)).Length];
    private readonly float[] _percentValues = new float[Enum.GetValues(typeof(TowerStatType)).Length];

    public bool TryAdd(TowerStatType statType, StatModifierType modifierType, float value)
    {
        if (!Enum.IsDefined(typeof(TowerStatType), statType) ||
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

    public float Calculate(TowerStatType statType, float baseValue)
    {
        int index = (int)statType;
        return (baseValue + _flatValues[index]) * (1f + _percentValues[index]);
    }

    public float Calculate(
        TowerStatType statType,
        float baseValue,
        TowerStatModifiers additionalModifiers)
    {
        int index = (int)statType;
        float additionalFlat = additionalModifiers?.GetFlat(statType) ?? 0f;
        float additionalPercent = additionalModifiers?.GetPercent(statType) ?? 0f;

        return (baseValue + _flatValues[index] + additionalFlat) *
            (1f + _percentValues[index] + additionalPercent);
    }

    public float GetFlat(TowerStatType statType)
    {
        return _flatValues[(int)statType];
    }

    public float GetPercent(TowerStatType statType)
    {
        return _percentValues[(int)statType];
    }
}
