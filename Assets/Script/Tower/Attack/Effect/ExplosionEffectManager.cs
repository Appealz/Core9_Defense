using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public sealed class ExplosionEffectManager : IUpdatable, IImpactEffect
{
    private readonly ObjectPool<ExplosionEffect> _effectPool;
    private readonly List<ExplosionEffect> _activeEffects = new(16);

    public ExplosionEffectManager(ObjectPool<ExplosionEffect> effectPool)
    {
        _effectPool = effectPool ?? throw new ArgumentNullException(nameof(effectPool));
    }

    public void Play(Vector3 position, float radius)
    {
        PlayAsync(position, radius).Forget();
    }

    public void Update(float deltaTime)
    {
        for (int i = _activeEffects.Count - 1; i >= 0; i--)
        {
            ExplosionEffect effect = _activeEffects[i];

            if (effect.IsAlive)
                continue;

            Release(i, effect);
        }
    }

    private async UniTask PlayAsync(Vector3 position, float radius)
    {
        ExplosionEffect effect = await _effectPool.GetAsync();

        effect.Play(position, radius);

        _activeEffects.Add(effect);
    }

    private void Release(int index, ExplosionEffect effect)
    {
        _activeEffects.RemoveAt(index);

        effect.Clear();
        _effectPool.Return(effect);
    }
}