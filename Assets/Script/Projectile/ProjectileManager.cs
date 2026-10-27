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

    public async UniTask FireAsync(
        Vector3 origin,
        IDamageable target,
        float lifetime,
        float collisionRadius,
        float damage,
        float speed,
        IProjectileImpact impact,
        Sprite sprite = null)
    {
        if (target == null || !target.IsAlive)
            return;

        if (lifetime <= 0f || collisionRadius < 0f || damage <= 0f || speed <= 0f || impact == null)
            return;

        Projectile projectile = await _projectilePool.GetAsync();

        projectile.Initialize(origin, target, lifetime, collisionRadius, damage, speed, impact, sprite);
        projectile.gameObject.SetActive(true);

        _activeProjectiles.Add(projectile);
    }

    public async UniTask FireFixedAsync(
        Vector3 origin,
        Vector3 direction,
        float maxDistance,
        float lifetime,
        float damage,
        float speed,
        IProjectileImpact impact,
        ProjectileCollisionTarget collisionTarget,
        CoreBoundary collisionBoundary = null,
        float collisionRadius = 0f,
        Sprite sprite = null)
    {
        if (direction.sqrMagnitude <= 0f || maxDistance <= 0f || lifetime <= 0f ||
            collisionRadius < 0f || damage <= 0f || speed <= 0f || impact == null ||
            collisionTarget == ProjectileCollisionTarget.None ||
            (collisionTarget == ProjectileCollisionTarget.CoreBoundary && collisionBoundary == null))
        {
            return;
        }

        Projectile projectile = await _projectilePool.GetAsync();

        projectile.InitializeFixed(
            origin,
            direction,
            maxDistance,
            lifetime,
            damage,
            speed,
            impact,
            collisionTarget,
            collisionBoundary,
            collisionRadius,
            sprite);
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
