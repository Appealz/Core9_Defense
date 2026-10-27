
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

    public event Action<Enemy> Died;
    public event Action<Enemy> CoreReached;
    public event Action<Enemy> Retreated;

    private EnemyController _controller;
    private Transform _moveTarget;
    private CoreBoundary _coreBoundary;
    private IEnemyAbility _ability;

    private bool _isRemoved;

    private void Awake()
    {
        Visual = GetComponent<EnemyVisual>();
    }

    public void Initialize(EnemyHealth health, EnemyStats stats, EnemyController controller, IEnemyAttack attack, Transform moveTarget, EnemyConfig config)
    {
        if (Health != null)
            Health.Died -= OnDied;

        Health = health;
        Stats = stats;
        Attack = attack;
        Config = config;
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

#if UNITY_EDITOR
        float hpBefore = Health.CurrentHp;
#endif

        Health.TakeDamage(damage);

#if UNITY_EDITOR
        TowerCombatDebugTrace.LogEnemyDamage(name, damage, hpBefore, Health.CurrentHp);
#endif
    }

    public bool TryBeginRetreat()
    {
        if (!IsAlive || Config == null || Config.Role != EnemyRole.Support)
            return false;

        _isRemoved = true;
        Visual.StopSpawnSpread();
        _ability?.Stop();
        Visual.PlayRetreat(Config.RetreatDuration, CompleteRetreat);
        return true;
    }

    private void Update()
    {
        if (_controller == null || _coreBoundary == null || !IsAlive || Visual.IsSpawnSpreading)
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
        Visual.StopSpawnSpread();
        _ability?.Stop();

        Died?.Invoke(this);
    }

    private void CompleteRetreat()
    {
        Retreated?.Invoke(this);
    }

    private void OnDisable()
    {
        _ability?.Stop();
    }

    private void OnDestroy()
    {
        if (Health != null)
            Health.Died -= OnDied;

        _ability?.Stop();
    }
}
