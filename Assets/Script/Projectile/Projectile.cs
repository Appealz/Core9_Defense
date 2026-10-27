using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Projectile : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    private Sprite _defaultSprite;

    private IDamageable _target;
    private IProjectileImpact _impact;
    private Vector3 _lastTargetPosition;
    private Vector3 _fixedDirection;
    private readonly List<RaycastHit2D> _collisionResults = new(8);

    private float _damage;
    private float _speed;
    private float _remainingDistance;
    private float _remainingLifetime;
    private bool _isHoming;
    private bool _hasImpacted;
    private ProjectileCollisionTarget _collisionTarget;
    private CoreBoundary _collisionBoundary;

    private void Awake()
    {
        EnsureRenderer();
    }

    public void Initialize(Vector3 origin, IDamageable target, float damage, float speed, IProjectileImpact impact, Sprite sprite = null)
    {
        InitializeCommon(origin, damage, speed, impact, sprite);

        _target = target;
        _lastTargetPosition = target.Position;
        _isHoming = true;
        _collisionTarget = ProjectileCollisionTarget.None;
    }

    public void InitializeFixed(
        Vector3 origin,
        Vector3 direction,
        float maxDistance,
        float lifetime,
        float damage,
        float speed,
        IProjectileImpact impact,
        ProjectileCollisionTarget collisionTarget,
        CoreBoundary collisionBoundary,
        Sprite sprite = null)
    {
        InitializeCommon(origin, damage, speed, impact, sprite);

        _target = null;
        _fixedDirection = direction.normalized;
        _remainingDistance = Mathf.Max(0f, maxDistance);
        _remainingLifetime = Mathf.Max(0.01f, lifetime);
        _isHoming = false;
        _collisionTarget = collisionTarget;
        _collisionBoundary = collisionBoundary;
    }

    private void InitializeCommon(Vector3 origin, float damage, float speed, IProjectileImpact impact, Sprite sprite)
    {
        EnsureRenderer();
        transform.position = origin;

        _damage = damage;
        _speed = speed;
        _impact = impact;
        _hasImpacted = false;

        _spriteRenderer.sprite = sprite != null ? sprite : _defaultSprite;
    }

    public bool ProjectileUpdate(float deltaTime)
    {
        if (_isHoming)
            return UpdateHoming(deltaTime);

        return UpdateFixed(deltaTime);
    }

    private bool UpdateHoming(float deltaTime)
    {
        UpdateTargetState();

        Vector3 direction = _lastTargetPosition - transform.position;

        float distance = direction.magnitude;
        float moveDistance = _speed * deltaTime;

        if (distance <= moveDistance)
        {
            transform.position = _lastTargetPosition;
            ApplyImpact(_lastTargetPosition, _target);
            return false;
        }

        transform.position += direction / distance * moveDistance;
        return true;
    }

    private bool UpdateFixed(float deltaTime)
    {
        _remainingLifetime -= deltaTime;

        if (_remainingLifetime <= 0f || _remainingDistance <= 0f || _fixedDirection.sqrMagnitude <= 0f)
            return false;

        float moveDistance = Mathf.Min(_speed * deltaTime, _remainingDistance);
        Vector3 start = transform.position;
        Vector3 end = start + _fixedDirection * moveDistance;

        if (TryFindCollision(start, end, out RaycastHit2D hit, out IDamageable target))
        {
            transform.position = hit.point;
            ApplyImpact(hit.point, target);
            return false;
        }

        transform.position = end;
        _remainingDistance -= moveDistance;

        if (_remainingDistance <= 0f &&
            _collisionTarget == ProjectileCollisionTarget.CoreBoundary &&
            _collisionBoundary != null &&
            _collisionBoundary.ContainsPoint(end))
        {
            ApplyImpact(end, null);
            return false;
        }

        return _remainingDistance > 0f && _remainingLifetime > 0f;
    }

    private bool TryFindCollision(
        Vector3 start,
        Vector3 end,
        out RaycastHit2D nearestHit,
        out IDamageable target)
    {
        nearestHit = default;
        target = null;

        if (_collisionTarget == ProjectileCollisionTarget.None)
            return false;

        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = true;
        filter.SetLayerMask(_collisionTarget == ProjectileCollisionTarget.Enemy
            ? LayerMask.GetMask("Enemy")
            : Physics2D.AllLayers);

        _collisionResults.Clear();
        Physics2D.Linecast(start, end, filter, _collisionResults);

        float nearestFraction = float.MaxValue;

        for (int i = 0; i < _collisionResults.Count; i++)
        {
            RaycastHit2D candidate = _collisionResults[i];

            if (!TryResolveCollision(candidate.collider, out IDamageable candidateTarget) ||
                candidate.fraction >= nearestFraction)
            {
                continue;
            }

            nearestFraction = candidate.fraction;
            nearestHit = candidate;
            target = candidateTarget;
        }

        return nearestFraction < float.MaxValue;
    }

    private bool TryResolveCollision(Collider2D collider, out IDamageable target)
    {
        target = null;

        if (collider == null)
            return false;

        if (_collisionTarget == ProjectileCollisionTarget.Enemy)
        {
            Enemy enemy = collider.GetComponentInParent<Enemy>();

            if (enemy == null || !enemy.IsAlive)
                return false;

            target = enemy;
            return true;
        }

        return _collisionTarget == ProjectileCollisionTarget.CoreBoundary &&
            _collisionBoundary != null &&
            collider.GetComponentInParent<CoreBoundary>() == _collisionBoundary;
    }

    private void ApplyImpact(Vector3 position, IDamageable target)
    {
        if (_hasImpacted)
            return;

        _hasImpacted = true;
        _impact.Impact(position, target, _damage);
    }

    public void Reset()
    {
        _target = null;
        _impact = null;
        _lastTargetPosition = Vector3.zero;
        _fixedDirection = Vector3.zero;

        _damage = 0f;
        _speed = 0f;
        _remainingDistance = 0f;
        _remainingLifetime = 0f;
        _isHoming = false;
        _hasImpacted = false;
        _collisionTarget = ProjectileCollisionTarget.None;
        _collisionBoundary = null;
        _collisionResults.Clear();

        if (_spriteRenderer != null)
            _spriteRenderer.sprite = _defaultSprite;
    }

    private void EnsureRenderer()
    {
        if (_spriteRenderer != null)
            return;

        _spriteRenderer = GetComponent<SpriteRenderer>();

        if (_spriteRenderer != null)
            _defaultSprite = _spriteRenderer.sprite;
    }

    private void UpdateTargetState()
    {
        if (!IsTargetReferenceValid())
        {
            _target = null;
            return;
        }

        if (!_target.IsAlive)
        {
            _lastTargetPosition = _target.Position;
            _target = null;
            return;
        }

        _lastTargetPosition = _target.Position;
    }

    private bool IsTargetReferenceValid()
    {
        if (_target == null)
            return false;

        if (_target is Object unityObject && unityObject == null)
            return false;

        return true;
    }
}
