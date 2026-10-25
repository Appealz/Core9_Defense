using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class HealerAbility : IEnemyAbility
{
    private readonly Enemy _owner;
    private readonly float _healRadius;
    private readonly float _healAmount;
    private readonly float _healInterval;
    private readonly Sprite _healAreaSprite;
    private readonly Color _healAreaColor;
    private readonly ContactFilter2D _contactFilter;
    private readonly List<Collider2D> _overlapResults = new(32);
    private readonly HashSet<Enemy> _healTargets = new();

    private RadiusAreaVisual _areaVisual;
    private float _healTimer;
    private bool _isActive;

    public HealerAbility(Enemy owner, float healRadius, float healAmount, float healInterval, Sprite healAreaSprite, Color healAreaColor)
    {
        _owner = owner != null ? owner : throw new ArgumentNullException(nameof(owner));
        _healRadius = Mathf.Max(0.01f, healRadius);
        _healAmount = Mathf.Max(0.01f, healAmount);
        _healInterval = Mathf.Max(0.01f, healInterval);
        _healAreaSprite = healAreaSprite;
        _healAreaColor = healAreaColor;

        ContactFilter2D contactFilter = new();
        contactFilter.SetLayerMask(LayerMask.GetMask("Enemy"));
        _contactFilter = contactFilter;
    }

    public void AbilityUpdate(float deltaTime, bool isMovementStopped)
    {
        if (!_owner.IsAlive || !isMovementStopped)
        {
            if (_isActive)
                Stop();

            return;
        }

        if (!_isActive)
        {
            Activate();
            HealNearbyEnemies();
            _healTimer = _healInterval;
            return;
        }

        _healTimer -= deltaTime;

        if (_healTimer > 0f)
            return;

        HealNearbyEnemies();
        _healTimer = _healInterval;
    }

    public void Stop()
    {
        _isActive = false;
        _healTimer = 0f;
        _overlapResults.Clear();
        _healTargets.Clear();
        _areaVisual?.Hide();
    }

    private void Activate()
    {
        _isActive = true;

        if (_healAreaSprite == null)
            return;

        _areaVisual = _owner.GetComponent<RadiusAreaVisual>();

        if (_areaVisual == null)
            _areaVisual = _owner.gameObject.AddComponent<RadiusAreaVisual>();

        _areaVisual.Show(_healAreaSprite, _healAreaColor, _healRadius);
    }

    private void HealNearbyEnemies()
    {
        _overlapResults.Clear();
        _healTargets.Clear();

        Physics2D.OverlapCircle(_owner.Position, _healRadius, _contactFilter, _overlapResults);

        for (int i = 0; i < _overlapResults.Count; i++)
        {
            Collider2D hit = _overlapResults[i];

            if (hit == null || !hit.TryGetComponent(out Enemy target))
                continue;

            if (target == _owner || !target.CanReceiveHealing || !_healTargets.Add(target))
                continue;

            target.Health.TryHeal(_healAmount);
        }
    }
}
