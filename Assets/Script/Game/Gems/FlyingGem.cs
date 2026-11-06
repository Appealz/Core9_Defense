using UnityEngine;

public sealed class FlyingGem : MonoBehaviour
{
    private const float MinimumDirectionComponent = 0.08f;

    [SerializeField] private Transform _visualRoot;
    [SerializeField] private CircleCollider2D _touchCollider;
    [Header("Touch Hint")]
    [SerializeField] private GameObject _touchHintRoot;
    [SerializeField] private RectTransform _touchHintCanvasRect;
    [SerializeField] private RectTransform _touchHintLabelRoot;

    private Vector3 _baseVisualScale;
    private Vector3 _baseTouchHintLocalPosition;
    private Vector2 _baseTouchHintLabelPosition;
    private Vector2 _direction;
    private CombatBounds _movementBounds;
    private float _moveSpeed;
    private float _bounceRandomAngle;
    private float _remainingLifetime;
    private float _touchHintInterval;
    private float _touchHintDuration;
    private float _touchHintCountdown;
    private float _touchHintTimeRemaining;
    private int _touchHintBlinkCount;
    private bool _canCollect;
    private bool _isInitialized;
    private bool _defaultsCached;
    private bool _touchHintEnabled;
    private bool _touchHintPlaying;
    private bool _isTouchHintActive;

    public bool IsConfigured =>
        _visualRoot != null &&
        _touchCollider != null &&
        _touchHintRoot != null &&
        _touchHintCanvasRect != null &&
        _touchHintLabelRoot != null;
    public Vector3 WorldPosition => transform.position;

    private void Awake()
    {
        CacheDefaults();
    }

    public void Initialize(
        Vector2 position,
        Vector2 direction,
        float moveSpeed,
        float lifetime,
        CombatBounds movementBounds,
        float visualScale,
        float touchHitboxScale,
        float bounceRandomAngle,
        bool enableTouchHint,
        float touchHintInterval,
        float touchHintDuration,
        int touchHintBlinkCount)
    {
        CacheDefaults();

        transform.position = new Vector3(position.x, position.y, 0f);
        _direction = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.right;
        _moveSpeed = Mathf.Max(0f, moveSpeed);
        _remainingLifetime = Mathf.Max(0.01f, lifetime);
        _movementBounds = movementBounds;
        _bounceRandomAngle = Mathf.Clamp(bounceRandomAngle, 0f, 45f);
        _visualRoot.localScale = _baseVisualScale * Mathf.Max(0.1f, visualScale);
        _touchCollider.radius = 0.5f * Mathf.Max(0.1f, visualScale) * Mathf.Max(1f, touchHitboxScale);
        _touchCollider.enabled = true;
        _canCollect = true;
        _isInitialized = true;
        ConfigureTouchHint(
            enableTouchHint,
            touchHintInterval,
            touchHintDuration,
            touchHintBlinkCount);
        gameObject.SetActive(true);
    }

    public void SetMovementBounds(CombatBounds movementBounds)
    {
        _movementBounds = movementBounds;
        Vector3 position = transform.position;
        position.x = Mathf.Clamp(position.x, movementBounds.Left, movementBounds.Right);
        position.y = Mathf.Clamp(position.y, movementBounds.Bottom, movementBounds.Top);
        transform.position = position;
    }

    public bool Tick(float unscaledDeltaTime)
    {
        if (!_isInitialized || _canCollect == false)
            return false;

        float deltaTime = Mathf.Max(0f, unscaledDeltaTime);
        _remainingLifetime -= deltaTime;

        if (_remainingLifetime <= 0f)
        {
            _canCollect = false;
            _touchCollider.enabled = false;
            StopTouchHint();
            return false;
        }

        MoveAndBounce(deltaTime);
        UpdateTouchHint(deltaTime);
        return true;
    }

    public bool TryBeginCollection(Vector2 worldPosition)
    {
        if (!_canCollect || !_touchCollider.enabled)
            return false;

        Vector2 center = transform.TransformPoint(_touchCollider.offset);
        Vector3 scale = transform.lossyScale;
        float worldRadius = _touchCollider.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y));

        if ((worldPosition - center).sqrMagnitude > worldRadius * worldRadius)
            return false;

        _canCollect = false;
        _touchCollider.enabled = false;
        StopTouchHint();
        return true;
    }

    public void SetTouchHintPlaying(bool isPlaying)
    {
        _touchHintPlaying = isPlaying;

        if (!isPlaying)
        {
            HideTouchHintVisual();
            return;
        }

        if (_isTouchHintActive)
        {
            UpdateTouchHintPosition();
            ApplyTouchHintBlink();
        }
    }

    public void ResetForPool()
    {
        CacheDefaults();
        _direction = Vector2.zero;
        _moveSpeed = 0f;
        _remainingLifetime = 0f;
        _bounceRandomAngle = 0f;
        _touchHintInterval = 0f;
        _touchHintDuration = 0f;
        _touchHintCountdown = 0f;
        _touchHintTimeRemaining = 0f;
        _touchHintBlinkCount = 0;
        _canCollect = false;
        _isInitialized = false;
        _touchHintEnabled = false;
        _touchHintPlaying = false;
        _isTouchHintActive = false;
        _touchCollider.enabled = false;
        _touchCollider.radius = 0.5f;
        _visualRoot.localScale = _baseVisualScale;
        RestoreTouchHintLayout();
        HideTouchHintVisual();
        transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
    }

    private void ConfigureTouchHint(
        bool isEnabled,
        float interval,
        float duration,
        int blinkCount)
    {
        _touchHintEnabled = isEnabled;
        _touchHintPlaying = true;
        _touchHintInterval = Mathf.Max(0.1f, interval);
        _touchHintDuration = Mathf.Max(0.1f, duration);
        _touchHintBlinkCount = Mathf.Max(1, blinkCount);
        _touchHintCountdown = _touchHintInterval;
        _touchHintTimeRemaining = 0f;
        _isTouchHintActive = false;
        RestoreTouchHintLayout();
        HideTouchHintVisual();
    }

    private void UpdateTouchHint(float deltaTime)
    {
        if (!_touchHintEnabled || !_touchHintPlaying)
            return;

        _touchHintCountdown -= deltaTime;

        if (_touchHintCountdown <= 0f)
        {
            do
            {
                _touchHintCountdown += _touchHintInterval;
            }
            while (_touchHintCountdown <= 0f);

            _isTouchHintActive = true;
            _touchHintTimeRemaining = _touchHintDuration;
        }

        if (!_isTouchHintActive)
            return;

        _touchHintTimeRemaining = Mathf.Max(0f, _touchHintTimeRemaining - deltaTime);

        if (_touchHintTimeRemaining <= 0f)
        {
            StopTouchHint();
            return;
        }

        UpdateTouchHintPosition();
        ApplyTouchHintBlink();
    }

    private void UpdateTouchHintPosition()
    {
        if (_touchHintCanvasRect == null)
            return;

        Vector3 gemPosition = transform.position;
        Vector3 canvasScale = _touchHintCanvasRect.lossyScale;
        float halfWidth = _touchHintCanvasRect.rect.width * Mathf.Abs(canvasScale.x) * 0.5f;
        float halfHeight = _touchHintCanvasRect.rect.height * Mathf.Abs(canvasScale.y) * 0.5f;
        float verticalDistance = Mathf.Max(
            Mathf.Abs(_baseTouchHintLocalPosition.y),
            halfHeight + 0.65f);
        float minimumX = _movementBounds.Left + halfWidth;
        float maximumX = _movementBounds.Right - halfWidth;
        float preferredX = gemPosition.x + _baseTouchHintLocalPosition.x;
        float centerX = minimumX <= maximumX
            ? Mathf.Clamp(preferredX, minimumX, maximumX)
            : _movementBounds.Center.x;
        float aboveY = gemPosition.y + verticalDistance;
        float belowY = gemPosition.y - verticalDistance;
        bool placeAbove = aboveY + halfHeight <= _movementBounds.Top;
        float centerY = placeAbove ? aboveY : belowY;
        float minimumY = _movementBounds.Bottom + halfHeight;
        float maximumY = _movementBounds.Top - halfHeight;

        centerY = minimumY <= maximumY
            ? Mathf.Clamp(centerY, minimumY, maximumY)
            : _movementBounds.Center.y;

        _touchHintCanvasRect.localPosition = new Vector3(
            centerX - gemPosition.x,
            centerY - gemPosition.y,
            _baseTouchHintLocalPosition.z);

        Vector2 labelPosition = _baseTouchHintLabelPosition;
        labelPosition.y = 0f;
        _touchHintLabelRoot.anchoredPosition = labelPosition;
    }

    private void ApplyTouchHintBlink()
    {
        float elapsed = _touchHintDuration - _touchHintTimeRemaining;
        float normalized = _touchHintDuration > 0f
            ? Mathf.Clamp01(elapsed / _touchHintDuration)
            : 1f;
        float cycle = normalized * _touchHintBlinkCount;
        bool isVisible = cycle - Mathf.Floor(cycle) < 0.5f;
        _touchHintRoot.SetActive(isVisible);
    }

    private void StopTouchHint()
    {
        _isTouchHintActive = false;
        _touchHintTimeRemaining = 0f;
        HideTouchHintVisual();
    }

    private void HideTouchHintVisual()
    {
        if (_touchHintRoot != null)
            _touchHintRoot.SetActive(false);
    }

    private void RestoreTouchHintLayout()
    {
        if (!_defaultsCached)
            return;

        _touchHintCanvasRect.localPosition = _baseTouchHintLocalPosition;
        _touchHintLabelRoot.anchoredPosition = _baseTouchHintLabelPosition;
    }

    private void MoveAndBounce(float deltaTime)
    {
        Vector2 nextPosition = (Vector2)transform.position + _direction * (_moveSpeed * deltaTime);
        bool hitLeft = nextPosition.x < _movementBounds.Left;
        bool hitRight = nextPosition.x > _movementBounds.Right;
        bool hitBottom = nextPosition.y < _movementBounds.Bottom;
        bool hitTop = nextPosition.y > _movementBounds.Top;

        if (hitLeft || hitRight)
            _direction.x = -_direction.x;

        if (hitBottom || hitTop)
            _direction.y = -_direction.y;

        if (hitLeft || hitRight || hitBottom || hitTop)
        {
            if (_bounceRandomAngle > 0f)
            {
                float offset = Random.Range(-_bounceRandomAngle, _bounceRandomAngle);
                _direction = Quaternion.Euler(0f, 0f, offset) * _direction;
            }

            KeepDirectionInsideBounds(hitLeft, hitRight, hitBottom, hitTop);
        }

        nextPosition.x = Mathf.Clamp(nextPosition.x, _movementBounds.Left, _movementBounds.Right);
        nextPosition.y = Mathf.Clamp(nextPosition.y, _movementBounds.Bottom, _movementBounds.Top);
        transform.position = new Vector3(nextPosition.x, nextPosition.y, 0f);
    }

    private void KeepDirectionInsideBounds(bool hitLeft, bool hitRight, bool hitBottom, bool hitTop)
    {
        if (hitLeft)
            _direction.x = Mathf.Max(MinimumDirectionComponent, Mathf.Abs(_direction.x));
        else if (hitRight)
            _direction.x = -Mathf.Max(MinimumDirectionComponent, Mathf.Abs(_direction.x));

        if (hitBottom)
            _direction.y = Mathf.Max(MinimumDirectionComponent, Mathf.Abs(_direction.y));
        else if (hitTop)
            _direction.y = -Mathf.Max(MinimumDirectionComponent, Mathf.Abs(_direction.y));

        _direction.Normalize();
    }

    private void CacheDefaults()
    {
        if (_defaultsCached || !IsConfigured)
            return;

        _baseVisualScale = _visualRoot.localScale;
        _baseTouchHintLocalPosition = _touchHintCanvasRect.localPosition;
        _baseTouchHintLabelPosition = _touchHintLabelRoot.anchoredPosition;
        _defaultsCached = true;
    }
}
