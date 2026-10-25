
using UnityEngine;

public interface IEnemyMovement
{
    bool Move(Transform transform, Vector3 targetPosition, float deltaTime, EnemyStats stats);
}
