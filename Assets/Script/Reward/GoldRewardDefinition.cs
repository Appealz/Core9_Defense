using System;
using UnityEngine;

[Serializable]
public sealed class GoldRewardDefinition : RewardDefinition
{
    [Tooltip("Gold granted immediately when this reward is selected.")]
    [SerializeField, Min(1)] private int _amount = 100;

    public override IReward CreateReward(RewardDependencies dependencies)
    {
        return new GoldReward(dependencies.GoldManager, _amount);
    }

    public override string GetDescription(string fallbackDescription)
    {
        return $"Gold +{Mathf.Max(0, _amount)}";
    }
}
