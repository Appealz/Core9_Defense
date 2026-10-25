
using UnityEngine;

public class StopAtRangeMovement : IEnemyMovement
{
    private readonly float? _stopDistance;

    public StopAtRangeMovement()
    {
    }

    public StopAtRangeMovement(float stopDistance)
    {
        _stopDistance = Mathf.Max(0f, stopDistance);
    }

    public bool Move(Transform transform, Vector3 targetPosition, float deltaTime, EnemyStats stats)
    {
        Vector3 direction = targetPosition - transform.position;
        direction.z = 0f;

        float distance = direction.magnitude;
        float stopDistance = _stopDistance ?? Mathf.Max(0f, stats.AttackRange);

        if (distance <= stopDistance)
            return true;

        float remainingDistance = distance - stopDistance;
        float moveDistance = Mathf.Min(Mathf.Max(0f, stats.MoveSpeed) * deltaTime, remainingDistance);

        transform.position += direction.normalized * moveDistance;
        return moveDistance >= remainingDistance;
    }
}
