using UnityEngine;

public interface INonTargetedTowerAttack : ITowerAttack
{
    void Attack(Vector3 origin, TowerStats stats);
}