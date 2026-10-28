using System;

public interface ITowerGoldUpgradeApplier
{
    event Action<Tower> TowerRemoved;

    bool IsTowerActive(Tower tower);
    bool CanApplyTowerGoldUpgrade(Tower tower, TowerStatType statType, float flatValue);
    bool TryApplyTowerGoldUpgrade(Tower tower, TowerStatType statType, float flatValue);
}
