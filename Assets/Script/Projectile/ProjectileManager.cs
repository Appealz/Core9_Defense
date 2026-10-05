using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class ProjectileManager : IUpdatable
{
    private readonly ObjectPool<Projectile> _projectilePool;
    private readonly List<Projectile> _activeProjectiles = new(128);

    public ProjectileManager(ObjectPool<Projectile> projectilePool)
    {
        _projectilePool = projectilePool ?? throw new ArgumentNullException(nameof(projectilePool));
    }

    public async UniTask FireAsync(Vector3 origin, IDamageable target, float damage, float speed)
    {
        if (target == null || !target.IsAlive)
            return;

        if (damage <= 0f || speed <= 0f)
            return;

        Projectile projectile = await _projectilePool.GetAsync();

        projectile.Initialize(origin, target, damage, speed);
        projectile.gameObject.SetActive(true);

        _activeProjectiles.Add(projectile);
    }

    public void Update(float deltaTime)
    {
        for (int i = _activeProjectiles.Count - 1; i >= 0; i--)
        {
            Projectile projectile = _activeProjectiles[i];

            if (projectile.ProjectileUpdate(deltaTime))
                continue;

            Release(i, projectile);
        }
    }

    private void Release(int index, Projectile projectile)
    {
        _activeProjectiles.RemoveAt(index);

        projectile.Reset();
        _projectilePool.Return(projectile);
    }
}