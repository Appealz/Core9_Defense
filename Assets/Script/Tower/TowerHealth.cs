using UnityEngine;

public class TowerHealth
{
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
        if (!IsAlive)
            return;

        if (damage <= 0f)
            return;

        CurrentHp -= damage;

        if (CurrentHp < 0f)
            CurrentHp = 0f;
    }
}
