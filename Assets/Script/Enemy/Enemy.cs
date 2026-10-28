
using System;
using UnityEngine;

[RequireComponent(typeof(EnemyVisual))]
public class Enemy : MonoBehaviour, IDamageable
{
    public EnemyHealth Health { get; private set; }
    public EnemyStats Stats { get; private set; }
    public IEnemyAttack Attack { get; private set; }
    public EnemyConfig Config { get; private set; }
    public EnemyVisual Visual { get; private set; }

    public bool IsAlive => Health != null && Health.IsAlive && !_isRemoved;
    public bool CanReceiveHealing => IsAlive && Config != null && Config.CanReceiveHealing;
    public Vector3 Position => transform.position;
    public int GoldReward { get; private set; }
    public int SpawnVersion { get; private set; }
    public bool IsRetreating => _isRetreating;
    public Bounds WorldBounds => _hitCollider != null
        ? _hitCollider.bounds
        : new Bounds(Position, Vector3.zero);

    public event Action<Enemy> Died;
    public event Action<Enemy> CoreReached;
    public event Action<Enemy> Retreated;

    private EnemyController _controller;
    private Transform _moveTarget;
    private CoreBoundary _coreBoundary;
    private IEnemyAbility _ability;
    private CameraBoundsProvider _cameraBoundsProvider;
    private Collider2D _hitCollider;

    private bool _isRemoved;
    private bool _isRetreatWaiting;
    private bool _isRetreating;
    private float _retreatWaitElapsed;
    private Vector3 _retreatDirection;

    private void Awake()
    {
        Visual = GetComponent<EnemyVisual>();
        _hitCollider = GetComponent<Collider2D>();
    }

    public void Initialize(
        EnemyHealth health,
        EnemyStats stats,
        EnemyController controller,
        IEnemyAttack attack,
        Transform moveTarget,
        EnemyConfig config,
        CameraBoundsProvider cameraBoundsProvider)
    {
        if (Health != null)
            Health.Died -= OnDied;

        Health = health;
        Stats = stats;
        Attack = attack;
        Config = config;
        _cameraBoundsProvider = cameraBoundsProvider ?? throw new ArgumentNullException(nameof(cameraBoundsProvider));
        _controller = controller;

        _ability?.Stop();
        _ability = config.AbilityDefinition?.CreateAbility(this);

        if (_moveTarget != moveTarget || _coreBoundary == null)
        {
            _moveTarget = moveTarget;
            _coreBoundary = moveTarget.GetComponentInChildren<CoreBoundary>();
        }

        if (_coreBoundary == null)
            throw new InvalidOperationException("CoreBoundary를 찾을 수 없습니다.");

        _isRemoved = false;
        _isRetreatWaiting = false;
        _isRetreating = false;
        _retreatWaitElapsed = 0f;
        _retreatDirection = Vector3.zero;
        SpawnVersion = unchecked(SpawnVersion + 1);
        GoldReward = config.GoldReward;

        Visual.Initialize(config.Sprite);

        Health.Died += OnDied;
    }

    public void ReachCore()
    {
        if (!IsAlive || Attack == null || Stats == null)
            return;

        if (_coreBoundary == null || !_coreBoundary.CanReceiveDamage)
            return;

        _isRemoved = true;
        Visual.StopSpawnSpread();

        Attack.Attack(Stats.AttackDamage);
        CoreReached?.Invoke(this);
    }

    public void TakeDamage(float damage)
    {
        if (!IsAlive)
            return;

        float hpBefore = Health.CurrentHp;

        Health.TakeDamage(damage);

        if (Health.CurrentHp < hpBefore && _isRetreatWaiting && !_isRetreating)
            _retreatWaitElapsed = 0f;

#if UNITY_EDITOR
        TowerCombatDebugTrace.LogEnemyDamage(name, damage, hpBefore, Health.CurrentHp);
#endif
    }

    public bool TryBeginRetreat()
    {
        if (!IsAlive || Config == null || Config.Role != EnemyRole.Support)
            return false;

        Visual.StopSpawnSpread();
        _ability?.Stop();
        _isRetreatWaiting = false;
        _isRetreating = true;
        _retreatWaitElapsed = 0f;

        Vector3 coreToEnemy = transform.position - _moveTarget.position;
        coreToEnemy.z = 0f;
        _retreatDirection = coreToEnemy.sqrMagnitude > 0.0001f
            ? coreToEnemy.normalized
            : Vector3.up;
        return true;
    }

    public void SetRetreatWaiting(bool shouldWait)
    {
        if (_isRetreating || Config == null || Config.Role != EnemyRole.Support || !IsAlive)
            return;

        if (!shouldWait)
        {
            _isRetreatWaiting = false;
            _retreatWaitElapsed = 0f;
            return;
        }

        _isRetreatWaiting = true;
    }

    private void Update()
    {
        if (!IsAlive)
            return;

        if (_isRetreating)
        {
            UpdateRetreat(Time.deltaTime);
            return;
        }

        if (_isRetreatWaiting)
        {
            _retreatWaitElapsed += Time.deltaTime;

            if (_retreatWaitElapsed >= Config.RetreatDelay)
            {
                TryBeginRetreat();
                return;
            }
        }

        if (_controller == null || _coreBoundary == null || Visual.IsSpawnSpreading)
            return;

        Vector3 targetPosition = _coreBoundary.GetClosestPoint(transform.position);
        bool isMovementStopped = _controller.EnemyUpdate(transform, targetPosition, Time.deltaTime);
        _ability?.AbilityUpdate(Time.deltaTime, isMovementStopped);
    }

    private void OnDied()
    {
        if (_isRemoved)
            return;

        _isRemoved = true;
        _isRetreatWaiting = false;
        _isRetreating = false;
        Visual.StopSpawnSpread();
        _ability?.Stop();

        Died?.Invoke(this);
    }

    private void UpdateRetreat(float deltaTime)
    {
        transform.position += _retreatDirection * (Config.RetreatMoveSpeed * deltaTime);

        CameraBounds bounds = _cameraBoundsProvider.GetBounds();
        float margin = Config.RetreatExitMargin;
        Vector3 position = transform.position;

        if (position.x >= bounds.Left - margin &&
            position.x <= bounds.Right + margin &&
            position.y >= bounds.Bottom - margin &&
            position.y <= bounds.Top + margin)
        {
            return;
        }

        _isRetreating = false;
        _isRemoved = true;
        Retreated?.Invoke(this);
    }

    private void OnDisable()
    {
        _ability?.Stop();
        _isRetreatWaiting = false;
        _isRetreating = false;
        _retreatWaitElapsed = 0f;
    }

    private void OnDestroy()
    {
        if (Health != null)
            Health.Died -= OnDied;

        _ability?.Stop();
    }
}
