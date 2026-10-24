
using UnityEngine;

public class StopAtRangeMovement : IEnemyMovement
{
    public void Move(Transform transform, Vector3 targetPosition, float deltaTime, EnemyStats stats)
    {
        Vector3 direction = targetPosition - transform.position;
        direction.z = 0f;

        float distance = direction.magnitude;
        float attackRange = Mathf.Max(0f, stats.AttackRange);

        if (distance <= attackRange)
            return;

        float moveDistance = Mathf.Min(Mathf.Max(0f, stats.MoveSpeed) * deltaTime, distance - attackRange);

        transform.position += direction.normalized * moveDistance;
    }
}
