using System;
using Cysharp.Threading.Tasks;

public sealed class GlobalStatReward : IReward
{
    private readonly TowerManager _towerManager;
    private readonly TowerStatType _statType;
    private readonly StatModifierType _modifierType;
    private readonly float _value;

    public GlobalStatReward(
        TowerManager towerManager,
        TowerStatType statType,
        StatModifierType modifierType,
        float value)
    {
        _towerManager = towerManager;
        _statType = statType;
        _modifierType = modifierType;
        _value = value;
    }

    public bool CanApply()
    {
        return _towerManager != null &&
            Enum.IsDefined(typeof(TowerStatType), _statType) &&
            Enum.IsDefined(typeof(StatModifierType), _modifierType) &&
            _value > 0f &&
            !float.IsNaN(_value) &&
            !float.IsInfinity(_value);
    }

    public UniTask<bool> ApplyAsync()
    {
        bool applied = CanApply() &&
            _towerManager.TryApplyGlobalStatUpgrade(_statType, _modifierType, _value);

        return UniTask.FromResult(applied);
    }
}
