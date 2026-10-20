
using System;

public class TowerHealth
{
    public event Action Damaged;
    public event Action Died;

    public float MaxHp { get; }
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
}
