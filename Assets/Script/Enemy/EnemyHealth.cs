using UnityEngine;
using System;

public class EnemyHealth
{
    public float MaxHp { get; }
    public float CurrentHp { get; private set; }
    public bool IsAlive => CurrentHp > 0f;

    public event Action Died;

    public EnemyHealth(float maxHp)
    {
        MaxHp = maxHp;
        CurrentHp = maxHp;
    }

    public void TakeDamage(float damage)
    {
        if (!IsAlive)
            return;

        if (damage <= 0f)
            return;

        CurrentHp -= damage;

        if (CurrentHp > 0f)
            return;

        CurrentHp = 0f;
        Died?.Invoke();
    }

    public bool TryHeal(float amount)
    {
        if (!IsAlive || amount <= 0f || CurrentHp >= MaxHp)
            return false;

        CurrentHp = Mathf.Min(MaxHp, CurrentHp + amount);
        return true;
    }
}
