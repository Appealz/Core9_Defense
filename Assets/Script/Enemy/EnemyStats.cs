using UnityEngine;

public class EnemyStats
{
    public float MoveSpeed { get; private set; }
    public float AttackDamage { get; private set; }
    public float AttackRange { get; private set; }
    public float AttackCooldown { get; private set; }

    public EnemyStats(float moveSpeed, float attackDamage, float attackRange, float attackCooldown)
    {
        MoveSpeed = moveSpeed;
        AttackDamage = attackDamage;
        AttackRange = attackRange;
        AttackCooldown = attackCooldown;
    }
}
