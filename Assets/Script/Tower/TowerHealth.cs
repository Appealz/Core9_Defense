
using System;

public class TowerHealth
{
    public event Action Damaged;
    public event Action Died;

    public float MaxHp { get; private set; }
    public float CurrentHp { get; private set; }

    public bool IsAlive => CurrentHp > 0f;

    public TowerHealth(float maxHp)
    {
        MaxHp = maxHp;
        CurrentHp = maxHp;
    }

    public void TakeDamage(float damage)
    {
        if (!IsAlive || damage <= 0f)
            return;

        CurrentHp = Math.Max(0f, CurrentHp - damage);

        Damaged?.Invoke();

        if (!IsAlive)
            Died?.Invoke();
    }

    public void UpdateMaxHp(float maxHp)
    {
        float nextMaxHp = Math.Max(1f, maxHp);
        float previousMaxHp = MaxHp;
        bool wasAlive = IsAlive;

        MaxHp = nextMaxHp;

        if (!wasAlive)
            return;

        if (nextMaxHp > previousMaxHp)
            CurrentHp += nextMaxHp - previousMaxHp;

        CurrentHp = Math.Min(CurrentHp, MaxHp);
    }

    public bool Heal(float amount)
    {
        if (!IsAlive || amount <= 0f || CurrentHp >= MaxHp)
            return false;

        float previousHp = CurrentHp;
        CurrentHp = Math.Min(MaxHp, CurrentHp + amount);
        return CurrentHp > previousHp;
    }
}
