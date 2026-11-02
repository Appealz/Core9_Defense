public readonly struct TowerStatValues
{
    public float AttackDamage { get; }
    public float AttackSpeed { get; }
    public float MaxHp { get; }

    public TowerStatValues(float attackDamage, float attackSpeed, float maxHp)
    {
        AttackDamage = attackDamage;
        AttackSpeed = attackSpeed;
        MaxHp = maxHp;
    }
}

public readonly struct TowerStatSummary
{
    public TowerStatValues Base { get; }
    public TowerStatValues NonCore { get; }
    public TowerStatValues Final { get; }

    public TowerStatSummary(
        TowerStatValues baseValues,
        TowerStatValues nonCoreValues,
        TowerStatValues finalValues)
    {
        Base = baseValues;
        NonCore = nonCoreValues;
        Final = finalValues;
    }
}
