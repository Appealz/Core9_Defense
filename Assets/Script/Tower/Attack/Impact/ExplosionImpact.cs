using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class ExplosionImpact : IProjectileImpact
{
    private readonly float _radius;
    private readonly IImpactEffect _effect;
    private readonly ContactFilter2D _contactFilter;
    private readonly List<Collider2D> _results = new(32);
    private readonly HashSet<Enemy> _damagedEnemies = new();

    public ExplosionImpact(float radius, IImpactEffect effect)
    {
        if (radius <= 0f)
            throw new ArgumentOutOfRangeException(nameof(radius));

        _radius = radius;
        _effect = effect ?? throw new ArgumentNullException(nameof(effect));

        ContactFilter2D contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(LayerMask.GetMask("Enemy"));

        _contactFilter = contactFilter;
    }

    public void Impact(Vector3 position, IDamageable target, float damage)
    {
        _effect.Play(position, _radius);

        _results.Clear();
        _damagedEnemies.Clear();

        Physics2D.OverlapCircle(position, _radius, _contactFilter, _results);

        for (int i = 0; i < _results.Count; i++)
        {
            Collider2D hit = _results[i];
            Enemy enemy = hit != null ? hit.GetComponentInParent<Enemy>() : null;

            if (enemy == null || !enemy.IsAlive || !_damagedEnemies.Add(enemy))
                continue;

            enemy.TakeDamage(damage);
        }
    }
}
