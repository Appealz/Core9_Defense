
using UnityEngine;

public class ChaseMovement : IEnemyMovement
{
    public bool Move(Transform transform, Vector3 targetPosition, float deltaTime, EnemyStats stats)
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, stats.MoveSpeed * deltaTime);

        return transform.position == targetPosition;
    }
}
