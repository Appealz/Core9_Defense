using System;

public static class GoldUpgradeCostUtility
{
    public static bool TryCalculate(
        int baseCost,
        CoreGoldUpgradeCostGrowth growth,
        float growthValue,
        int currentLevel,
        out int cost)
    {
        cost = 0;

        if (baseCost <= 0 || currentLevel < 0 ||
            !Enum.IsDefined(typeof(CoreGoldUpgradeCostGrowth), growth))
        {
            return false;
        }

        double rawCost = growth switch
        {
            CoreGoldUpgradeCostGrowth.Linear => baseCost + (double)growthValue * currentLevel,
            CoreGoldUpgradeCostGrowth.Exponential => baseCost * Math.Pow(growthValue, currentLevel),
            _ => double.NaN
        };

        if (double.IsNaN(rawCost) || rawCost <= 0d)
            return false;

        if (double.IsInfinity(rawCost) || rawCost >= int.MaxValue)
        {
            cost = int.MaxValue;
            return true;
        }

        cost = Math.Max(1, (int)Math.Ceiling(rawCost));
        return true;
    }
}
