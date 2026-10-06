using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Projectile : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    private Sprite _defaultSprite;

    private IDamageable _target;
    private IProjectileImpact _impact;
    private Vector3 _lastTargetPosition;

    private float _damage;
    private float _speed;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _defaultSprite = _spriteRenderer.sprite;
    }

    public void Initialize(Vector3 origin, IDamageable target, float damage, float speed, IProjectileImpact impact, Sprite sprite = null)
    {
        transform.position = origin;

        _target = target;
        _impact = impact;
        _lastTargetPosition = target.Position;

        _damage = damage;
        _speed = speed;

        _spriteRenderer.sprite = sprite != null ? sprite : _defaultSprite;
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

            _impact.Impact(_lastTargetPosition, _target, _damage);

            return false;
        }

        transform.position += direction / distance * moveDistance;

        return true;
    }

    public void Reset()
    {
        _target = null;
        _impact = null;
        _lastTargetPosition = Vector3.zero;

        _damage = 0f;
        _speed = 0f;

        _spriteRenderer.sprite = _defaultSprite;
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