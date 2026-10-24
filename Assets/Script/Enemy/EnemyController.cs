using System;
using UnityEngine;

public class EnemyController
{
    private readonly EnemyStats _stats;
    private readonly IEnemyMovement _movement;
    private readonly IEnemyAttack _attack;
    private readonly CoreBoundary _coreBoundary;

    private float _attackCooldownRemaining;
    private bool _isCoreContact;

    public EnemyController(EnemyStats stats, IEnemyMovement movement, IEnemyAttack attack, CoreBoundary coreBoundary)
    {
        _stats = stats ?? throw new ArgumentNullException(nameof(stats));
        _movement = movement ?? throw new ArgumentNullException(nameof(movement));
        _attack = attack ?? throw new ArgumentNullException(nameof(attack));
        _coreBoundary = coreBoundary != null ? coreBoundary : throw new ArgumentNullException(nameof(coreBoundary));
    }

    public void EnemyUpdate(Transform transform, Vector3 targetPosition, float deltaTime)
    {
        if (_isCoreContact)
            return;

        _movement.Move(transform, targetPosition, deltaTime, _stats);

        if (!_attack.IsRanged)
            return;

        float attackRange = Mathf.Max(0f, _stats.AttackRange);

        if ((transform.position - targetPosition).sqrMagnitude > attackRange * attackRange)
            return;

        _attackCooldownRemaining -= deltaTime;

        if (_attackCooldownRemaining > 0f || !_coreBoundary.CanReceiveDamage)
            return;

        _attack.Attack(_stats.AttackDamage);
        _attackCooldownRemaining = Mathf.Max(0.01f, _stats.AttackCooldown);
    }

    public void SetCoreContact(bool isContact)
    {
        _isCoreContact = isContact;
    }
}
