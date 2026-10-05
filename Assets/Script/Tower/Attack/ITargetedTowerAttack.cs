using UnityEngine;

public interface ITargetedTowerAttack : ITowerAttack
{
    void Attack(Vector3 origin, Enemy target, TowerStats stats);
}