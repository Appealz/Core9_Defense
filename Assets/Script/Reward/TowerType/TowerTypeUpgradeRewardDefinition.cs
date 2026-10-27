using System;
using UnityEngine;

[Serializable]
public sealed class TowerTypeUpgradeRewardDefinition : RewardDefinition
{
    [SerializeField] private TowerTypeUpgradeData _upgradeData;
    [SerializeField, Min(1)] private int _targetLevel = 1;

    public TowerTypeUpgradeRewardDefinition()
    {
    }

    public TowerTypeUpgradeRewardDefinition(TowerTypeUpgradeData upgradeData, int targetLevel)
    {
        _upgradeData = upgradeData;
        _targetLevel = targetLevel;
    }

    public override IReward CreateReward(RewardDependencies dependencies)
    {
        return new TowerTypeUpgradeReward(dependencies.TowerManager, _upgradeData, _targetLevel);
    }

    public override string GetDescription(string fallbackDescription)
    {
        if (_upgradeData != null && _upgradeData.TryGetLevel(_targetLevel, out TowerTypeUpgradeLevelData levelData))
            return levelData.GetDescription(_upgradeData.TowerType);

        return fallbackDescription;
    }
}
