using System;
using UnityEngine;

[Serializable]
public class NewTowerRewardDefinition : RewardDefinition
{
    [SerializeField] private TowerPartData _towerPartData;

    public NewTowerRewardDefinition()
    {
    }

    public NewTowerRewardDefinition(TowerPartData towerPartData)
    {
        _towerPartData = towerPartData;
    }

    public override IReward CreateReward(RewardDependencies dependencies)
    {
        return new NewTowerReward(dependencies.TowerManager, _towerPartData);
    }
}
