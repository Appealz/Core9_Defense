public class Tower
{
    public TowerHealth Health { get; }

    public TowerStats Stats { get; }

    public TowerAttack Attack { get; }

    public Tower(TowerHealth health, TowerStats stats, TowerAttack attack)
    {
        Health = health;
        Stats = stats;
        Attack = attack;
    }
}