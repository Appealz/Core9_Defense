using UnityEngine;

public class Tower : MonoBehaviour
{
    public TowerHealth Health { get; private set; }
    public TowerStats Stats { get; private set; }
    public TowerAttack Attack { get; private set; }

    public void Initialize(TowerHealth health, TowerStats stats, TowerAttack attack)
    {
        Health = health;
        Stats = stats;
        Attack = attack;
    }
}