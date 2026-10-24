using UnityEngine;



public interface IEnemyAttack
{
    bool IsRanged { get; }

    void Attack(float damage);
}

