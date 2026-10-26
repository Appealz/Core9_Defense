using UnityEngine;
using System;

[Serializable]
public abstract class RewardDefinition
{
    public abstract IReward CreateReward(RewardDependencies dependencies);

    public virtual string GetDescription(string fallbackDescription)
    {
        return fallbackDescription;
    }
}
