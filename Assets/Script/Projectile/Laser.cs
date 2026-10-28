using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public sealed class Laser : MonoBehaviour
{
    private readonly List<Collider2D> _results = new(32);

    private LineRenderer _lineRenderer;
    private ContactFilter2D _contactFilter;

    private Vector3 _origin;
    private Vector3 _end;
    private Vector3 _direction;
    private CombatBounds _combatBounds;

    private float _damage;
    private float _length;
    private float _width;
    private float _duration;
    private float _damageInterval;

    private float _remainingTime;
    private float _damageTimer;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.useWorldSpace = true;

        ContactFilter2D contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(LayerMask.GetMask("Enemy"));

        _contactFilter = contactFilter;
    }

    public void Initialize(
        Vector3 origin,
        Vector3 end,
        CombatBounds combatBounds,
        float damage,
        float width,
        float duration,
        float damageInterval)
    {
        Vector3 segment = end - origin;

        if (segment.sqrMagnitude <= 0f)
            throw new ArgumentException("Laser 구간이 유효하지 않습니다.", nameof(end));

        if (damage <= 0f)
            throw new ArgumentOutOfRangeException(nameof(damage));

        if (width <= 0f)
            throw new ArgumentOutOfRangeException(nameof(width));

        if (duration <= 0f)
            throw new ArgumentOutOfRangeException(nameof(duration));

        if (damageInterval <= 0f)
            throw new ArgumentOutOfRangeException(nameof(damageInterval));

        _origin = origin;
        _end = end;
        _direction = segment.normalized;
        _combatBounds = combatBounds;

        _damage = damage;
        _length = segment.magnitude;
        _width = width;
        _duration = duration;
        _damageInterval = damageInterval;

        _remainingTime = _duration;
        _damageTimer = 0f;

        SetVisual();

        gameObject.SetActive(true);
    }

    public bool LaserUpdate(float deltaTime)
    {
        _remainingTime -= deltaTime;
        _damageTimer -= deltaTime;

        if (_damageTimer <= 0f)
        {
            ApplyDamage();
            _damageTimer += _damageInterval;
        }

        return _remainingTime > 0f;
    }

    public void Clear()
    {
        _origin = Vector3.zero;
        _end = Vector3.zero;
        _direction = Vector3.zero;
        _combatBounds = default;

        _damage = 0f;
        _length = 0f;
        _width = 0f;
        _duration = 0f;
        _damageInterval = 0f;

        _remainingTime = 0f;
        _damageTimer = 0f;

        _results.Clear();

        _lineRenderer.positionCount = 0;
    }

    private void ApplyDamage()
    {
        _results.Clear();

        Vector3 center = Vector3.Lerp(_origin, _end, 0.5f);
        Vector2 size = new Vector2(_length, _width);
        float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;

        Physics2D.OverlapBox(center, size, angle, _contactFilter, _results);

        for (int i = 0; i < _results.Count; i++)
        {
            Collider2D hit = _results[i];

            if (hit == null ||
                !_combatBounds.Intersects(hit.bounds) ||
                !hit.TryGetComponent(out Enemy enemy) ||
                !enemy.IsAlive)
            {
                continue;
            }

            enemy.TakeDamage(_damage);
        }
    }

    private void SetVisual()
    {
        _lineRenderer.positionCount = 5;

        _lineRenderer.SetPosition(0, _origin);
        _lineRenderer.SetPosition(1, Vector3.Lerp(_origin, _end, 0.05f));
        _lineRenderer.SetPosition(2, Vector3.Lerp(_origin, _end, 0.5f));
        _lineRenderer.SetPosition(3, Vector3.Lerp(_origin, _end, 0.95f));
        _lineRenderer.SetPosition(4, _end);

        _lineRenderer.widthMultiplier = _width;

        _lineRenderer.widthCurve = new AnimationCurve(
            new Keyframe(0f, 0.3f),
            new Keyframe(0.05f, 1f),
            new Keyframe(0.95f, 1f),
            new Keyframe(1f, 0.3f));
    }
}
