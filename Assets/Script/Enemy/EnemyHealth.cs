using UnityEngine;
using System;

public class EnemyHealth
{
    public float MaxHp { get; }
    public float CurrentHp { get; private set; }
    public bool IsAlive => CurrentHp > 0f;

    public event Action Died;
    public event Action<float> Damaged;
    public event Action<float> Healed;

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

        CurrentHp = Mathf.Max(0f, CurrentHp - damage);
        Damaged?.Invoke(damage);

        if (CurrentHp <= 0f)
            Died?.Invoke();
    }

    public bool TryHeal(float amount)
    {
        if (!IsAlive || amount <= 0f || CurrentHp >= MaxHp)
            return false;

        float previousHp = CurrentHp;
        CurrentHp = Mathf.Min(MaxHp, CurrentHp + amount);
        Healed?.Invoke(CurrentHp - previousHp);
        return true;
    }
}
