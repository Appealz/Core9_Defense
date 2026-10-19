using UnityEngine;

public class RewardDependencies
{
    public TowerManager TowerManager { get; }

    public RewardDependencies(TowerManager towerManager)
    {
        TowerManager = towerManager;
    }
}