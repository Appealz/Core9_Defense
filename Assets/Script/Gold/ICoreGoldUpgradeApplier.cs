public interface ICoreGoldUpgradeApplier
{
    bool CanApplyCoreGoldUpgrade(TowerStatType statType, float flatValue);
    bool TryApplyCoreGoldUpgrade(TowerStatType statType, float flatValue);
}
