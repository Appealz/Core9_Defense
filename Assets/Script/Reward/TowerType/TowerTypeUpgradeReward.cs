using Cysharp.Threading.Tasks;

public sealed class TowerTypeUpgradeReward : IReward
{
    private readonly TowerManager _towerManager;
    private readonly TowerTypeUpgradeData _upgradeData;
    private readonly int _targetLevel;

    public TowerTypeUpgradeReward(TowerManager towerManager, TowerTypeUpgradeData upgradeData, int targetLevel)
    {
        _towerManager = towerManager;
        _upgradeData = upgradeData;
        _targetLevel = targetLevel;
    }

    public bool CanApply()
    {
        return _upgradeData != null &&
            _towerManager.HasTowerType(_upgradeData.TowerType) &&
            _towerManager.CanApplyTowerTypeUpgrade(_upgradeData, _targetLevel);
    }

    public UniTask<bool> ApplyAsync()
    {
        return UniTask.FromResult(_towerManager.TryApplyTowerTypeUpgrade(_upgradeData, _targetLevel));
    }
}
