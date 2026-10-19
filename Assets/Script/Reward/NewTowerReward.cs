using UnityEngine;
using Cysharp.Threading.Tasks;

public class NewTowerReward : IReward
{
    private readonly TowerManager _towerManager;
    private readonly TowerPartData _towerPartData;

    public NewTowerReward(TowerManager towerManager, TowerPartData towerPartData)
    {
        _towerManager = towerManager;
        _towerPartData = towerPartData;
    }

    public bool CanApply()
    {
        return _towerManager.CanPlaceNextTower();
    }

    public UniTask<bool> ApplyAsync()
    {
        return _towerManager.TryPlaceNextTowerAsync(_towerPartData);
    }
}