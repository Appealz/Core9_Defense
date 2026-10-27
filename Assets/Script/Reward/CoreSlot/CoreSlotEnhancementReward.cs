using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public sealed class CoreSlotEnhancementReward : IReward
{
    private readonly CoreSlotEnhancementSelectionController _selectionController;
    private readonly CoreSlotPatternData _pattern;
    private readonly IReadOnlyList<CoreSlotEffect> _effects;

    public CoreSlotEnhancementReward(
        CoreSlotEnhancementSelectionController selectionController,
        CoreSlotPatternData pattern,
        IReadOnlyList<CoreSlotEffect> effects)
    {
        _selectionController = selectionController;
        _pattern = pattern;
        _effects = effects;
    }

    public bool CanApply()
    {
        return _selectionController != null &&
            _pattern != null &&
            _effects != null &&
            _effects.Count > 0 &&
            _selectionController.HasValidPlacement(_pattern, _effects);
    }

    public async UniTask<bool> ApplyAsync()
    {
        if (!CanApply())
            return false;

        return await _selectionController.SelectAndApplyAsync(_pattern, _effects);
    }
}
