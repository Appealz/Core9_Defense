using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public sealed class LaserManager : IUpdatable
{
    private readonly ObjectPool<Laser> _laserPool;
    private readonly CombatBoundsProvider _combatBoundsProvider;
    private readonly List<Laser> _activeLasers = new(16);

    public LaserManager(
        ObjectPool<Laser> laserPool,
        CombatBoundsProvider combatBoundsProvider)
    {
        _laserPool = laserPool ?? throw new ArgumentNullException(nameof(laserPool));
        _combatBoundsProvider = combatBoundsProvider ??
            throw new ArgumentNullException(nameof(combatBoundsProvider));
    }

    public async UniTask FireAsync(
        Vector3 origin,
        Vector3 direction,
        float damage,
        float width,
        float duration,
        float damageInterval)
    {
        if (direction.sqrMagnitude <= 0f)
            return;

        if (damage <= 0f || width <= 0f || duration <= 0f || damageInterval <= 0f)
            return;

        CombatBounds combatBounds = _combatBoundsProvider.GetBounds();

        if (!combatBounds.TryGetRayIntersection(origin, direction, out Vector2 boundaryPoint))
            return;

        Vector3 end = new(boundaryPoint.x, boundaryPoint.y, origin.z);

        if ((end - origin).sqrMagnitude <= 0f)
            return;

        Laser laser = await _laserPool.GetAsync();

        laser.Initialize(
            origin,
            end,
            combatBounds,
            damage,
            width,
            duration,
            damageInterval);

        _activeLasers.Add(laser);
    }

    public void Update(float deltaTime)
    {
        for (int i = _activeLasers.Count - 1; i >= 0; i--)
        {
            Laser laser = _activeLasers[i];

            if (laser.LaserUpdate(deltaTime))
                continue;

            Release(i, laser);
        }
    }

    private void Release(int index, Laser laser)
    {
        _activeLasers.RemoveAt(index);

        laser.Clear();
        _laserPool.Return(laser);
    }
}
