using System;

public sealed class TowerStatModifiers
{
    private readonly float[] _flatValues = new float[Enum.GetValues(typeof(TowerStatType)).Length];
    private readonly float[] _percentValues = new float[Enum.GetValues(typeof(TowerStatType)).Length];

    public bool TryAdd(TowerStatType statType, StatModifierType modifierType, float value)
    {
        if (!CanModify(statType, modifierType, value))
            return false;

        int index = (int)statType;
        float currentValue = modifierType == StatModifierType.Flat
            ? _flatValues[index]
            : _percentValues[index];
        double nextValue = (double)currentValue + value;

        if (nextValue > float.MaxValue)
            return false;

        if (modifierType == StatModifierType.Flat)
            _flatValues[index] = (float)nextValue;
        else
            _percentValues[index] = (float)nextValue;

        return true;
    }

    public bool TryRemove(TowerStatType statType, StatModifierType modifierType, float value)
    {
        if (!CanModify(statType, modifierType, value))
            return false;

        int index = (int)statType;
        float currentValue = modifierType == StatModifierType.Flat
            ? _flatValues[index]
            : _percentValues[index];

        if (currentValue < value)
            return false;

        float nextValue = Math.Max(0f, currentValue - value);

        if (modifierType == StatModifierType.Flat)
            _flatValues[index] = nextValue;
        else
            _percentValues[index] = nextValue;

        return true;
    }

    public static bool CanModify(TowerStatType statType, StatModifierType modifierType, float value)
    {
        return Enum.IsDefined(typeof(TowerStatType), statType) &&
            Enum.IsDefined(typeof(StatModifierType), modifierType) &&
            value > 0f &&
            !float.IsNaN(value) &&
            !float.IsInfinity(value);
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
        return Calculate(statType, baseValue, additionalModifiers, null);
    }

    public float Calculate(
        TowerStatType statType,
        float baseValue,
        TowerStatModifiers firstAdditionalModifiers,
        TowerStatModifiers secondAdditionalModifiers)
    {
        return Calculate(
            statType,
            baseValue,
            firstAdditionalModifiers,
            secondAdditionalModifiers,
            null);
    }

    public float Calculate(
        TowerStatType statType,
        float baseValue,
        TowerStatModifiers firstAdditionalModifiers,
        TowerStatModifiers secondAdditionalModifiers,
        TowerStatModifiers thirdAdditionalModifiers)
    {
        return Calculate(
            statType,
            baseValue,
            firstAdditionalModifiers,
            secondAdditionalModifiers,
            thirdAdditionalModifiers,
            null);
    }

    public float Calculate(
        TowerStatType statType,
        float baseValue,
        TowerStatModifiers firstAdditionalModifiers,
        TowerStatModifiers secondAdditionalModifiers,
        TowerStatModifiers thirdAdditionalModifiers,
        TowerStatModifiers fourthAdditionalModifiers)
    {
        int index = (int)statType;
        float additionalFlat = (firstAdditionalModifiers?.GetFlat(statType) ?? 0f) +
            (secondAdditionalModifiers?.GetFlat(statType) ?? 0f) +
            (thirdAdditionalModifiers?.GetFlat(statType) ?? 0f) +
            (fourthAdditionalModifiers?.GetFlat(statType) ?? 0f);
        float additionalPercent = (firstAdditionalModifiers?.GetPercent(statType) ?? 0f) +
            (secondAdditionalModifiers?.GetPercent(statType) ?? 0f) +
            (thirdAdditionalModifiers?.GetPercent(statType) ?? 0f) +
            (fourthAdditionalModifiers?.GetPercent(statType) ?? 0f);

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
