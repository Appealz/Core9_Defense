using System.Collections.Generic;
using UnityEngine;

public sealed class AreaAttack : INonTargetedTowerAttack
{
    private readonly ContactFilter2D _contactFilter;
    private readonly List<Collider2D> _results = new(32);

    public AreaAttack()
    {
        ContactFilter2D contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(LayerMask.GetMask("Enemy"));

        _contactFilter = contactFilter;
    }

    public void Attack(Vector3 origin, TowerStats stats, float damage)
    {
        _results.Clear();

        Physics2D.OverlapCircle(origin, stats.AttackRange, _contactFilter, _results);

        for (int i = 0; i < _results.Count; i++)
        {
            Collider2D hit = _results[i];

            if (hit == null || !hit.TryGetComponent(out Enemy enemy) || !enemy.IsAlive)
                continue;

            enemy.TakeDamage(damage);
        }
    }
}
