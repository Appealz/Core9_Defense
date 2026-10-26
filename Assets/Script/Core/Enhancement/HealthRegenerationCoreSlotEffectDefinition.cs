using UnityEngine;

[CreateAssetMenu(fileName = "CoreSlotHealthRegeneration", menuName = "Core9/Core Slot Effects/Health Regeneration")]
public sealed class HealthRegenerationCoreSlotEffectDefinition : CoreSlotEffectDefinition
{
    [SerializeField, Min(0f)] private float _flatHealing = 1f;
    [SerializeField, Min(0f)] private float _maxHpPercentHealing;
    [SerializeField, Min(0.05f)] private float _interval = 1f;

    public override string GetDescription()
    {
        string flat = _flatHealing > 0f ? $"{_flatHealing:0.##} HP" : string.Empty;
        string percent = _maxHpPercentHealing > 0f ? $"{_maxHpPercentHealing * 100f:0.##}% Max HP" : string.Empty;
        string amount = flat.Length > 0 && percent.Length > 0 ? $"{flat} + {percent}" : flat + percent;
        return $"Regenerate {amount} every {_interval:0.##}s";
    }

    protected override CoreSlotEffect CreateEffect(
        string effectId,
        int maxStackCount,
        Color slotColor,
        int visualPriority)
    {
        return new HealthRegenerationCoreSlotEffect(
            effectId,
            maxStackCount,
            slotColor,
            visualPriority,
            Mathf.Max(0f, _flatHealing),
            Mathf.Max(0f, _maxHpPercentHealing),
            Mathf.Max(0.05f, _interval)
        );
    }
}
