using UnityEngine;

public class TowerAttack
{
    private const float TargetSearchInterval = 0.1f;

    private readonly TowerStats _stats;
    private readonly ITowerAttack _attack;
    private readonly IFireMode _fireMode;
    private readonly TowerTargetSelector _targetSelector;

    private float _attackTimer;

    public TowerAttack(TowerStats stats, ITowerAttack attack, IFireMode fireMode, TowerTargetSelector targetSelector)
    {
        _stats = stats;
        _attack = attack;
        _fireMode = fireMode;
        _targetSelector = targetSelector;
    }

    public void Update(Vector3 origin, float deltaTime)
    {
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
                _attackTimer = _stats.AttackRate;

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
            _attackTimer = _stats.AttackRate;
    }

    private void UpdateNonTargeted(Vector3 origin, float deltaTime, INonTargetedTowerAttack nonTargetedAttack)
    {
        if (_fireMode.IsFiring)
        {
            _fireMode.Update(deltaTime);
            FireReadyShots(origin, nonTargetedAttack);

            if (!_fireMode.IsFiring)
                _attackTimer = _stats.AttackRate;

            return;
        }

        _attackTimer -= deltaTime;

        if (_attackTimer > 0f)
            return;

        _fireMode.Start();
        FireReadyShots(origin, nonTargetedAttack);

        if (!_fireMode.IsFiring)
            _attackTimer = _stats.AttackRate;
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

            targetedAttack.Attack(origin, target, _stats);
        }
    }

    private void FireReadyShots(Vector3 origin, INonTargetedTowerAttack nonTargetedAttack)
    {
        while (_fireMode.TryGetShot(out _))
            nonTargetedAttack.Attack(origin, _stats);
    }
}