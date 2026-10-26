using System.Collections.Generic;

public abstract class TowerSlotRuntimeEffect
{
    public abstract void Update(float deltaTime);
}

public sealed class TowerSlotEffectRuntime
{
    private readonly List<TowerSlotRuntimeEffect> _effects = new();

    public void Bind(Tower tower, IReadOnlyList<CoreSlotEffect> slotEffects)
    {
        _effects.Clear();

        if (tower == null || slotEffects == null)
            return;

        for (int i = 0; i < slotEffects.Count; i++)
        {
            TowerSlotRuntimeEffect runtimeEffect = slotEffects[i].CreateTowerRuntimeEffect(tower);

            if (runtimeEffect != null)
                _effects.Add(runtimeEffect);
        }
    }

    public void Update(float deltaTime)
    {
        for (int i = 0; i < _effects.Count; i++)
            _effects[i].Update(deltaTime);
    }

    public void Clear()
    {
        _effects.Clear();
    }
}

public sealed class HealthRegenerationRuntimeEffect : TowerSlotRuntimeEffect
{
    private readonly Tower _tower;
    private readonly HealthRegenerationCoreSlotEffect _effect;
    private float _remainingTime;

    public HealthRegenerationRuntimeEffect(Tower tower, HealthRegenerationCoreSlotEffect effect)
    {
        _tower = tower;
        _effect = effect;
        _remainingTime = effect.Interval;
    }

    public override void Update(float deltaTime)
    {
        if (_tower == null || !_tower.IsAlive)
            return;

        _remainingTime -= deltaTime;

        while (_remainingTime <= 0f)
        {
            float healing = _effect.FlatHealing + _tower.Health.MaxHp * _effect.MaxHpPercentHealing;
            _tower.Health.Heal(healing);
            _remainingTime += _effect.Interval;
        }
    }
}
