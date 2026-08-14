using UnityEngine;

public class TowerStats
{
    public float AttackDamage { get; private set; }

    public float AttackRate { get; private set; }

    public float AttackRange { get; private set; }

    public TowerStats(float attackDamage, float attackRate, float attackRange)
    {
        AttackDamage = attackDamage;
        AttackRate = attackRate;
        AttackRange = attackRange;
    }
}