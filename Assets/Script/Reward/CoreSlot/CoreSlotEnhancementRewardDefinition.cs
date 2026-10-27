using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[Serializable]
public sealed class CoreSlotEnhancementRewardDefinition : RewardDefinition
{
    [SerializeField] private CoreSlotPatternData _pattern;
    [SerializeField] private List<CoreSlotEffectDefinition> _effects = new();

    public override IReward CreateReward(RewardDependencies dependencies)
    {
        List<CoreSlotEffect> effects = new();

        for (int i = 0; i < _effects.Count; i++)
        {
            if (_effects[i] != null)
                effects.Add(_effects[i].CreateEffect());
        }

        return new CoreSlotEnhancementReward(
            dependencies.CoreSlotEnhancementSelection,
            _pattern,
            effects
        );
    }

    public override string GetDescription(string fallbackDescription)
    {
        if (_pattern == null || _effects.Count == 0)
            return fallbackDescription;

        StringBuilder builder = new StringBuilder(_pattern.PatternName);
        builder.Append(": ");

        for (int i = 0; i < _effects.Count; i++)
        {
            if (_effects[i] == null)
                continue;

            if (builder[builder.Length - 1] != ' ')
                builder.Append(", ");

            builder.Append(_effects[i].GetDescription());
        }

        return builder.ToString();
    }
}
