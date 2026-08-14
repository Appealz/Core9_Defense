using UnityEngine;

public class TowerAttack
{
    private readonly TowerStats _stats;
    private readonly ITowerAttack _attack;

    private float _attackTimer;

    public TowerAttack(TowerStats stats, ITowerAttack attack)
    {
        _stats = stats;
        _attack = attack;
    }
}
