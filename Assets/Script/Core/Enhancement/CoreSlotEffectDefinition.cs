using UnityEngine;

public abstract class CoreSlotEffectDefinition : ScriptableObject
{
    [Header("Stacking")]
    [Tooltip("Effects with the same id share the per-slot stack limit.")]
    [SerializeField] private string _effectId;
    [SerializeField, Min(1)] private int _maxStackCount = 99;

    [Header("Visualization")]
    [SerializeField] private Color _slotColor = Color.white;
    [SerializeField] private int _visualPriority;

    public CoreSlotEffect CreateEffect()
    {
        string effectId = string.IsNullOrWhiteSpace(_effectId) ? name : _effectId;
        return CreateEffect(effectId, Mathf.Max(1, _maxStackCount), _slotColor, _visualPriority);
    }

    public abstract string GetDescription();

    protected abstract CoreSlotEffect CreateEffect(
        string effectId,
        int maxStackCount,
        Color slotColor,
        int visualPriority);
}
