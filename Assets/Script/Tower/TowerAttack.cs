using UnityEngine;

public class TowerAttack
{
    private const float TargetSearchInterval = 0.1f;

    private readonly TowerStats _stats;
    private readonly ITowerAttack _attack;
    private readonly IFireMode _fireMode;
    private readonly TowerTargetSelector _targetSelector;

    private float _attackTimer;
    private float _lastAttackInterval;

    public TowerAttack(TowerStats stats, ITowerAttack attack, IFireMode fireMode, TowerTargetSelector targetSelector)
    {
        _stats = stats;
        _attack = attack;
        _fireMode = fireMode;
        _targetSelector = targetSelector;
        _lastAttackInterval = stats.AttackInterval;
    }

    public void Update(Vector3 origin, float deltaTime)
    {
        SynchronizeAttackInterval();

        if (_attack is ITargetedTowerAttack targetedAttack)
        {
            UpdateTargeted(origin, deltaTime, targetedAttack);
            return;
        }

        if (_attack is INonTargetedTowerAttack nonTargetedAttack)
            UpdateNonTargeted(origin, deltaTime, nonTargetedAttack);
    }

    private void UpdateTargeted(Vector3 origin, float deltaTime, ITargetedTowerAttack targetedAttack)
    {
        if (_fireMode.IsFiring)
        {
            _fireMode.Update(deltaTime);
            FireReadyShots(origin, targetedAttack);

            if (!_fireMode.IsFiring)
                _attackTimer = _stats.AttackInterval;

            return;
        }

        _attackTimer -= deltaTime;

        if (_attackTimer > 0f)
            return;

        if (!_targetSelector.TryFindNearestTarget(origin, _stats.AttackRange, out Enemy target))
        {
            _attackTimer = TargetSearchInterval;
            return;
        }

        _fireMode.Start();
        FireReadyShots(origin, targetedAttack, target);

        if (!_fireMode.IsFiring)
            _attackTimer = _stats.AttackInterval;
    }

    private void UpdateNonTargeted(Vector3 origin, float deltaTime, INonTargetedTowerAttack nonTargetedAttack)
    {
        if (_fireMode.IsFiring)
        {
            _fireMode.Update(deltaTime);
            FireReadyShots(origin, nonTargetedAttack);

            if (!_fireMode.IsFiring)
                _attackTimer = _stats.AttackInterval;

            return;
        }

        _attackTimer -= deltaTime;

        if (_attackTimer > 0f)
            return;

        _fireMode.Start();
        FireReadyShots(origin, nonTargetedAttack);

        if (!_fireMode.IsFiring)
            _attackTimer = _stats.AttackInterval;
    }

    private void FireReadyShots(Vector3 origin, ITargetedTowerAttack targetedAttack, Enemy firstTarget = null)
    {
        bool useFirstTarget = firstTarget != null && firstTarget.IsAlive;

        while (_fireMode.TryGetShot(out _))
        {
            Enemy target;

            if (useFirstTarget)
            {
                target = firstTarget;
                useFirstTarget = false;
            }
            else if (!_targetSelector.TryFindNearestTarget(origin, _stats.AttackRange, out target))
            {
                continue;
            }

            float damage = _stats.CalculateAttackDamage(Random.value, out bool isCritical);

#if UNITY_EDITOR
            TowerCombatDebugTrace.LogAttack(_attack.GetType().Name, damage, isCritical);
#endif

            targetedAttack.Attack(origin, target, _stats, damage);
        }
    }

    private void FireReadyShots(Vector3 origin, INonTargetedTowerAttack nonTargetedAttack)
    {
        while (_fireMode.TryGetShot(out _))
        {
            float damage = _stats.CalculateAttackDamage(Random.value, out bool isCritical);

#if UNITY_EDITOR
            TowerCombatDebugTrace.LogAttack(_attack.GetType().Name, damage, isCritical);
#endif

            nonTargetedAttack.Attack(origin, _stats, damage);
        }
    }

    private void SynchronizeAttackInterval()
    {
        float currentInterval = _stats.AttackInterval;

        if (Mathf.Approximately(currentInterval, _lastAttackInterval))
            return;

        if (_attackTimer > 0f && _lastAttackInterval > 0f)
            _attackTimer *= currentInterval / _lastAttackInterval;

        _lastAttackInterval = currentInterval;
    }
}
