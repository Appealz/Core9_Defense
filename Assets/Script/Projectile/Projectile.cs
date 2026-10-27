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
    private bool _isHoming;
    private int _enemyLayerMask;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _defaultSprite = _spriteRenderer.sprite;
        _enemyLayerMask = LayerMask.GetMask("Enemy");
    }

    public void Initialize(Vector3 origin, IDamageable target, float damage, float speed, IProjectileImpact impact, Sprite sprite = null)
    {
        InitializeCommon(origin, damage, speed, impact, sprite);

        _target = target;
        _lastTargetPosition = target.Position;
        _isHoming = true;
    }

    public void InitializeFixed(Vector3 origin, Vector3 targetPosition, float damage, float speed, IProjectileImpact impact, Sprite sprite = null)
    {
        InitializeCommon(origin, damage, speed, impact, sprite);

        _target = null;
        _lastTargetPosition = targetPosition;
        _isHoming = false;
    }

    private void InitializeCommon(Vector3 origin, float damage, float speed, IProjectileImpact impact, Sprite sprite)
    {
        transform.position = origin;

        _damage = damage;
        _speed = speed;
        _impact = impact;

        _spriteRenderer.sprite = sprite != null ? sprite : _defaultSprite;
    }

    public bool ProjectileUpdate(float deltaTime)
    {
        if (_isHoming)
            UpdateTargetState();

        Vector3 direction = _lastTargetPosition - transform.position;

        float distance = direction.magnitude;
        float moveDistance = _speed * deltaTime;

        if (!_isHoming && TryHitEnemyAlongPath(transform.position, direction, distance, moveDistance))
            return false;

        if (distance <= moveDistance)
        {
            transform.position = _lastTargetPosition;

            if (_isHoming)
                _impact.Impact(_lastTargetPosition, _target, _damage);

            return false;
        }

        transform.position += direction / distance * moveDistance;
        return true;
    }

    private bool TryHitEnemyAlongPath(Vector3 start, Vector3 direction, float distance, float moveDistance)
    {
        if (distance <= 0f)
            return false;

        Vector3 end = start + direction / distance * Mathf.Min(distance, moveDistance);
        RaycastHit2D hit = Physics2D.Linecast(start, end, _enemyLayerMask);

        if (hit.collider == null)
            return false;

        Enemy enemy = hit.collider.GetComponentInParent<Enemy>();

        if (enemy == null || !enemy.IsAlive)
            return false;

        transform.position = hit.point;
        _impact.Impact(hit.point, enemy, _damage);
        return true;
    }

    public void Reset()
    {
        _target = null;
        _impact = null;
        _lastTargetPosition = Vector3.zero;

        _damage = 0f;
        _speed = 0f;
        _isHoming = false;

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
