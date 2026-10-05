using UnityEngine;

public class Projectile : MonoBehaviour
{
    private IDamageable _target;
    private Vector3 _lastTargetPosition;

    private float _damage;
    private float _speed;
    private bool _canDamage;

    public void Initialize(Vector3 origin, IDamageable target, float damage, float speed)
    {
        transform.position = origin;

        _target = target;
        _lastTargetPosition = target.Position;

        _damage = damage;
        _speed = speed;
        _canDamage = target.IsAlive;
    }

    public bool ProjectileUpdate(float deltaTime)
    {
        UpdateTargetState();

        Vector3 direction = _lastTargetPosition - transform.position;

        float distance = direction.magnitude;
        float moveDistance = _speed * deltaTime;

        if (distance <= moveDistance)
        {
            transform.position = _lastTargetPosition;

            if (_canDamage && IsTargetValid())
                _target.TakeDamage(_damage);

            return false;
        }

        transform.position += direction / distance * moveDistance;

        return true;
    }

    public void Reset()
    {
        _target = null;
        _lastTargetPosition = Vector3.zero;

        _damage = 0f;
        _speed = 0f;
        _canDamage = false;
    }

    private void UpdateTargetState()
    {
        if (!_canDamage)
            return;

        if (!IsTargetReferenceValid())
        {
            _canDamage = false;
            _target = null;
            return;
        }

        if (!_target.IsAlive)
        {
            _lastTargetPosition = _target.Position;
            _canDamage = false;
            _target = null;
            return;
        }

        _lastTargetPosition = _target.Position;
    }

    private bool IsTargetValid()
    {
        return IsTargetReferenceValid() && _target.IsAlive;
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